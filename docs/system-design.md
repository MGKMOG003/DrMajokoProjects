# System Design — DrMajokoProjects

**Version:** 1.0  
**Date:** 2025-11-03

## 1. Overview
DrMajokoProjects is a project & contractor management platform aligned to the academic POE. It provides role-based portals for Project Managers, Contractors, and Clients, with a quote lifecycle and progress tracking. The system is built on ASP.NET Core (Web MVC + Web API) with Firebase Firestore as the initial data store.

## 2. Goals & Non‑Goals
- **Goals**
  - Support projects → phases → tasks hierarchy
  - Contractor task assignment and quote submission per task
  - PM quote approval/rejection and audit trail
  - Client progress visibility and rating
  - Secure, role-based access; cloud-friendly
- **Non‑Goals (for MVP)**
  - Complex procurement/PO workflows
  - Real-time notifications
  - Advanced cost forecasting

## 3. High-Level Architecture (C4-Style)
```mermaid
C4Context
  title System Context
  Person(pm, "Project Manager")
  Person(contractor, "Contractor")
  Person(client, "Client")

  System_Boundary(sys, "DrMajokoProjects") {
    System(web, "Web MVC (UI)", "ASP.NET Core MVC")
    System(api, "Backend API", "ASP.NET Core Web API")
    SystemDb(db, "Firestore", "NoSQL")
  }

  Rel(pm, web, "Uses via browser (auth)")
  Rel(contractor, web, "Uses via browser (auth)")
  Rel(client, web, "Uses via browser (auth)")
  Rel(web, api, "REST/JSON")
  Rel(api, db, "gRPC/HTTP via SDK")
```

### Containers
```mermaid
C4Container
  title Containers
  Person(pm, "PM")
  Person(contractor, "Contractor")
  Person(client, "Client")

  Container(web, "Sustainacore.Web / DrMajokoProjects.Web", "ASP.NET Core MVC", "UI, Areas, AuthZ")
  Container(api, "Sustainacore.Api / DrMajokoProjects.Api", "ASP.NET Core Web API", "Business endpoints")
  ContainerDb(db, "Firestore", "NoSQL", "Projects, Phases, Tasks, Quotes, Users")

  Rel(pm, web, "HTTP/S + Cookies")
  Rel(contractor, web, "HTTP/S + Cookies")
  Rel(client, web, "HTTP/S + Cookies")
  Rel(web, api, "JSON over HTTP/S")
  Rel(api, db, "Firestore SDK")
```

## 4. Core Domain Model (ERD — logical)
```mermaid
erDiagram
  USER ||--o{ USERROLE : has
  ROLE ||--o{ USERROLE : granted

  PROJECT ||--o{ PHASE : contains
  PHASE ||--o{ TASK : contains
  TASK ||--o{ QUOTE : requests
  USER ||--o{ QUOTE : "submitted by Contractor"
  USER ||--o{ PROJECT : "PM owns/assigned"
  PROJECT ||--o{ DOCUMENT : "shared docs"

  USER {
    string UserId PK
    string Email
    string DisplayName
  }
  ROLE {
    string RoleId PK
    string Name
  }
  USERROLE {
    string UserId FK
    string RoleId FK
  }
  PROJECT {
    string ProjectId PK
    string Name
    string ClientName
    decimal Budget
    string Status
    date StartDate
    date EndDate
  }
  PHASE {
    string PhaseId PK
    string ProjectId FK
    string Name
    decimal Budget
    int SortOrder
  }
  TASK {
    string TaskId PK
    string PhaseId FK
    string Name
    string Description
    string Status
    decimal EstHours
    decimal ActualHours
    string AssigneeId
    int SortOrder
    date EstStart
    date EstEnd
  }
  QUOTE {
    string QuoteId PK
    string TaskId FK
    string SubmittedBy FK
    decimal AmountExcl
    decimal VAT
    decimal Total
    string Status
    date SubmittedAt
    date? DecidedAt
    string? DecisionBy
    string? Notes
  }
  DOCUMENT {
    string DocumentId PK
    string ProjectId FK
    string FileName
    string Url
    string Visibility  // client|contractor|internal
    date UploadedAt
  }
```

## 5. Sequence Diagrams

### 5.1 Quote Lifecycle
```mermaid
sequenceDiagram
  actor Contractor
  participant Web as Web MVC
  participant API as Backend API
  participant DB as Firestore

  Contractor->>Web: Open assigned Task
  Web->>API: GET /tasks/{id}
  API->>DB: Read Task
  DB-->>API: Task
  API-->>Web: Task
  Contractor->>Web: Submit Quote (Amount/Notes)
  Web->>API: POST /quotes
  API->>DB: Create Quote (Status=Pending)
  DB-->>API: OK
  API-->>Web: 201 Created

  actor PM as Project Manager
  PM->>Web: Review Quotes
  Web->>API: GET /quotes?taskId=...
  API->>DB: Query Quotes
  DB-->>API: Quotes
  API-->>Web: Quotes
  PM->>Web: Approve/Reject
  Web->>API: PATCH /quotes/{id} (Status=Approved/Rejected)
  API->>DB: Update Quote
  DB-->>API: OK
  API-->>Web: 200 OK
```

### 5.2 Task Status Update
```mermaid
sequenceDiagram
  actor PM
  participant Web
  participant API
  participant DB

  PM->>Web: Set Task status to InProgress
  Web->>API: PATCH /tasks/{id} (Status=InProgress)
  API->>DB: Update Task
  DB-->>API: OK
  API-->>Web: 200 OK
```

## 6. Firestore Data Design (Collections)
- `users/{userId}`: profile, roles (denormalized list)
- `projects/{projectId}`: meta
  - `phases/{phaseId}`
    - `tasks/{taskId}`
      - `quotes/{quoteId}`
  - `documents/{documentId}`

Recommended composite indexes:
- `quotes` by `taskId, status`
- `tasks` by `assigneeId, status`
- `projects` by `ownerId, status`

## 7. Security
- Cookie Auth (server-side) + role-based authorization per Area (ProjectManager, Contractor, Admin)
- Enforce HTTPS, secure cookies, anti-forgery tokens on form posts
- Principle of least privilege for Firestore service account
- Secrets via `dotnet user-secrets` in dev; never commit credentials

## 8. Deployment Topology
- **Local**: Web + API run on localhost (different ports)
- **Cloud**: Container-ready; API and Web can be separate services behind HTTPS
- CI builds and runs tests on PRs; optional deploy job can be added later with environment protection rules

## 9. Observability
- ASP.NET logging (structured) with Console provider
- Correlation IDs via middleware
- Future: OpenTelemetry exporters

## 10. Screenshots (Mocks)
Below are mock screenshots for documentation purposes. Replace with real screenshots when UI is finalized.

![PM Dashboard](docs/screenshots/pm-dashboard.png)
![Phases & Tasks](docs/screenshots/phase-task.png)

## 11. Appendix: PlantUML Versions
For teams preferring PlantUML, equivalent diagrams can be generated by copying the Mermaid logic.
```plantuml
@startuml
!theme blueprint
title System Context (Simplified)
actor "Project Manager" as PM
actor "Contractor" as CT
actor "Client" as CL
rectangle "DrMajokoProjects" {
  [Web MVC] as WEB
  [Backend API] as API
  database "Firestore" as DB
}
PM --> WEB : Uses
CT --> WEB : Uses
CL --> WEB : Uses
WEB --> API : REST/JSON
API --> DB : SDK
@enduml
```