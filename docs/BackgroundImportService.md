# Background Job for CSV Ingestion and Database Population

## Overview

The Smart Pressure Monitoring system includes a background service that automatically imports pressure map data from CSV files into the database. This service runs continuously, polling for new import jobs and processing CSV files containing 32x32 pressure matrices.

## Architecture

### Components

```
┌─────────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│  ImportController   │────▶│  ImportJobRepository │────▶│  Database       │
│  (Start Job)        │     │                      │     │  (ImportJobs)   │
└─────────────────────┘     └──────────────────────┘     └─────────────────┘
                                      │
                                      ▼
┌─────────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│  PressureMap        │◀────│  CsvPressureMap      │◀────│  CSV Files      │
│  ImportWorker       │     │  Parser              │     │  (Data_Imports) │
│  (Background)       │     │                      │     │                 │
└─────────────────────┘     └──────────────────────┘     └─────────────────┘
         │
         ▼
┌─────────────────────┐
│  PressureMap        │
│  Repository         │
└─────────────────────┘
```

### Key Classes

| Class | Location | Purpose |
|-------|----------|---------|
| `PressureMapImportWorker` | `Services/PressureMapImportWorker.cs` | Background service that processes import jobs |
| `CsvPressureMapParser` | `Services/CsvPressureMapParser.cs` | Parses CSV files into pressure maps |
| `ImportJob` | `Models/ImportJob.cs` | Entity tracking import job status |
| `ImportJobRepository` | `Repositories/ImportJobRepository.cs` | Data access for import jobs |
| `ImportController` | `Controllers/ImportController.cs` | API endpoints for job management |

## Data Models

### ImportJob Entity

```csharp
public class ImportJob
{
    public int Id { get; set; }
    public string Status { get; set; }              // Pending, Processing, Completed, Failed
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public int TotalMaps { get; set; }
    public int ProcessedMaps { get; set; }
    public string? CurrentFileName { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ProcessedFilesList { get; set; } // JSON array of processed filenames
}
```

### Job Status Constants

```csharp
public static class ImportJobStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
}
```

## CSV File Format

### File Naming Convention

```
{deviceId}_{YYYYMMDD}.csv
```

**Examples:**
- `1c0fd777_20251011.csv`
- `sensor01_20251205.csv`

### File Structure

Each CSV file contains multiple 32x32 pressure maps stacked vertically:
- Each row has 32 comma-separated integer values (0-255)
- Every 32 rows form one complete pressure map
- No headers in the file

**Example (partial):**
```csv
45,52,48,51,49,50,47,53,...(32 values total)
48,55,52,49,51,48,50,52,...
...
(32 rows = 1 pressure map)
120,125,130,128,135,140,145,150,...
...
(next 32 rows = another pressure map)
```

### Data Location

CSV files should be placed in:
```
{ProjectRoot}/Data_Imports/
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/import/start` | Start a new import job |
| `GET` | `/api/import/status` | Get latest job status |
| `GET` | `/api/import/status/{id}` | Get specific job status |

## Usage Examples

### Start an Import Job

```http
POST /api/import/start
```

**Success Response (200 OK):**
```json
{
  "id": 1,
  "status": "Pending",
  "totalFiles": 0,
  "processedFiles": 0,
  "totalMaps": 0,
  "processedMaps": 0,
  "currentFileName": null,
  "errorMessage": null,
  "createdAt": "2025-12-05T10:00:00Z",
  "startedAt": null,
  "completedAt": null
}
```

**Conflict Response (409) - Job Already Running:**
```json
{
  "message": "An import job is already pending or in progress.",
  "jobId": 1
}
```

### Check Import Status

```http
GET /api/import/status
```

**Response (In Progress):**
```json
{
  "id": 1,
  "status": "Processing",
  "totalFiles": 15,
  "processedFiles": 5,
  "totalMaps": 4500,
  "processedMaps": 1500,
  "currentFileName": "1c0fd777_20251013.csv",
  "errorMessage": null,
  "createdAt": "2025-12-05T10:00:00Z",
  "startedAt": "2025-12-05T10:00:05Z",
  "completedAt": null
}
```

**Response (Completed):**
```json
{
  "id": 1,
  "status": "Completed",
  "totalFiles": 15,
  "processedFiles": 15,
  "totalMaps": 4500,
  "processedMaps": 4500,
  "currentFileName": null,
  "errorMessage": null,
  "createdAt": "2025-12-05T10:00:00Z",
  "startedAt": "2025-12-05T10:00:05Z",
  "completedAt": "2025-12-05T10:15:30Z"
}
```

## Background Worker Details

### Service Registration

The worker is registered in `Program.cs`:

```csharp
builder.Services.AddHostedService<PressureMapImportWorker>();
```

### Processing Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    Worker Startup                            │
├─────────────────────────────────────────────────────────────┤
│  1. Mark any stale "Processing" jobs as Failed              │
│  2. Enter polling loop (every 10 seconds)                   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Poll for Pending Job                      │
├─────────────────────────────────────────────────────────────┤
│  - Check for job with Status = "Pending"                    │
│  - If none found, wait and poll again                       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Process Job                               │
├─────────────────────────────────────────────────────────────┤
│  1. Update status to "Processing"                           │
│  2. Scan Data_Imports folder for CSV files                  │
│  3. Filter out already-processed files                      │
│  4. Count total maps across all files                       │
│  5. Process each file in batches of 100 maps                │
│  6. Update progress after each batch                        │
│  7. Mark job as Completed or Failed                         │
└─────────────────────────────────────────────────────────────┘
```

### Batch Processing

Files are processed in batches to:
- Reduce memory usage for large files (~10MB each)
- Allow progress tracking
- Support graceful cancellation

```csharp
private const int BatchSize = 100;  // Maps per batch
```

### Resumable Imports

The system tracks processed files in `ProcessedFilesList` (JSON array), allowing:
- Resumption after application restart
- Skipping already-imported files
- Re-running imports without duplicates

### Error Handling

| Scenario | Behavior |
|----------|----------|
| Application restart during processing | Job marked as Failed on next startup |
| File not found | Job marked as Failed with error message |
| Invalid CSV data | Row skipped, processing continues |
| Cancellation requested | Job marked as Failed with "cancelled" message |

## CSV Parser Details

### Parsing Logic

```csharp
public async IAsyncEnumerable<PressureMapBatch> ParseFileAsync(
    string filePath,
    int batchSize = 100,
    CancellationToken cancellationToken = default)
```

**Key Features:**
- Streaming parser (doesn't load entire file into memory)
- Validates each row has exactly 32 values
- Clamps values to 0-255 range
- Generates unique timestamps (1 second apart) based on filename date
- Yields batches for efficient database insertion

### Timestamp Generation

Timestamps are derived from the filename date:
```
1c0fd777_20251011.csv → Base date: 2025-10-11 00:00:00 UTC
Map 0 → 2025-10-11 00:00:00
Map 1 → 2025-10-11 00:00:01
Map 2 → 2025-10-11 00:00:02
...
```

### Validation

- Matrix must be exactly 32×32
- All values must be numeric
- Values are clamped to 0-255 range

## Database Schema

### ImportJobs Table

```sql
CREATE TABLE ImportJobs (
    Id INT PRIMARY KEY IDENTITY,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    CreatedAt DATETIME2 NOT NULL,
    StartedAt DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    TotalFiles INT NOT NULL DEFAULT 0,
    ProcessedFiles INT NOT NULL DEFAULT 0,
    TotalMaps INT NOT NULL DEFAULT 0,
    ProcessedMaps INT NOT NULL DEFAULT 0,
    CurrentFileName NVARCHAR(255) NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    ProcessedFilesList NVARCHAR(MAX) NULL
);
```

### SensorData Table (Target)

```sql
CREATE TABLE SensorData (
    Id INT PRIMARY KEY IDENTITY,
    Pressure FLOAT NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    PressureMapJson NVARCHAR(MAX) NULL,
    RequiresClinicianReview BIT NOT NULL DEFAULT 0,
    MetricsJson NVARCHAR(MAX) NULL
);
```

## Configuration

### Polling Interval

```csharp
private const int PollingIntervalSeconds = 10;
```

### Batch Size

```csharp
private const int BatchSize = 100;
```

### Data Import Folder

```csharp
var importFolder = Path.Combine(_environment.ContentRootPath, "Data_Imports");
```

## Monitoring & Logging

The service logs key events:

| Event | Log Level |
|-------|-----------|
| Worker started/stopped | Information |
| Processing job started | Information |
| File processing started/completed | Information |
| Stale job marked as failed | Warning |
| Invalid pressure map | Warning |
| Job failed | Error |
| Unrecoverable error | Error |

**Example Log Output:**
```
info: PressureMapImportWorker[0]
      PressureMapImportWorker started
info: PressureMapImportWorker[0]
      Processing import job 1
info: PressureMapImportWorker[0]
      Processing file: 1c0fd777_20251011.csv
info: CsvPressureMapParser[0]
      Parsed 300 pressure maps from 1c0fd777_20251011.csv
info: PressureMapImportWorker[0]
      Completed file: 1c0fd777_20251011.csv
info: PressureMapImportWorker[0]
      Import job 1 completed. Processed 15 files, 4500 maps
```

## Performance Considerations

### Memory Usage

- Streaming CSV parser minimizes memory footprint
- Batch processing limits concurrent database operations
- Each batch is processed in a separate DI scope

### Database Performance

- Bulk inserts via `AddRangeAsync` for efficiency
- No metrics calculation during import (deferred to alert scanner)
- Indexes recommended on `Timestamp` and `PressureMapJson IS NOT NULL`

### Estimated Processing Time

| File Size | Maps per File | Approximate Time |
|-----------|---------------|------------------|
| ~10 MB | ~300 maps | ~5-10 seconds |
| 15 files | ~4500 maps | ~2-3 minutes |

## Workflow

### Typical Import Workflow

1. **Place CSV Files** → Copy files to `Data_Imports` folder
2. **Start Import** → Call `POST /api/import/start`
3. **Monitor Progress** → Poll `GET /api/import/status`
4. **Scan for Alerts** → Call `POST /api/alerts/scan` after import completes
5. **Review Alerts** → Clinicians review generated alerts

### Integration with Alert Scanner

After import completes, run the alert scanner to:
- Analyze imported pressure maps
- Generate alerts for high-risk maps
- Flag maps requiring clinician review

```http
POST /api/alerts/scan
```

## Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Job stuck in "Processing" | Application crashed | Restart application (auto-recovers) |
| No files found | Wrong folder | Check `Data_Imports` folder exists |
| Invalid CSV format | Wrong column count | Ensure 32 columns per row |
| Duplicate data | Re-running import | System skips already-processed files |

### Checking Job History

```http
GET /api/import/status/{jobId}
```