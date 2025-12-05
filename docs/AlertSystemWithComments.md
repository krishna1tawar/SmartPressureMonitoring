# Alert System with Comments Feature

## Overview

The Smart Pressure Monitoring system includes a comprehensive alert management system that detects high-pressure anomalies from 32x32 pressure maps and allows clinicians to add comments and feedback on alerts.

## Architecture

### Components

```
┌─────────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│  PressureMapAlert   │────▶│  AlertsRepository    │────▶│  Database       │
│  Scanner            │     │                      │     │  (Alerts,       │
└─────────────────────┘     └──────────────────────┘     │   Comments)     │
         │                           │                    └─────────────────┘
         ▼                           ▼
┌─────────────────────┐     ┌──────────────────────┐
│  PressureMap        │     │  AlertsController    │
│  AnalysisService    │     │  (API Endpoints)     │
└─────────────────────┘     └──────────────────────┘
```

### Key Classes

| Class | Location | Purpose |
|-------|----------|---------|
| `Alert` | `Models/Alert.cs` | Entity representing an alert with cluster info |
| `Comment` | `Models/Comment.cs` | Entity for comments with feedback support |
| `AlertsController` | `Controllers/AlertController.cs` | REST API endpoints |
| `AlertsRepository` | `Repositories/AlertsRepository.cs` | Data access layer |
| `PressureMapAlertScanner` | `Services/PressureMapAlertScanner.cs` | Scans maps for alerts |

## Data Models

### Alert Entity

```csharp
public class Alert
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; }
    public double Pressure { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsResolved { get; set; }
    public int? PressureMapId { get; set; }          // Links to SensorData
    public string AlertType { get; set; }            // e.g., "HighPressureCluster"
    public ClusterInfo? ClusterInfo { get; set; }    // JSON-serialized cluster data
    public List<Comment> Comments { get; set; }      // Related comments
}
```

### Comment Entity

```csharp
public class Comment
{
    public int Id { get; set; }
    public int AlertId { get; set; }                 // Foreign key to Alert
    public int UserId { get; set; }                  // Comment author
    public string CommentText { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? FeedbackText { get; set; }        // Clinician feedback
    public DateTime? FeedbackProvidedAt { get; set; }
    public int? FeedbackUserId { get; set; }         // Feedback provider
}
```

## API Endpoints

### Alert Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/alerts/list` | Get all alerts |
| `GET` | `/api/alerts/unresolved` | Get unresolved alerts |
| `GET` | `/api/alerts/filtered` | Filter alerts by date, type, status |
| `GET` | `/api/alerts/by-type/{type}` | Get alerts by type |
| `GET` | `/api/alerts/by-map/{mapId}` | Get alerts for a pressure map |
| `GET` | `/api/alerts/{id}/detail` | Get alert with full details |
| `POST` | `/api/alerts/resolve/{id}` | Mark alert as resolved |
| `POST` | `/api/alerts/scan` | Trigger manual alert scan |

### Comments & Feedback

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/alerts/{id}/comments` | Get comments for an alert |
| `POST` | `/api/alerts/{id}/comments` | Add a comment to an alert |
| `POST` | `/api/alerts/comments/{commentId}/feedback` | Add/update feedback on a comment |

### Statistics & Trends

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/alerts/stats` | Get alert statistics |
| `GET` | `/api/alerts/trend` | Get alert trend data for charts |
| `GET` | `/api/alerts/for-review` | Get items needing clinician review |

## Usage Examples

### Get All Unresolved Alerts

```http
GET /api/alerts/unresolved
```

**Response:**
```json
[
  {
    "id": 1,
    "alertType": "HighPressureCluster",
    "message": "High risk detected: 2 high-pressure cluster(s) found. Max pressure: 245. Coverage: 85 pixels.",
    "pressure": 245,
    "timestamp": "2025-12-05T10:30:00Z",
    "isResolved": false,
    "pressureMapId": 42
  }
]
```

### Get Alert Details with Pressure Map

```http
GET /api/alerts/1/detail
```

**Response:**
```json
{
  "id": 1,
  "alertType": "HighPressureCluster",
  "message": "High risk detected...",
  "pressure": 245,
  "timestamp": "2025-12-05T10:30:00Z",
  "isResolved": false,
  "pressureMapId": 42,
  "pressureMapMatrix": [[...32x32 matrix...]],
  "mapScale": "1-255",
  "mapUnit": "mmHg",
  "clusterInfo": {
    "clusters": [...],
    "totalClusters": 2,
    "totalHighPressurePixels": 85
  },
  "metrics": {
    "riskScore": 72.5,
    "riskLevel": "High"
  },
  "comments": [...]
}
```

### Add a Comment

```http
POST /api/alerts/1/comments
Content-Type: application/json

{
  "commentText": "Patient repositioned. Will monitor for improvement.",
  "userId": 5
}
```

**Response:**
```json
{
  "id": 10,
  "alertId": 1,
  "userId": 5,
  "commentText": "Patient repositioned. Will monitor for improvement.",
  "createdAt": "2025-12-05T10:45:00Z",
  "feedbackText": null,
  "feedbackProvidedAt": null,
  "feedbackUserId": null
}
```

### Add Feedback to a Comment

```http
POST /api/alerts/comments/10/feedback
Content-Type: application/json

{
  "feedbackText": "Good intervention. Continue monitoring every 2 hours.",
  "userId": 3
}
```

### Filter Alerts

```http
GET /api/alerts/filtered?start=2025-12-01&end=2025-12-05&type=HighPressureCluster&status=active
```

### Get Alert Statistics

```http
GET /api/alerts/stats?start=2025-12-01&end=2025-12-05
```

**Response:**
```json
{
  "total": 45,
  "active": 12,
  "resolved": 33,
  "maxPressure": 252,
  "byType": {
    "HighPressureCluster": 40,
    "PressureAnomaly": 5
  }
}
```

### Trigger Manual Scan

```http
POST /api/alerts/scan
```

**Response:**
```json
{
  "totalMapsScanned": 150,
  "alertsCreated": 3,
  "mapsSkipped": 147,
  "errors": 0,
  "errorMessages": [],
  "isComplete": true,
  "cancelled": false,
  "message": "Scan complete: 3 alert(s) created, 147 map(s) skipped, 0 error(s)."
}
```

## Alert Generation Logic

### When Alerts Are Created

The `PressureMapAlertScanner` creates alerts based on these criteria:

1. **Critical Risk (score ≥ 75)** → Always generates alert
2. **High Risk (score ≥ 50)** with ≥ 20 high-pressure pixels → Generates alert
3. **Medium Risk (score ≥ 35)** with ≥ 60 high-pressure pixels → Generates alert

### One Alert Per Map

The system creates **one alert per pressure map**, not per cluster. This prevents alert fatigue while still capturing all relevant cluster information in the `ClusterInfo` property.

### Risk Score Calculation

```
Risk Score = (Intensity Component × 0.6) + (Coverage Component × 0.4)

Where:
- Intensity Component = Max Pressure / 255
- Coverage Component = High Pressure Pixels / 1024 (total pixels)
```

### Risk Levels

| Score Range | Level |
|-------------|-------|
| 0-24 | Low |
| 25-49 | Medium |
| 50-74 | High |
| 75-100 | Critical |

## Database Schema

### Alerts Table

```sql
CREATE TABLE Alerts (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL DEFAULT 1,
    Message NVARCHAR(MAX) NOT NULL,
    Pressure FLOAT NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    IsResolved BIT NOT NULL DEFAULT 0,
    PressureMapId INT NULL,
    AlertType NVARCHAR(255) NOT NULL DEFAULT 'PressureAnomaly',
    ClusterInfoJson NVARCHAR(MAX) NULL
);
```

### Comments Table

```sql
CREATE TABLE Comments (
    Id INT PRIMARY KEY IDENTITY,
    AlertId INT NOT NULL FOREIGN KEY REFERENCES Alerts(Id),
    UserId INT NOT NULL,
    CommentText NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    FeedbackText NVARCHAR(MAX) NULL,
    FeedbackProvidedAt DATETIME2 NULL,
    FeedbackUserId INT NULL
);
```

## Workflow

### Typical Clinical Workflow

1. **Alert Generated** → System detects high-pressure cluster in pressure map
2. **Clinician Reviews** → Views alert details including pressure map visualization
3. **Action Taken** → Clinician repositions patient or takes other action
4. **Comment Added** → Clinician documents the intervention
5. **Feedback Provided** → Senior clinician reviews and provides feedback
6. **Alert Resolved** → Alert marked as resolved after successful intervention

### Integration with Pressure Map Scanner

The `PressureMapAlertScanner` service:
- Runs on-demand via `/api/alerts/scan` endpoint
- Processes maps in batches of 100
- Skips maps that already have alerts
- Updates metrics after scanning each map
- Supports cancellation via `CancellationToken`
