# DrMajokoProjects

Project & Construction Management Platform (Academic POE)

[![Build](https://img.shields.io/badge/.NET-9.0-blue)]() [![ASP.NET MVC](https://img.shields.io/badge/ASP.NET-MVC-success)]() [![Swagger](https://img.shields.io/badge/API-Swagger-green)]()

## Overview

**DrMajokoProjects** is a web-based project and contractor management platform with three major roles:
- **Project Manager (PM)** – creates projects/phases/tasks, approves quotations, manages contractors and documents
- **Contractor** – receives tasks, submits quotations, updates progress/performance artifacts
- **Client** – views project progress and rates contractors

### Solution layout:

DrMajokoProjects.sln
├─ DrMajokoProjects.Web # ASP.NET Core MVC front-end (default route → Account/Login)
├─ DrMajokoProjects.API # REST API with Swagger (AllowAll CORS, local storage service)
├─ DrMajokoProjects.Mobile # Mobile project (scaffold present)
└─ docs/ # Design notes, screenshots, system design


## Features (MVP)

- Projects → Phases → Tasks hierarchy
- Contractor assignment per task
- Quotation workflow (Contractor submit → PM approve/reject with history)
- Client visibility and rating
- Document uploads (local storage service), tags/visibility flags
- Role-gated UI areas (Admin / PM / Contractor / Client)
- API Swagger UI at `/swagger` (when API runs)

