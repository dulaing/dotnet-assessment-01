# Overdue Scan — API Contract (Mobile & QA)

**Feature:** Daily + on-demand overdue borrowing scan  
**System:** Library Management API  
**Style:** REST only (poll for status — no WebSockets)

---

## 1. Requirement

The library must mark past-due borrowings as **Overdue** and prepare member reminders.

This work runs in the **background** so HTTP clients are not blocked.

### Triggers

| Trigger | Description |
| --- | --- |
| **Scheduled** | Runs automatically every day at **00:30 UTC** (configurable) |
| **Manual** | Admin starts a scan via HTTP whenever needed |

Both triggers use the same scan logic and the same job status API.

### Business rules (eligible borrowings)

A borrowing is marked overdue when:

- `Status == Borrowed`
- `DueDate <` scan time (UTC)
- `ReturnedDate` is null
- Not already `Overdue` (unless force-reminder option is used)

Marking overdue does **not** change `AvailableCopies`. Returning still works afterwards.

---

## 2. API URLs

| Action | Method | URL |
| --- | --- | --- |
| Start manual scan | `POST` | `/api/jobs/overdue-scans` |
| Get job status (poll) | `GET` | `/api/jobs/{jobId}` |
| List recent jobs | `GET` | `/api/jobs?type=OverdueScan&take=20` |
| Filter by trigger | `GET` | `/api/jobs?type=OverdueScan&trigger=Manual` or `trigger=Scheduled` |
| Get schedule info | `GET` | `/api/jobs/overdue-scans/schedule` |

---

## 3. Payloads & Responses

### 3.1 Start manual scan

`POST /api/jobs/overdue-scans`

**Request**

```json
{
  "requestedBy": "mobile-admin",
  "includeReminderDetails": true,
  "forceReprocessReminders": false,
  "asOfUtc": null
}
```

| Field | Required | Notes |
| --- | --- | --- |
| `requestedBy` | No | Who started it (max 100 chars) |
| `includeReminderDetails` | No | Default `true` |
| `forceReprocessReminders` | No | Default `false`. If `true`, create reminders again for already-overdue loans (status stays `Overdue`) |
| `asOfUtc` | No | Default now (UTC). Cannot be more than 1 day in the future |

**Response — `202 Accepted`**

```json
{
  "jobId": "2f1e4a8c-0b9a-4f2d-9c3e-6a1b8d7e5f40",
  "type": "OverdueScan",
  "trigger": "Manual",
  "status": "Queued",
  "createdAt": "2026-08-12T06:15:00.123Z",
  "startedAt": null,
  "completedAt": null,
  "requestedBy": "mobile-admin",
  "options": {
    "includeReminderDetails": true,
    "forceReprocessReminders": false,
    "asOfUtc": "2026-08-12T06:15:00.123Z"
  },
  "result": null,
  "error": null,
  "statusUrl": "/api/jobs/2f1e4a8c-0b9a-4f2d-9c3e-6a1b8d7e5f40",
  "message": "Overdue scan accepted and queued for background processing."
}
```

Also expect header: `Location: /api/jobs/{jobId}`

**Errors**

`400 Bad Request` — invalid payload

```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": [
    {
      "field": "asOfUtc",
      "message": "asOfUtc cannot be more than 1 day in the future"
    }
  ]
}
```

`409 Conflict` — another scan is already queued/processing

```json
{
  "statusCode": 409,
  "message": "An overdue scan job is already processing",
  "traceId": "00-abc123..."
}
```

---

### 3.2 Poll job status

`GET /api/jobs/{jobId}`

Poll every **1–2 seconds** until `status` is `Completed`, `Failed`, or `Skipped`.

**Statuses:** `Queued` → `Processing` → `Completed` | `Failed` | `Skipped`

**While running — `200 OK`**

```json
{
  "jobId": "2f1e4a8c-0b9a-4f2d-9c3e-6a1b8d7e5f40",
  "type": "OverdueScan",
  "trigger": "Manual",
  "status": "Processing",
  "createdAt": "2026-08-12T06:15:00.123Z",
  "startedAt": "2026-08-12T06:15:00.180Z",
  "completedAt": null,
  "requestedBy": "mobile-admin",
  "options": {
    "includeReminderDetails": true,
    "forceReprocessReminders": false,
    "asOfUtc": "2026-08-12T06:15:00.123Z"
  },
  "result": null,
  "error": null,
  "statusUrl": "/api/jobs/2f1e4a8c-0b9a-4f2d-9c3e-6a1b8d7e5f40"
}
```

**Completed — `200 OK`**

```json
{
  "jobId": "2f1e4a8c-0b9a-4f2d-9c3e-6a1b8d7e5f40",
  "type": "OverdueScan",
  "trigger": "Manual",
  "status": "Completed",
  "createdAt": "2026-08-12T06:15:00.123Z",
  "startedAt": "2026-08-12T06:15:00.180Z",
  "completedAt": "2026-08-12T06:15:02.540Z",
  "requestedBy": "mobile-admin",
  "options": {
    "includeReminderDetails": true,
    "forceReprocessReminders": false,
    "asOfUtc": "2026-08-12T06:15:00.123Z"
  },
  "result": {
    "borrowingsScanned": 25,
    "markedOverdue": 3,
    "remindersCreated": 3,
    "alreadyOverdueSkipped": 2,
    "durationMs": 2360,
    "overdueBorrowingIds": [
      "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "a3bb189e-8bf9-3888-9912-ace4e6543002",
      "1b9d6bcd-bbfd-4b2d-9b5d-ab8dfbbd4bed"
    ],
    "reminders": [
      {
        "memberId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "memberEmail": "jane@example.com",
        "memberName": "Jane Doe",
        "borrowingId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
        "bookId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
        "bookTitle": "Clean Code",
        "dueDate": "2026-08-01T00:00:00Z",
        "message": "Your borrowed book is overdue. Please return it to the library."
      }
    ]
  },
  "error": null,
  "statusUrl": "/api/jobs/2f1e4a8c-0b9a-4f2d-9c3e-6a1b8d7e5f40"
}
```

**Failed — `200 OK`** (job exists; failure is in `status`)

```json
{
  "jobId": "2f1e4a8c-0b9a-4f2d-9c3e-6a1b8d7e5f40",
  "type": "OverdueScan",
  "trigger": "Manual",
  "status": "Failed",
  "createdAt": "2026-08-12T06:15:00.123Z",
  "startedAt": "2026-08-12T06:15:00.180Z",
  "completedAt": "2026-08-12T06:15:01.010Z",
  "requestedBy": "mobile-admin",
  "result": null,
  "error": {
    "code": "OVERDUE_SCAN_FAILED",
    "message": "Database failure while updating borrowing status"
  },
  "statusUrl": "/api/jobs/2f1e4a8c-0b9a-4f2d-9c3e-6a1b8d7e5f40"
}
```

**Not found — `404`**

```json
{
  "statusCode": 404,
  "message": "Job not found",
  "traceId": "00-abc123..."
}
```

---

### 3.3 List jobs

`GET /api/jobs?type=OverdueScan&take=20`

```json
{
  "items": [
    {
      "jobId": "2f1e4a8c-0b9a-4f2d-9c3e-6a1b8d7e5f40",
      "type": "OverdueScan",
      "trigger": "Manual",
      "status": "Completed",
      "createdAt": "2026-08-12T06:15:00.123Z",
      "completedAt": "2026-08-12T06:15:02.540Z",
      "requestedBy": "mobile-admin",
      "resultSummary": {
        "markedOverdue": 3,
        "remindersCreated": 3
      }
    },
    {
      "jobId": "8c1a2b3d-4e5f-6789-abcd-ef0123456789",
      "type": "OverdueScan",
      "trigger": "Scheduled",
      "status": "Completed",
      "createdAt": "2026-08-12T00:30:00.010Z",
      "completedAt": "2026-08-12T00:30:01.820Z",
      "requestedBy": "scheduler",
      "resultSummary": {
        "markedOverdue": 5,
        "remindersCreated": 5
      }
    }
  ]
}
```

---

### 3.4 Schedule info

`GET /api/jobs/overdue-scans/schedule`

```json
{
  "enabled": true,
  "timeOfDay": "00:30",
  "timeZone": "UTC",
  "cronExpression": "30 0 * * *",
  "nextRunAtUtc": "2026-08-13T00:30:00Z",
  "lastScheduledJobId": "8c1a2b3d-4e5f-6789-abcd-ef0123456789",
  "lastScheduledRunAtUtc": "2026-08-12T00:30:00.010Z",
  "lastScheduledStatus": "Completed"
}
```

Scheduled runs have **no start URL** — they are created by the server. Inspect them with the same `GET /api/jobs/...` APIs (`trigger: "Scheduled"`).

---

## 4. Expected Background Processing Behaviour

| Expectation | Detail |
| --- | --- |
| Manual start is fast | `POST` returns **202** quickly (typically &lt; 500ms). It does **not** wait for the scan to finish. |
| Work continues server-side | After `202`, status moves `Queued` → `Processing` → `Completed` (or `Failed`). |
| Poll for outcome | Use `GET /api/jobs/{jobId}`. No WebSockets. |
| Daily automatic run | At **00:30 UTC**, server creates a job with `trigger: "Scheduled"`, `requestedBy: "scheduler"`. |
| Same logic for both triggers | Manual and scheduled jobs produce the same `result` shape and update borrowings the same way. |
| One active scan at a time | If a scan is already `Queued`/`Processing`: manual `POST` → **409**; scheduled tick → **skipped** (host stays up). |
| Side effect on borrowings | After `Completed`, eligible loans appear as `Status: Overdue` on `GET /api/borrowings`. |
| API stays usable | Other endpoints (books, members, borrowings) remain responsive while a scan runs. |
| Empty run is valid | If nothing is overdue, job still `Completed` with `markedOverdue: 0`. |
| Failed job is visible via poll | Poll returns **200** with `status: "Failed"` and `error.message` — not a transport error. |

### Mobile flow (manual)

1. `POST /api/jobs/overdue-scans` → store `jobId`
2. Poll `GET /api/jobs/{jobId}` every 1–2s
3. Stop on `Completed` / `Failed` / `Skipped`
4. Show `result` or `error`

### Quick checks for QA

- Manual `POST` returns `202` with `trigger: "Manual"`
- Poll shows progress, then `Completed` with matching overdue counts
- Past-due `Borrowed` loans become `Overdue`
- Returned / not-due loans are not marked
- Second scan with no new overdue rows → `markedOverdue: 0`
- Schedule endpoint returns next run time
- After schedule time (or test config near “now”), a `Scheduled` job appears in the list
