# RaceDay - Event Management System

## Part 1: System Planning and Database

### Overview
RaceDay is a full-stack web-based event management system designed for the South African road running, walking, and cycling community.

### Project Description
RaceDay is a full-stack web-based event management system designed specifically for the South African road running, walking, and cycling community. The platform addresses the challenges faced by event organisers who still rely on paper-based registration, spreadsheets, and disconnected communication channels.

The Problem: Despite South Africa's rich road events culture—from the iconic Comrades Marathon to the Cape Town Cycle Tour, Soweto Marathon, and hundreds of community events—many organisers struggle with manual registration processes, fragmented participant management, and inadequate race-day preparation tools.

The Solution: RaceDay provides a centralised digital platform where Event Organisers can create and manage events, categories, and participant results, while Participants can browse upcoming events, register, track their personal performance history, and prepare for race day using live weather and route information.

### Participant
Participants are the end-users who engage with events on the platform. Their capabilities include:

Event Discovery: Browse all upcoming events with filtering by province and date

Event Registration: Enrol in events and select specific categories (e.g., 10km Run or 42km Marathon)

Payment Tracking: Monitor payment status for enrolments

Performance History: View personal results across all events, including:

- Finish times

- Overall and category positions

- Race numbers

Race-Day Preparation: Access live weather information and route details for upcoming events

Profile Management: Update personal information and view enrolment history

### Documentation
- **ERD**: `/docs/erd.png` - Entity Relationship Diagram
- **API Plan**: `/docs/api-endpoint-plan.md` - Complete API endpoint specifications
- **Database Script**: `/docs/raceday-database.sql` - SQL Server schema and seed data

### Database Schema
- 8 core tables (Role, User, Event, Category, EventCategory, Enrolment, Result, Weather)
- 3 views for common queries
- 2 stored procedures for common operations
- 4 triggers for data integrity

### Features Implemented
- Role-based user management (Administrator, Organiser, Participant)
- Event creation and management
- Category management for events
- Participant enrolment system
- Results tracking and leaderboards
- Weather information for events

### GitHub Actions
gi(https://github.com/jabu1lani/RaceDay/blob/main/.github/workflows/validate.ym)

l
### Video Walkthrough
[Part 1 Submission Video](https://youtu.be/your-video-link)

### Repository Structure

raceday/
├── docs/
│ ├── erd.png
│ ├── Raceday_API_Endpoint-Plan.md
│ └── Raceday-database.sql
├── .github/
│ └── workflows/
│ └── validate.yml
└── README.md