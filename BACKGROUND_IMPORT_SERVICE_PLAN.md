# Background Import Service Implementation Plan

## Overview

This document outlines a phased approach to implement a background service that imports pressure map data from CSV files in `Data_Imports/` folder into the SQL LocalDB database, making it available for the alert system.

---

## Current System Analysis

### Existing Components (Will Be Reused)
| Component | Location | Purpose |
|-----------|----------|---------|
| `ApplicationDbContext` | `ApplicationDbContext.cs` | EF Core database context with `SensorData`, `Alerts`, `RiskPredictions` |
| `IPressureMapRepository` | `Repositories/IPressureMapRepository.cs` | Data access for pressure maps (`AddAsync`, `AddRangeAsync`) |
| `IPressureMapAnalysisService` | `Services/IPressureMapAnalysisService.cs` | Analyzes 32x32 matrices, detects clusters |
| `IRiskPredictionService` | `Services/IRiskPredictionService.cs` | Generates risk predictions from maps |
| `IAlertsRepository` | `Repositories/IAlertsRepository.cs` | Stores generated alerts |
| `PressureMap` model | `Models/PressureMap.cs` | 32x32 matrix with `Scale` and `Unit` (mmHg) |
| `PressureMetrics` model | `Models/PressureMetrics.cs` | Cluster analysis results |

### CSV File Structure
- **Location**: `Sensore Project/Data_Imports/`
- **File count**: 15 files
- **File size**: ~10 MB each
- **Format**: Each file contains multiple 32x32 pressure maps
  - 32 consecutive rows = 1 pressure map
  - Each row has 32 comma-separated integer values (0-255)
  - No separator between maps (continuous rows)
- **Naming pattern**: `{deviceId}_{YYYYMMDD}.csv` (e.g., `1c0fd777_20251011.csv`)
- **Total lines per file**: ~142,000 lines → ~4,400 pressure maps per file

### UI Entry Point
- **Home page**: `Views/Home/Index.cshtml`
- **Controller**: `Controllers/HomeController.cs`
- **Current features**: Pressure chart, alerts table, auto-refresh every 5 seconds

---

## Phase 1: Database Schema for Import Jobs

### Goal
Create a table to track import job status and progress.

### New Files to Create
1. `Models/ImportJob.cs` - Entity model for import jobs

### Files to Modify
1. `ApplicationDbContext.cs` - Add `DbSet<ImportJob>`

### ImportJob Model Schema
```
ImportJob
├── Id (int, PK)
├── Status (string): "Pending" | "Processing" | "Completed" | "Failed" | "Cancelled"
├── CreatedAt (DateTime)
├── StartedAt (DateTime?)
├── CompletedAt (DateTime?)
├── TotalFiles (int)
├── ProcessedFiles (int)
├── TotalMaps (int)
├── ProcessedMaps (int)
├── CurrentFileName (string?)
├── ErrorMessage (string?)
└── ProcessedFilesList (string?) - JSON array of processed file names
```

### Migration Required
- Create migration: `dotnet ef migrations add AddImportJobTable`
- Apply migration: `dotnet ef database update`

### Verification
- [ ] ImportJob table exists in database
- [ ] Can insert/update/query ImportJob records
- [ ] Existing tables (SensorData, Alerts, RiskPredictions) unaffected

---

## Phase 2: Import Job Repository

### Goal
Create data access layer for import jobs.

### New Files to Create
1. `Repositories/IImportJobRepository.cs` - Interface
2. `Repositories/ImportJobRepository.cs` - Implementation

### Interface Methods
```csharp
public interface IImportJobRepository
{
    Task<ImportJob?> GetLatestAsync();
    Task<ImportJob?> GetByIdAsync(int id);
    Task<ImportJob?> GetPendingAsync();
    Task<ImportJob> CreateAsync();
    Task UpdateStatusAsync(int id, string status);
    Task UpdateProgressAsync(int id, int processedFiles, int processedMaps, string? currentFileName);
    Task MarkCompletedAsync(int id);
    Task MarkFailedAsync(int id, string errorMessage);
    Task<bool> HasPendingOrRunningJobAsync();
}
```

### Files to Modify
1. `Program.cs` - Register `IImportJobRepository` in DI container

### Verification
- [ ] Repository methods work correctly
- [ ] DI registration successful
- [ ] No impact on existing repositories

---

## Phase 3: CSV Parsing Service

### Goal
Create a service to parse CSV files into pressure maps with streaming support for large files.

### New Files to Create
1. `Services/ICsvPressureMapParser.cs` - Interface
2. `Services/CsvPressureMapParser.cs` - Implementation

### Key Design Decisions
- **Streaming**: Use `StreamReader` to read line-by-line (memory efficient for 10MB files)
- **Batching**: Yield pressure maps in batches (configurable, default 100)
- **Validation**: Skip invalid rows (not 32 columns, non-numeric values)
- **Timestamp inference**: Extract date from filename (`{deviceId}_{YYYYMMDD}.csv`)

### Interface Methods
```csharp
public interface ICsvPressureMapParser
{
    IAsyncEnumerable<PressureMapBatch> ParseFileAsync(string filePath, int batchSize = 100, CancellationToken ct = default);
    (string DeviceId, DateTime Date) ParseFileName(string fileName);
}

public class PressureMapBatch
{
    public List<PressureMap> Maps { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
}
```

### Files to Modify
1. `Program.cs` - Register `ICsvPressureMapParser` in DI container

### Verification
- [ ] Can parse a single CSV file correctly
- [ ] Memory usage stays bounded during parsing
- [ ] Invalid rows are logged and skipped
- [ ] Batch yielding works correctly

---

## Phase 4: Background Import Worker Service

### Goal
Create a hosted background service that processes import jobs.

### New Files to Create
1. `Services/PressureMapImportWorker.cs` - BackgroundService implementation

### Key Design Decisions
- **Polling interval**: Check for pending jobs every 10 seconds
- **Batch processing**: Process 100 maps at a time, then save to DB
- **Cancellation support**: Respond to application shutdown gracefully
- **Error handling**: Log errors, mark job as failed, continue with next file
- **Progress tracking**: Update ImportJob record after each batch
- **Duplicate prevention**: Track processed files in ImportJob.ProcessedFilesList

### Processing Flow
```
1. Poll for pending ImportJob
2. If found:
   a. Mark job as "Processing"
   b. Get list of CSV files in Data_Imports/
   c. Filter out already-processed files
   d. For each file:
      i.   Update CurrentFileName
      ii.  Parse CSV in batches
      iii. For each batch:
           - Create SensorData entities with PressureMap
           - Run PressureMapAnalysisService to get metrics
           - Run RiskPredictionService if needed
           - Create alerts if metrics.AlertGenerated
           - Save batch to database
           - Update ProcessedMaps count
      iv.  Add file to ProcessedFilesList
      v.   Update ProcessedFiles count
   e. Mark job as "Completed"
3. If error:
   a. Log exception
   b. Mark job as "Failed" with error message
4. Wait 10 seconds, repeat
```

### Files to Modify
1. `Program.cs` - Register `PressureMapImportWorker` as hosted service

### Verification
- [ ] Worker starts with application
- [ ] Worker stops gracefully on shutdown
- [ ] Processing works end-to-end with test data
- [ ] Progress updates are visible in database
- [ ] Errors are handled and logged

---

## Phase 5: Admin API Controller

### Goal
Create API endpoints to trigger and monitor import jobs.

### New Files to Create
1. `Controllers/ImportController.cs` - API controller

### Endpoints
| Method | Route | Purpose |
|--------|-------|---------|
| POST | `/api/import/start` | Create new import job (if none pending/running) |
| GET | `/api/import/status` | Get latest job status and progress |
| GET | `/api/import/status/{id}` | Get specific job status |
| POST | `/api/import/cancel/{id}` | Cancel a running job (future enhancement) |

### Response DTOs
```csharp
public class ImportStatusResponse
{
    public int Id { get; set; }
    public string Status { get; set; }
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public int TotalMaps { get; set; }
    public int ProcessedMaps { get; set; }
    public string? CurrentFileName { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

### Verification
- [ ] POST /api/import/start creates job and returns ID
- [ ] GET /api/import/status returns current progress
- [ ] Cannot start new job if one is already running
- [ ] Swagger documentation shows endpoints

---

## Phase 6: UI Integration (Home Page)

### Goal
Add import button and progress display to the home page.

### Files to Modify
1. `Views/Home/Index.cshtml` - Add UI elements and JavaScript

### UI Elements to Add
1. **Import Section** (new card above or below existing cards)
   - "Import Pressure Maps" button
   - Progress bar (hidden when no job running)
   - Status text (files processed, maps processed)
   - Error message display (if failed)

### JavaScript Functions
```javascript
async function startImport() {
    // POST to /api/import/start
    // Show progress section
    // Start polling for status
}

async function pollImportStatus() {
    // GET /api/import/status
    // Update progress bar and text
    // Stop polling when completed/failed
}
```

### UI States
1. **Idle**: Show "Import Pressure Maps" button
2. **Processing**: Show progress bar, disable button, show current file
3. **Completed**: Show success message, re-enable button
4. **Failed**: Show error message, re-enable button

### Verification
- [ ] Button triggers import
- [ ] Progress updates in real-time
- [ ] Completion/failure states display correctly
- [ ] Existing dashboard functionality unaffected
- [ ] Page refresh shows correct state

---

## Phase 7: Testing & Validation

### Unit Tests to Add
1. `CsvPressureMapParserTests.cs`
   - Test parsing valid CSV
   - Test handling invalid rows
   - Test filename parsing
   - Test batch yielding

2. `ImportJobRepositoryTests.cs`
   - Test CRUD operations
   - Test status transitions

3. `PressureMapImportWorkerTests.cs`
   - Test job processing flow
   - Test error handling
   - Test cancellation

### Integration Tests
1. End-to-end import with sample CSV file
2. Verify SensorData, Alerts, RiskPredictions created correctly
3. Verify progress tracking accuracy

### Manual Testing Checklist
- [ ] Import small CSV file (< 100 maps)
- [ ] Import full-size CSV file (~10 MB)
- [ ] Import all 15 files sequentially
- [ ] Verify alerts generated for high-pressure maps
- [ ] Verify existing dashboard shows imported data
- [ ] Test app restart during import (job should resume or be marked failed)

---

## File Summary

### New Files (8 files)
| File | Phase |
|------|-------|
| `Models/ImportJob.cs` | Phase 1 |
| `Repositories/IImportJobRepository.cs` | Phase 2 |
| `Repositories/ImportJobRepository.cs` | Phase 2 |
| `Services/ICsvPressureMapParser.cs` | Phase 3 |
| `Services/CsvPressureMapParser.cs` | Phase 3 |
| `Services/PressureMapImportWorker.cs` | Phase 4 |
| `Controllers/ImportController.cs` | Phase 5 |
| `Migrations/YYYYMMDD_AddImportJobTable.cs` | Phase 1 (auto-generated) |

### Modified Files (3 files)
| File | Phase | Changes |
|------|-------|---------|
| `ApplicationDbContext.cs` | Phase 1 | Add `DbSet<ImportJob>` |
| `Program.cs` | Phase 2-4 | Register new services and hosted worker |
| `Views/Home/Index.cshtml` | Phase 6 | Add import UI section |

---

## Risk Mitigation

### Large File Handling (~10 MB)
- **Risk**: Memory exhaustion
- **Mitigation**: Stream-based parsing, batch processing (100 maps at a time)

### Long-Running Operations
- **Risk**: HTTP timeout, UI freeze
- **Mitigation**: Background service, async polling, immediate API response

### Database Performance
- **Risk**: Slow inserts, table locks
- **Mitigation**: Batch inserts (100 records), use `AddRangeAsync`

### Duplicate Imports
- **Risk**: Same file imported multiple times
- **Mitigation**: Track processed files in ImportJob.ProcessedFilesList

### Application Restart
- **Risk**: Job stuck in "Processing" state
- **Mitigation**: On startup, check for stale "Processing" jobs and mark as "Failed"

### Existing System Impact
- **Risk**: Breaking existing functionality
- **Mitigation**: 
  - All new code in separate files
  - Minimal changes to existing files
  - Reuse existing services (PressureMapAnalysisService, RiskPredictionService)
  - Thorough testing before deployment

---

## Design Decisions (Confirmed)

1. **Timestamp handling**: Each pressure map gets a unique timestamp (incremented from file date)

2. **Device ID**: Not stored (filename prefix ignored)

3. **Alert generation**: Alerts NOT generated during import - only raw data stored. Existing system handles alerts on demand.

4. **Re-import behavior**: Skip already-imported files automatically

5. **Progress granularity**: Show both per-file and per-map progress

---

## Implementation Progress

| Phase | Status |
|-------|--------|
| Phase 1: Database Schema | ✅ Completed |
| Phase 2: Import Job Repository | ✅ Completed |
| Phase 3: CSV Parsing Service | ✅ Completed |
| Phase 4: Background Worker | ✅ Completed |
| Phase 5: Admin API Controller | ✅ Completed |
| Phase 6: UI Integration | ⏳ Pending |
| Phase 7: Testing & Validation | ⏳ Pending |
