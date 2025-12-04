# Alerts Page UI Enhancement Plan

This document outlines a phase-wise approach to redesign the Alerts page with a two-column layout, line chart with threshold, and modal-based alert detail with heatmap visualization.

---

## Current State Analysis

### Existing Files

| File | Purpose |
|------|---------|
| `Views/Home/Alerts.cshtml` | Alerts page with summary cards and table |
| `Views/SensorView/Alerts.cshtml` | Simpler alerts table view |
| `Controllers/AlertController.cs` | **API controller** (`AlertsController`) for alert data |
| `Controllers/SensorViewController.cs` | MVC controller with only `History` action |
| `Controllers/HomeController.cs` | MVC controller with `AlertsPage` action (returns view) |
| `Repositories/AlertsRepository.cs` | Data access for alerts |
| `Models/Alert.cs` | Alert entity with `PressureMapId`, `AlertType`, `ClusterInfoJson` |

### Navigation Issue

The navbar in `_Layout.cshtml` links to:
```html
<a asp-controller="Alert" asp-action="Index">Alerts</a>
```
This route does **not exist**. Options:
1. Create a new `AlertController` (MVC) with `Index` action
2. Fix navbar to point to existing `HomeController.AlertsPage`

**Decision**: Create a new MVC `AlertController` with `Index` action to keep API and MVC controllers separate.

---

## Target Design

### Layout (Two-Column)
```
┌─────────────────────────────────────────────────────────────────┐
│  Alerts Dashboard                                    [Filters]  │
├───────────────────────────────────────────┬─────────────────────┤
│                                           │  Summary Cards      │
│  Line Chart (Alert counts over time)      │  - Total Alerts     │
│  with horizontal threshold line           │  - Active / Resolved│
│                                           │  - Avg Resolution   │
├───────────────────────────────────────────┤  - By Type (pie)    │
│                                           │                     │
│  Alerts Table                             │                     │
│  - ID, Type, Pressure, Time, Status       │                     │
│  - Click row → Modal                      │                     │
│                                           │                     │
└───────────────────────────────────────────┴─────────────────────┘
```

### Modal (Alert Detail)
```
┌─────────────────────────────────────────────────────────────────┐
│  Alert #123                                              [X]    │
├─────────────────────────────────────────────────────────────────┤
│  Type: PressureAnomaly          Status: Active                  │
│  Pressure: 142.5 PSI            Timestamp: 2025-12-03 21:30     │
│  Message: High pressure detected in cluster                     │
├─────────────────────────────────────────────────────────────────┤
│  Pressure Map Heatmap (if PressureMapId exists)                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │                                                         │    │
│  │              32x32 Canvas Heatmap                       │    │
│  │                                                         │    │
│  └─────────────────────────────────────────────────────────┘    │
│  Color scale: 0 ──────────────────────────────────────── 255    │
├─────────────────────────────────────────────────────────────────┤
│  Cluster Info (if available)                                    │
│  - Cluster count, max pressure, centroid, bounding box          │
├─────────────────────────────────────────────────────────────────┤
│                                    [Resolve]  [Close]           │
└─────────────────────────────────────────────────────────────────┘
```

---

## Phase-Wise Implementation

### Phase 1: API Enhancements

**Goal**: Add endpoints for time-filtered alerts and trend data.

#### New Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/alerts/filtered` | GET | Alerts with query params: `start`, `end`, `type`, `status` |
| `/api/alerts/trend` | GET | Alert counts grouped by hour/day for chart |
| `/api/alerts/stats` | GET | Summary stats (total, active, resolved, by type) |
| `/api/alerts/{id}/detail` | GET | Single alert with related pressure map data |

#### Files to Modify
- `Controllers/AlertController.cs` (API)
- `Repositories/IAlertsRepository.cs`
- `Repositories/AlertsRepository.cs`

#### New DTOs
- `AlertFilterRequest` (start, end, type, status)
- `AlertTrendResponse` (timestamp, count)
- `AlertStatsResponse` (total, active, resolved, byType)
- `AlertDetailResponse` (alert + pressure map + metrics)

---

### Phase 2: MVC Controller & View Setup

**Goal**: Create MVC controller and new Alerts view with two-column layout.

#### New Files
- `Controllers/AlertController.cs` (MVC, not API) → **Rename to `AlertPageController.cs`** to avoid conflict with existing API controller

#### Modified Files
- `Views/Shared/_Layout.cshtml` – Fix navbar link to new controller

#### New View
- `Views/AlertPage/Index.cshtml` – Two-column layout skeleton

---

### Phase 3: Filters & Time Range Controls

**Goal**: Add filter bar with time range presets and status/type filters.

#### UI Components
- Time range buttons: 1H, 6H, 24H, 7D
- Alert type dropdown (All, PressureAnomaly, SustainedPressure, etc.)
- Status toggle (All / Active / Resolved)

#### JavaScript
- Store selected filters in state
- Re-fetch data on filter change
- Update URL query string for shareable links

---

### Phase 4: Line Chart with Threshold

**Goal**: Display alert counts over selected time window with horizontal threshold line.

#### Implementation
- Use Chart.js (already included)
- X-axis: time buckets (hourly for 1H/6H/24H, daily for 7D)
- Y-axis: alert count
- Horizontal annotation line for threshold (configurable, default = 5 alerts)

#### Data Source
- `/api/alerts/trend?start=...&end=...&bucket=hour|day`

---

### Phase 5: Alerts Table

**Goal**: Responsive table with row click to open modal.

#### Columns
- ID
- Type (badge)
- Pressure (PSI)
- Timestamp
- Status (badge: Active/Resolved)
- Actions (Resolve button if active)

#### Features
- Row highlighting for unresolved alerts
- Click row → open detail modal
- Pagination or infinite scroll (optional)

---

### Phase 6: Summary Sidebar

**Goal**: Right column with metric cards and mini charts.

#### Cards
1. **Total Alerts** – count in selected range
2. **Active vs Resolved** – two numbers or small bar
3. **By Type** – mini donut/pie chart
4. **Highest Pressure** – max pressure value in range

#### Data Source
- `/api/alerts/stats?start=...&end=...`

---

### Phase 7: Alert Detail Modal with Heatmap

**Goal**: Modal showing full alert info and canvas-based pressure map heatmap.

#### Modal Content
- Alert metadata (type, pressure, timestamp, message, status)
- Cluster info (if available)
- Canvas heatmap (32x32 grid, color-coded 0–255)
- Color scale legend
- Resolve button (if active)

#### Heatmap Implementation
- Fetch pressure map via `/api/risk/by-map/{pressureMapId}` or new endpoint
- Render 32x32 canvas with color gradient (blue → green → yellow → red)
- Scale canvas to ~256x256 or 320x320 pixels for visibility

---

### Phase 8: Testing & Validation

**Goal**: Verify all features work without breaking existing functionality.

#### Test Cases
1. Navbar correctly navigates to new Alerts page
2. Time range filters update chart and table
3. Type/status filters work correctly
4. Chart displays correct trend data with threshold line
5. Table shows alerts, row click opens modal
6. Modal displays correct alert details
7. Heatmap renders correctly when pressure map exists
8. Resolve action works from modal
9. Existing API endpoints still function
10. Home page dashboard unaffected

---

## File Summary

### New Files (6 files)

| File | Phase |
|------|-------|
| `Controllers/AlertPageController.cs` | Phase 2 |
| `Views/AlertPage/Index.cshtml` | Phase 2 |
| `Models/DTOs/AlertFilterRequest.cs` | Phase 1 |
| `Models/DTOs/AlertTrendResponse.cs` | Phase 1 |
| `Models/DTOs/AlertStatsResponse.cs` | Phase 1 |
| `Models/DTOs/AlertDetailResponse.cs` | Phase 1 |

### Modified Files (4 files)

| File | Phase | Changes |
|------|-------|---------|
| `Controllers/AlertController.cs` (API) | Phase 1 | Add filtered, trend, stats, detail endpoints |
| `Repositories/IAlertsRepository.cs` | Phase 1 | Add new query methods |
| `Repositories/AlertsRepository.cs` | Phase 1 | Implement new query methods |
| `Views/Shared/_Layout.cshtml` | Phase 2 | Fix navbar link |

---

## Dependencies

- **Chart.js** – Already included, will use annotation plugin for threshold line
- **Bootstrap** – Already included, will use modal component
- **Canvas API** – Native browser API for heatmap rendering

---

## Risk Mitigation

### Breaking Existing Functionality
- **Risk**: Modifying API controller breaks existing consumers
- **Mitigation**: Only add new endpoints, do not modify existing ones

### Performance with Large Data
- **Risk**: Fetching all alerts for chart/table
- **Mitigation**: Use time-filtered queries, limit results, use pagination

### Heatmap Rendering
- **Risk**: Missing pressure map data for some alerts
- **Mitigation**: Show "No map available" placeholder when `PressureMapId` is null

### Navigation Conflict
- **Risk**: MVC and API controllers with similar names
- **Mitigation**: Name MVC controller `AlertPageController` to avoid route conflicts

---

## Questions Resolved

1. **Which Alerts page?** → Create new `AlertPageController` with `Index` action, fix navbar link
2. **Detail panel behavior?** → Modal dialog
3. **Pressure map visualization?** → Canvas-based heatmap (32x32 grid)
4. **Time range presets?** → 1H, 6H, 24H, 7D
5. **Threshold line?** → Horizontal line on chart (configurable value)

---

## Implementation Progress

| Phase | Status |
|-------|--------|
| Phase 1: API Enhancements | ✅ Completed |
| Phase 2: MVC Controller & View Setup | ✅ Completed |
| Phase 3: Filters & Time Range Controls | ✅ Completed |
| Phase 4: Line Chart with Threshold | ✅ Completed |
| Phase 5: Alerts Table | ✅ Completed |
| Phase 6: Summary Sidebar | ✅ Completed |
| Phase 7: Alert Detail Modal with Heatmap | ✅ Completed |
| Phase 8: Testing & Validation | ⏳ Pending |

---

## Next Steps

1. Review and approve this plan
2. Proceed with Phase 1 (API Enhancements)
