# Pressure Map Alert Scanner Plan

This document outlines a structured, phase-wise approach to implement a scanner that analyzes stored pressure maps and creates alerts based on risk thresholds.

---

## Overview

**Goal**: Scan existing `SensorData` rows with pressure maps, analyze them using `PressureMapAnalysisService`, and create alerts for maps that meet risk criteria.

**Trigger**: Manual button click ("Scan for Alerts") on the Alerts page UI.

**Alert Strategy**: One alert per map (summarizing overall risk, not per-cluster).

---

## Current State Analysis

### Existing Components

| Component | File | Purpose |
|-----------|------|---------|
| `PressureMapAnalysisService` | `Services/PressureMapAnalysisService.cs` | Analyzes 32×32 maps, detects clusters, computes risk |
| `ShouldGenerateAlert()` | `Services/PressureMapAnalysisService.cs#184-209` | Rule-based decision (Critical ≥75, High+coverage, etc.) |
| `SensorData` | `Models/SensorData.cs` | Stores `PressureMapJson`, `MetricsJson` |
| `PressureMetrics` | `Models/PressureMetrics.cs` | Has `AlertGenerated`, `AlertTimestamp` flags |
| `Alert` | `Models/Alert.cs` | Has `PressureMapId`, `AlertType`, `ClusterInfoJson` |
| `IAlertsRepository` | `Repositories/IAlertsRepository.cs` | `AddAsync()`, `GetByPressureMapIdAsync()` |
| `IPressureMapRepository` | `Repositories/IPressureMapRepository.cs` | `GetRecentWithMapsAsync()`, `UpdateAsync()` |
| `AlertsController` | `Controllers/AlertController.cs` | API endpoints for alerts |
| `AlertPage/Index.cshtml` | `Views/AlertPage/Index.cshtml` | Alerts dashboard UI |

### Key Observations

1. `PressureMetrics.AlertGenerated` already exists to track if an alert was created.
2. `PressureMetrics.AlertTimestamp` can store when the alert was generated.
3. `Alert.PressureMapId` links an alert to a specific `SensorData` row.
4. `IAlertsRepository.GetByPressureMapIdAsync()` can check if an alert already exists for a map.
5. No existing method to get maps that need scanning (need to add).

---

## Phase-Wise Implementation

### Phase 1: Repository Extensions

**Goal**: Add methods to query maps needing alert review and check for existing alerts.

#### New Methods in `IPressureMapRepository`

```csharp
// Get maps with PressureMapJson that either:
// - Have no MetricsJson (never analyzed), OR
// - Have Metrics.AlertGenerated == false (analyzed but no alert created yet)
Task<List<SensorData>> GetMapsNeedingAlertScanAsync(int batchSize = 100);

// Update metrics after scan
Task UpdateMetricsAsync(int id, PressureMetrics metrics);
```

#### New Methods in `IAlertsRepository`

```csharp
// Check if an alert already exists for a given pressure map
Task<bool> AlertExistsForMapAsync(int pressureMapId);
```

#### Files to Modify
- `Repositories/IPressureMapRepository.cs`
- `Repositories/PressureMapRepository.cs`
- `Repositories/IAlertsRepository.cs`
- `Repositories/AlertsRepository.cs`

---

### Phase 2: Scanner Service

**Goal**: Create a service that scans maps and creates alerts.

#### New Interface: `IPressureMapAlertScanner`

```csharp
public interface IPressureMapAlertScanner
{
    Task<ScanResult> ScanAsync(CancellationToken cancellationToken = default);
}

public class ScanResult
{
    public int TotalMapsScanned { get; set; }
    public int AlertsCreated { get; set; }
    public int MapsSkipped { get; set; }  // Already had alerts
    public int Errors { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
    public bool IsComplete { get; set; }
}
```

#### Implementation: `PressureMapAlertScanner`

**Logic Flow:**
1. Fetch batch of maps needing scan via `GetMapsNeedingAlertScanAsync()`.
2. For each map:
   a. Deserialize `PressureMap` from JSON.
   b. Check if alert already exists via `AlertExistsForMapAsync()`.
   c. If exists, skip and mark metrics as `AlertGenerated = true`.
   d. Run `AnalyzePressureMap()` to get `PressureMetrics`.
   e. Check `ShouldGenerateAlert(metrics)`.
   f. If true, create `Alert` with:
      - `AlertType = "HighPressureCluster"`
      - `Message` = descriptive text (risk level, cluster count)
      - `Pressure` = max pressure from clusters
      - `PressureMapId` = SensorData.Id
      - `ClusterInfo` = summary of clusters
   g. Update `SensorData.Metrics` with `AlertGenerated = true`, `AlertTimestamp = now`.
3. Return `ScanResult` with counts.

#### Error Handling
- Wrap each map processing in try/catch.
- Log errors but continue scanning.
- Collect error messages in result.

#### Files to Create
- `Services/IPressureMapAlertScanner.cs`
- `Services/PressureMapAlertScanner.cs`

#### Files to Modify
- `Program.cs` – Register scanner in DI

---

### Phase 3: API Endpoint

**Goal**: Add an endpoint to trigger the scan and return results.

#### New Endpoint in `AlertsController`

```csharp
// POST: api/alerts/scan
[HttpPost("scan")]
public async Task<IActionResult> ScanForAlerts(CancellationToken cancellationToken)
{
    var result = await _scanner.ScanAsync(cancellationToken);
    return Ok(result);
}
```

#### Response DTO: `ScanResultResponse`

```csharp
public class ScanResultResponse
{
    public int TotalMapsScanned { get; set; }
    public int AlertsCreated { get; set; }
    public int MapsSkipped { get; set; }
    public int Errors { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
    public bool IsComplete { get; set; }
    public string Message { get; set; }  // Summary message
}
```

#### Files to Modify
- `Controllers/AlertController.cs`

#### Files to Create
- `Models/DTOs/ScanResultResponse.cs`

---

### Phase 4: UI Integration

**Goal**: Add "Scan for Alerts" button to Alerts page with progress feedback.

#### UI Changes

1. Add button in filter bar (next to Refresh):
   ```html
   <button class="btn btn-warning btn-sm" id="scanBtn">
       🔍 Scan for Alerts
   </button>
   ```

2. Add scan status display:
   ```html
   <div id="scanStatus" class="alert alert-info d-none">
       <span id="scanStatusText">Scanning...</span>
   </div>
   ```

3. Add JavaScript:
   ```javascript
   async function scanForAlerts() {
       // Disable button, show spinner
       // Call POST /api/alerts/scan
       // Show result (alerts created, errors)
       // Refresh dashboard
   }
   ```

#### Files to Modify
- `Views/AlertPage/Index.cshtml`

---

### Phase 5: Testing & Validation

**Goal**: Verify scanner works correctly without breaking existing functionality.

#### Test Cases

1. **No maps to scan**: Scanner returns 0 scanned, 0 created.
2. **Maps without metrics**: Scanner analyzes and creates alerts if warranted.
3. **Maps with existing alerts**: Scanner skips and marks as processed.
4. **Low-risk maps**: Scanner analyzes but doesn't create alerts.
5. **High-risk maps**: Scanner creates alerts with correct data.
6. **Invalid map data**: Scanner logs error and continues.
7. **Button click**: UI shows progress and result.
8. **Dashboard refresh**: New alerts appear after scan.
9. **Alert detail modal**: Heatmap renders for scanner-created alerts.
10. **Existing API endpoints**: Still function correctly.

---

## File Summary

### New Files (4 files)

| File | Phase |
|------|-------|
| `Services/IPressureMapAlertScanner.cs` | Phase 2 |
| `Services/PressureMapAlertScanner.cs` | Phase 2 |
| `Models/DTOs/ScanResultResponse.cs` | Phase 3 |
| `Models/ScanResult.cs` | Phase 2 |

### Modified Files (6 files)

| File | Phase | Changes |
|------|-------|---------|
| `Repositories/IPressureMapRepository.cs` | Phase 1 | Add `GetMapsNeedingAlertScanAsync`, `UpdateMetricsAsync` |
| `Repositories/PressureMapRepository.cs` | Phase 1 | Implement new methods |
| `Repositories/IAlertsRepository.cs` | Phase 1 | Add `AlertExistsForMapAsync` |
| `Repositories/AlertsRepository.cs` | Phase 1 | Implement new method |
| `Controllers/AlertController.cs` | Phase 3 | Add `POST /api/alerts/scan` endpoint |
| `Views/AlertPage/Index.cshtml` | Phase 4 | Add scan button and status UI |
| `Program.cs` | Phase 2 | Register `IPressureMapAlertScanner` |

---

## Alert Message Format

When an alert is created, the message will follow this format:

```
[RiskLevel] risk detected: [ClusterCount] high-pressure cluster(s) found. 
Max pressure: [MaxPressure]. Coverage: [TotalHighPixels] pixels.
```

Example:
```
High risk detected: 2 high-pressure cluster(s) found. 
Max pressure: 245. Coverage: 85 pixels.
```

---

## Risk Mitigation

### Breaking Existing Functionality
- **Risk**: Modifying repositories breaks existing queries.
- **Mitigation**: Only add new methods, don't modify existing ones.

### Duplicate Alerts
- **Risk**: Running scan multiple times creates duplicate alerts.
- **Mitigation**: Check `AlertExistsForMapAsync()` before creating; update `AlertGenerated` flag.

### Performance with Large Data
- **Risk**: Scanning thousands of maps blocks the request.
- **Mitigation**: Process in batches, consider adding pagination or background processing for very large datasets.

### Invalid Map Data
- **Risk**: Corrupted JSON causes scanner to crash.
- **Mitigation**: Wrap each map in try/catch, log errors, continue scanning.

### UI Feedback
- **Risk**: Long scan with no feedback confuses user.
- **Mitigation**: Show spinner during scan, display result summary.

---

## Dependencies

- `PressureMapAnalysisService` – Already exists, no changes needed.
- `IAlertsRepository` / `IPressureMapRepository` – Extend with new methods.
- Bootstrap – For button and alert styling (already included).

---

## Implementation Progress

| Phase | Status |
|-------|--------|
| Phase 1: Repository Extensions | ⏳ Pending |
| Phase 2: Scanner Service | ⏳ Pending |
| Phase 3: API Endpoint | ⏳ Pending |
| Phase 4: UI Integration | ⏳ Pending |
| Phase 5: Testing & Validation | ⏳ Pending |

---

## Next Steps

1. Review and approve this plan.
2. Proceed with Phase 1 (Repository Extensions).
