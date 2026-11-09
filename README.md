# RaceDay - Event Management System

### Overview
RaceDay is a full-stack, cloud-ready event management system designed for the South African road running, walking, and cycling community. It solves the problem of fragmented, paper-based event registration by providing a centralised digital platform. The system allows Organisers to create and manage events, while Participants can browse, register, track results, and prepare for race day.

The application is built using a modern 3-tier architecture:
ASP.NET Core MVC (Front-end) → RESTful API (Back-end) → SQL Server & Azure Blob Storage.

## User Roles

### Organiser
Organisers are the creators of the events. They have access to a dedicated dashboard where they can:

Create, edit, and delete their own events.

Upload event banner images (stored in Azure Blob Storage).

Create and manage event categories (e.g., 10km Run, 42km Marathon) and entry fees.

View participants who have enrolled in their events.

Capture and publish participant race results (finish times and positions).

### Participant
Participants are the end-users interacting with the events. They can:

Browse and filter upcoming events across South Africa.

View detailed event information and available categories.

Enrol in events.

View their personal dashboard showing their upcoming registered events.

View their past race results and finishing positions.

Manage their profile and upload a profile picture (stored in Azure Blob Storage).

### Solution Structure
The repository is structured as a single Visual Studio solution containing the following projects:

RaceDay.Api: The ASP.NET Core Web API that handles business logic, database operations (EF Core), and Azure Blob integration.

RaceDay.Contracts: A shared class library containing Data Transfer Objects (DTOs) used by both the API and the MVC application.

RaceDay.Api.Tests: The xUnit testing project for the API.

RaceDay.Mvc: The ASP.NET Core MVC front-end that consumes the API.

docs: Contains the Part 1 ERD, API Endpoint Plan, and SQL Database Script.

### Project Description
RaceDay is a full-stack web-based event management system designed specifically for the South African road running, walking, and cycling community. The platform addresses the challenges faced by event organisers who still rely on paper-based registration, spreadsheets, and disconnected communication channels.

The Problem: Despite South Africa's rich road events culture—from the iconic Comrades Marathon to the Cape Town Cycle Tour, Soweto Marathon, and hundreds of community events—many organisers struggle with manual registration processes, fragmented participant management, and inadequate race-day preparation tools.

The Solution: RaceDay provides a centralised digital platform where Event Organisers can create and manage events, categories, and participant results, while Participants can browse upcoming events, register, track their personal performance history, and prepare for race day using live weather and route information.

### Part 1 - Planning and Database
All planning documents are located in the /docs folder.

Entity Relationship Diagram (ERD): Designed a relational database with 8 core entities (Users, Events, Categories, Enrolments, Results, Weather, etc.).

API Endpoint Plan: Planned over 45 RESTful endpoints with defined roles and status codes.

SQL Script: A complete SQL Server script to create, constrain, and seed the database with realistic South African event data.

### Part 2 - RESTful API
The RaceDay.Api project implements the planned RESTful API using:

Entity Framework Core (Code First) for database management.

Session-based Authentication using custom password hashing (PBKDF2) and session cookies.

Role-Based Access Control (RBAC) via custom Authorization filters ([OrganiserOnly], [ParticipantOnly]).

Swagger/OpenAPI integration for API documentation and testing.

### Part 3 - MVC Web Application
The RaceDay.Mvc project acts as the front-end. It strictly adheres to the architecture rules:

It does not connect to SQL Server directly.

It does not connect to Azure directly.

It uses an IHttpClientFactory wrapped in a custom RaceDayApiClient service to communicate with the Part 2 API. It extracts the API session cookie from the login response and forwards it on all subsequent requests.

### Prerequisites
To run this project, you need:

Visual Studio 2022 (or later)

.NET 8 SDK or .NET 10 SDK (depending on your project configuration)

SQL Server Express / LocalDB

Docker Desktop (for the containerised MVC app)

An Azure Storage Account (for Blob Storage)

### Database Setup
Open SQL Server Management Studio (SSMS).

Ensure you have a local instance running (e.g., .\SQLEXPRESS).

Open a terminal in the RaceDay.Api directory and run:

bash
dotnet ef database update
(Alternatively, run the docs/raceday-database.sql script directly in SSMS).

### How to Run RaceDay.Api
To run the API locally:

Open the solution in Visual Studio.

Set RaceDay.Api as the Startup Project.

Press F5. The Swagger UI will open automatically at your localhost port.
(For Docker testing, it is recommended to run the API over HTTP on port 5000 so the container can reach it: dotnet run --project RaceDay.Api --urls http://0.0.0.0:5000)

### How to Run RaceDay.Mvc Locally
Ensure the API is running.

Set RaceDay.Mvc as the Startup Project.

Press F5.

The MVC app will load in your browser. Use the navigation bar to register or login.

### Azure Blob Storage Setup
File uploads go through the API, not the MVC app.

Create an Azure Storage Account.

Create a container named raceday-images and set its access level to Blob (anonymous read access for blobs only).

In RaceDay.Api, right-click the project and select Manage User Secrets.

Add your connection string:

json
{
  "AzureBlobStorage": {
    "ConnectionString": "YOUR_AZURE_CONNECTION_STRING",
    "ContainerName": "raceday-images"
  }
}

### Docker Setup
The MVC front-end is containerised using Docker.

### Build Image
Open a terminal in the solution root (where the .slnx file is) and run:

bash
docker build -f RaceDay.Mvc/Dockerfile -t raceday-mvc:part3 .

### Run Container
Ensure the API is running on your host machine at port 5000. Then run:

bash
docker run -d -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5000/ --name raceday-mvc raceday-mvc:part3
Access the containerised application at: http://localhost:8080

### Authentication and Session Flow
The user submits credentials on the MVC Login page.

The MVC RaceDayApiClient sends a POST request to api/auth/login.

The API validates the hashed password and sets an ASP.NET Core session cookie.

The MVC app extracts this cookie from the Set-Cookie header and saves it in its own session.

On every subsequent API request, the MVC client attaches this cookie to the Cookie header to prove the user's identity and role.

### Organiser Features
Dashboard: Summary of created events.

Event Management: Full CRUD operations.

Azure Integration: Upload banner images.

Categories: Create event distances and limits.

Results: View enrolled participants and capture finish times.

### Participant Features
Event Browsing: Filter by name or location.

Enrolment: Enter events based on available categories.

My Events: Track registered events.

My Results: View finish times and overall positions.

Profile: Manage personal info and upload a profile picture (Azure).

### CI/CD
This project uses GitHub Actions for Continuous Integration. The workflow (ci.yml) automatically:

Restores NuGet packages.

Builds the entire solution.

Runs all xUnit unit tests.

### GitHub Actions Screenshot
CI/CD
GitHub Actions workflow: .github/workflows/ci.yml
![alt text](latest_workflow)
