
# DrMajokoProjects

Project & Construction Management Platform (Academic POE)

## Overview

DrMajokoProjects is a web-based project and contractor management system designed for construction and service-based environments. It enables Project Managers, Contractors, and Clients to collaborate through a unified system supporting task planning, quotation workflows, progress tracking, and quality evaluation.

This app was built as part of an academic capstone POE focusing on software engineering, secure development, and real-world business use-cases.

## Key Features

### Project Management

* Create and manage projects, phases, and tasks
* Track project progress and budget
* Assign contractors to tasks

### Quotation Workflow

* Contractors submit quotations per assigned task
* Project Managers approve or reject quotations
* Audit history of quotes and decisions

### Contractor Portal

* View assigned tasks
* Upload quotations
* Submit progress information

### Client Interaction

* View project updates
* Rate contractor work via surveys

### Authentication & Security

* Secure login with ASP.NET Identity / Firebase Authentication
* Role-based access (Project Manager, Contractor, Client, Admin)
* Enforced authorization across protected areas
* HTTPS-first configuration and secure coding practices

## Technology Stack

| Layer           | Technology                                       |
| --------------- | ------------------------------------------------ |
| Frontend        | ASP.NET Core MVC / Razor Views                   |
| Backend API     | ASP.NET Core Web API                             |
| Hosting         | Local / Cloud Ready                              |
| Database        | Firebase Firestore (POE requirement) / SQL Ready |
| Authentication  | Cookie Auth + Firebase Admin SDK                 |
| Architecture    | Layered / Clean inspired                         |
| Dev Environment | .NET 9 SDK, Visual Studio 2022                   |

## System Architecture

### High-Level Design

* **Sustainacore.Web / DrMajokoProjects.Web**

  * MVC UI
  * Authentication & Authorization
  * Areas: ProjectManager, Contractor, Admin

* **Sustainacore.Api / DrMajokoProjects.Api**

  * API layer for handling project, task, quote, and user data
  * Firestore repository integration

* **Shared Contracts**

  * DTOs and request/response models

### Roles

| Role            | Permissions                            |
| --------------- | -------------------------------------- |
| Admin           | Manage users, roles, access            |
| Project Manager | Manage projects, phases, tasks, quotes |
| Contractor      | View tasks, submit quotes, update work |
| Client          | View project progress, rate work       |

## Folder Structure

```
src/
 ├─ Web/                  ASP.NET MVC UI
 │   ├─ Areas
 │   │   ├─ ProjectManager/
 │   │   ├─ Contractor/
 │   │   └─ Admin/
 │   └─ Services/
 ├─ Api/                  Backend REST API
 │   └─ Controllers/
 ├─ Contracts/            DTOs and shared models
 └─ README.md
```

## Getting Started

### Prerequisites

* .NET 9 SDK
* Visual Studio 2022 Community or higher
* Firebase Project
* Firestore Database enabled
* Firebase Admin SDK credentials JSON

### Configure Firebase

Set user-secret configuration:

```
dotnet user-secrets set "Firebase:ProjectId" "your-project-id"
dotnet user-secrets set "Firebase:CredentialsPath" "path/to/credentials.json"
```

### Run Locally

```
dotnet restore
dotnet build
dotnet run --project src/Web/Sustainacore.Web
```

Open browser:

```
https://localhost:7017/
```

## Security Considerations

* Cookie authentication configured with sliding expiration
* Strict access control via Roles & Areas
* Secure handling of Firebase credentials
* Input validation and sanitization patterns applied

## Future Enhancements

* Azure / Google Cloud deployment pipeline
* Offline mobile contractor app (MAUI)
* Document management with secure storage
* Notifications & automated reminders
* Budget forecasting and analytics

## Academic Notes

This project demonstrates:

* Secure software architecture principles
* Web development with ASP.NET Core
* API design and role-based access control
* Real-world project workflow modelling
* Cloud backend integration (Firestore)
* Collaborative software engineering lifecycle

## License

For academic use. All rights reserved to the contributor.

---
