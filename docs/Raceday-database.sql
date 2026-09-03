
-- CLEAN START: Drop existing database if it exists

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RaceDayDB')
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END
GO

-- Create fresh database
CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO


-- CREATE ALL TABLES


-- 1. ROLE table
CREATE TABLE [Role] (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- 2. USER table
CREATE TABLE [User] (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20),
    RoleID INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleID) REFERENCES [Role](RoleID)
);
GO

-- 3. EVENT table
CREATE TABLE [Event] (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Location NVARCHAR(255) NOT NULL,
    Province NVARCHAR(50) NOT NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    OrganiserID INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Event_User FOREIGN KEY (OrganiserID) REFERENCES [User](UserID),
    CONSTRAINT CHK_Event_Dates CHECK (EndDate >= StartDate)
);
GO

-- 4. CATEGORY table
CREATE TABLE [Category] (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- 5. EVENTCATEGORY table
CREATE TABLE [EventCategory] (
    EventCategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    RegistrationStart DATETIME NOT NULL,
    RegistrationEnd DATETIME NOT NULL,
    MaxParticipants INT NOT NULL DEFAULT 100,
    CurrentParticipants INT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_EventCategory_Event FOREIGN KEY (EventID) REFERENCES [Event](EventID) ON DELETE CASCADE,
    CONSTRAINT FK_EventCategory_Category FOREIGN KEY (CategoryID) REFERENCES [Category](CategoryID),
    CONSTRAINT UQ_EventCategory_EventCategory UNIQUE (EventID, CategoryID),
    CONSTRAINT CHK_RegistrationDates CHECK (RegistrationEnd >= RegistrationStart)
);
GO

-- 6. ENROLMENT table
CREATE TABLE [Enrolment] (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventCategoryID INT NOT NULL,
    EnrolmentDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    PaymentStatus NVARCHAR(20) NOT NULL DEFAULT 'Unpaid',
    RaceNumber NVARCHAR(20) UNIQUE,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Enrolment_User FOREIGN KEY (ParticipantID) REFERENCES [User](UserID),
    CONSTRAINT FK_Enrolment_EventCategory FOREIGN KEY (EventCategoryID) REFERENCES [EventCategory](EventCategoryID),
    CONSTRAINT CHK_Enrolment_Status CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')),
    CONSTRAINT CHK_Enrolment_PaymentStatus CHECK (PaymentStatus IN ('Unpaid', 'Paid', 'Refunded'))
);
GO

-- 7. RESULT table
CREATE TABLE [Result] (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL,
    EventCategoryID INT NOT NULL,
    FinishTime TIME,
    OverallPosition INT,
    CategoryPosition INT,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Result_Enrolment FOREIGN KEY (EnrolmentID) REFERENCES [Enrolment](EnrolmentID),
    CONSTRAINT FK_Result_EventCategory FOREIGN KEY (EventCategoryID) REFERENCES [EventCategory](EventCategoryID),
    CONSTRAINT UQ_Result_Enrolment UNIQUE (EnrolmentID),
    CONSTRAINT CHK_Result_Status CHECK (Status IN ('Pending', 'Completed', 'DNF', 'DNS'))
);
GO

-- 8. WEATHER table
CREATE TABLE [Weather] (
    WeatherID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    ForecastDateTime DATETIME NOT NULL,
    Temperature DECIMAL(5,2),
    Condition NVARCHAR(50),
    Humidity INT,
    WindSpeed DECIMAL(5,2),
    WindDirection NVARCHAR(20),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Weather_Event FOREIGN KEY (EventID) REFERENCES [Event](EventID) ON DELETE CASCADE
);
GO

-- ============================================================
-- CREATE INDEXES
-- ============================================================

CREATE INDEX IX_User_Email ON [User](Email);
CREATE INDEX IX_User_RoleID ON [User](RoleID);
CREATE INDEX IX_Event_OrganiserID ON [Event](OrganiserID);
CREATE INDEX IX_Event_Province ON [Event](Province);
CREATE INDEX IX_Event_StartDate ON [Event](StartDate);
CREATE INDEX IX_EventCategory_EventID ON [EventCategory](EventID);
CREATE INDEX IX_EventCategory_CategoryID ON [EventCategory](CategoryID);
CREATE INDEX IX_Enrolment_ParticipantID ON [Enrolment](ParticipantID);
CREATE INDEX IX_Enrolment_EventCategoryID ON [Enrolment](EventCategoryID);
CREATE INDEX IX_Result_EventCategoryID ON [Result](EventCategoryID);
CREATE INDEX IX_Result_EnrolmentID ON [Result](EnrolmentID);
CREATE INDEX IX_Weather_EventID ON [Weather](EventID);
CREATE INDEX IX_Weather_ForecastDateTime ON [Weather](ForecastDateTime);
GO


-- INSERT SAMPLE DATA


-- Insert Roles
INSERT INTO [Role] (RoleName, Description) VALUES
('Administrator', 'Full system access and management'),
('Organiser', 'Can create and manage events'),
('Participant', 'Can register for events and view results');
GO

-- Insert Users
INSERT INTO [User] (Email, PasswordHash, FirstName, LastName, PhoneNumber, RoleID) VALUES
('thabo.mokoena@raceday.co.za', 'hashed_password_123', 'Thabo', 'Mokoena', '+27 82 123 4567', 2),
('lindiwe.sithole@raceday.co.za', 'hashed_password_456', 'Lindiwe', 'Sithole', '+27 73 987 6543', 2),
('sipho.ndlovu@gmail.com', 'hashed_password_789', 'Sipho', 'Ndlovu', '+27 71 234 5678', 3),
('zanele.khumalo@gmail.com', 'hashed_password_012', 'Zanele', 'Khumalo', '+27 83 456 7890', 3),
('admin@raceday.co.za', 'hashed_password_admin', 'Admin', 'User', '+27 84 000 0000', 1);
GO

-- Insert Categories
INSERT INTO [Category] (Name, Description) VALUES
('5km Walk', 'A 5-kilometre walking event for all ages and fitness levels'),
('5km Run', 'A 5-kilometre running event for beginners and casual runners'),
('10km Walk', 'A 10-kilometre walking event for intermediate walkers'),
('10km Run', 'A 10-kilometre running event for intermediate runners'),
('21km Half Marathon', 'A 21.1-kilometre half marathon for experienced runners'),
('42km Marathon', 'A 42.2-kilometre full marathon for elite runners'),
('50km Cycle', 'A 50-kilometre cycling event for casual cyclists'),
('100km Cycle', 'A 100-kilometre cycling event for experienced cyclists'),
('Kids 1km', 'A 1-kilometre fun run for children under 12');
GO

-- Insert Events
INSERT INTO [Event] (Name, Description, Location, Province, StartDate, EndDate, OrganiserID) VALUES
('Cape Town Cycle Tour 2026', 
 'Africa''s largest timed cycle race, taking riders on a scenic route around the Cape Peninsula.', 
 'Cape Town', 'Western Cape', 
 '2026-03-08 06:00:00', '2026-03-08 18:00:00', 
 1),
('Comrades Marathon 2026', 
 'The ultimate human race - a 90km ultra-marathon between Pietermaritzburg and Durban.', 
 'Pietermaritzburg', 'KwaZulu-Natal', 
 '2026-06-14 05:30:00', '2026-06-14 17:30:00', 
 2),
('Soweto Marathon 2026', 
 'A vibrant marathon through the heart of Soweto, celebrating South African culture.', 
 'Soweto, Johannesburg', 'Gauteng', 
 '2026-11-01 06:00:00', '2026-11-01 14:00:00', 
 1),
('Two Oceans Marathon 2026', 
 'Known as the world''s most beautiful marathon, running through Cape Town''s stunning coastal scenery.', 
 'Cape Town', 'Western Cape', 
 '2026-04-04 06:00:00', '2026-04-04 16:00:00', 
 2),
('Durban Summer Walk 2026', 
 'A community walking event along Durban''s golden mile beachfront.', 
 'Durban', 'KwaZulu-Natal', 
 '2026-12-12 07:00:00', '2026-12-12 12:00:00', 
 1);
GO

-- Insert Event Categories
INSERT INTO [EventCategory] (EventID, CategoryID, EntryFee, RegistrationStart, RegistrationEnd, MaxParticipants) VALUES
(1, 7, 350.00, '2026-01-01 00:00:00', '2026-03-01 23:59:59', 5000),
(1, 8, 450.00, '2026-01-01 00:00:00', '2026-03-01 23:59:59', 3000),
(2, 6, 800.00, '2026-01-01 00:00:00', '2026-05-01 23:59:59', 20000),
(3, 5, 250.00, '2026-08-01 00:00:00', '2026-10-15 23:59:59', 10000),
(3, 6, 400.00, '2026-08-01 00:00:00', '2026-10-15 23:59:59', 5000),
(4, 5, 300.00, '2026-02-01 00:00:00', '2026-03-15 23:59:59', 8000),
(4, 6, 500.00, '2026-02-01 00:00:00', '2026-03-15 23:59:59', 4000),
(5, 1, 100.00, '2026-10-01 00:00:00', '2026-12-01 23:59:59', 2000),
(5, 2, 150.00, '2026-10-01 00:00:00', '2026-12-01 23:59:59', 1500);
GO

-- Insert Enrolments
INSERT INTO [Enrolment] (ParticipantID, EventCategoryID, Status, PaymentStatus, RaceNumber) VALUES
(3, 1, 'Confirmed', 'Paid', 'CTC-1001'),
(3, 3, 'Confirmed', 'Paid', 'COM-2001'),
(3, 5, 'Confirmed', 'Paid', 'SOW-3001'),
(4, 2, 'Confirmed', 'Paid', 'CTC-2001'),
(4, 6, 'Pending', 'Unpaid', 'TWO-1001'),
(4, 7, 'Confirmed', 'Paid', 'DUR-1001');
GO

-- Insert Results (some enrolments don't have results yet)
INSERT INTO [Result] (EnrolmentID, EventCategoryID, FinishTime, OverallPosition, CategoryPosition, Status) VALUES
(1, 1, '02:35:45', 125, 45, 'Completed'),
(2, 3, '07:42:30', 342, 78, 'Completed'),
(3, 5, '01:55:20', 89, 12, 'Completed'),
(4, 2, '04:15:00', 567, 23, 'Completed'),
(6, 7, '00:35:10', 45, 8, 'Completed');
GO

-- Insert Weather Data
INSERT INTO [Weather] (EventID, ForecastDateTime, Temperature, Condition, Humidity, WindSpeed, WindDirection) VALUES
(1, '2026-03-08 06:00:00', 18.5, 'Partly Cloudy', 65, 15.5, 'SE'),
(1, '2026-03-08 09:00:00', 22.0, 'Sunny', 55, 18.0, 'SE'),
(1, '2026-03-08 12:00:00', 25.5, 'Sunny', 45, 20.5, 'S'),
(1, '2026-03-08 15:00:00', 24.0, 'Sunny', 50, 22.0, 'SW'),
(1, '2026-03-08 18:00:00', 20.0, 'Clear', 60, 16.0, 'SW'),
(2, '2026-06-14 05:30:00', 12.0, 'Clear', 70, 5.0, 'NE'),
(2, '2026-06-14 09:00:00', 18.0, 'Sunny', 60, 8.0, 'NE'),
(2, '2026-06-14 12:00:00', 22.0, 'Sunny', 50, 10.0, 'E'),
(2, '2026-06-14 15:00:00', 20.0, 'Sunny', 55, 12.0, 'E'),
(2, '2026-06-14 17:30:00', 16.0, 'Clear', 65, 7.0, 'NE'),
(3, '2026-11-01 06:00:00', 15.0, 'Clear', 70, 3.0, 'N'),
(3, '2026-11-01 09:00:00', 22.0, 'Sunny', 55, 5.0, 'NW'),
(3, '2026-11-01 12:00:00', 26.0, 'Sunny', 45, 8.0, 'NW'),
(3, '2026-11-01 14:00:00', 24.0, 'Partly Cloudy', 50, 10.0, 'W');
GO


-- CREATE VIEWS


CREATE VIEW vw_EventDetails AS
SELECT 
    e.EventID,
    e.Name AS EventName,
    e.Description AS EventDescription,
    e.Location,
    e.Province,
    e.StartDate,
    e.EndDate,
    u.FirstName + ' ' + u.LastName AS OrganiserName,
    u.Email AS OrganiserEmail,
    u.PhoneNumber AS OrganiserPhone
FROM [Event] e
INNER JOIN [User] u ON e.OrganiserID = u.UserID;
GO

CREATE VIEW vw_EventCategories AS
SELECT 
    ec.EventCategoryID,
    e.EventID,
    e.Name AS EventName,
    c.CategoryID,
    c.Name AS CategoryName,
    c.Description AS CategoryDescription,
    ec.EntryFee,
    ec.RegistrationStart,
    ec.RegistrationEnd,
    ec.MaxParticipants,
    ec.CurrentParticipants,
    (ec.MaxParticipants - ec.CurrentParticipants) AS AvailableSpots,
    COUNT(en.EnrolmentID) AS ActualEnrolments
FROM [EventCategory] ec
INNER JOIN [Event] e ON ec.EventID = e.EventID
INNER JOIN [Category] c ON ec.CategoryID = c.CategoryID
LEFT JOIN [Enrolment] en ON ec.EventCategoryID = en.EventCategoryID AND en.Status != 'Cancelled'
GROUP BY 
    ec.EventCategoryID,
    e.EventID,
    e.Name,
    c.CategoryID,
    c.Name,
    c.Description,
    ec.EntryFee,
    ec.RegistrationStart,
    ec.RegistrationEnd,
    ec.MaxParticipants,
    ec.CurrentParticipants;
GO

CREATE VIEW vw_ParticipantEnrolments AS
SELECT 
    u.UserID,
    u.FirstName + ' ' + u.LastName AS ParticipantName,
    u.Email,
    en.EnrolmentID,
    e.EventID,
    e.Name AS EventName,
    c.Name AS CategoryName,
    ec.EntryFee,
    en.EnrolmentDate,
    en.Status AS EnrolmentStatus,
    en.PaymentStatus,
    en.RaceNumber,
    r.FinishTime,
    r.OverallPosition,
    r.CategoryPosition,
    r.Status AS ResultStatus
FROM [User] u
INNER JOIN [Enrolment] en ON u.UserID = en.ParticipantID
INNER JOIN [EventCategory] ec ON en.EventCategoryID = ec.EventCategoryID
INNER JOIN [Event] e ON ec.EventID = e.EventID
INNER JOIN [Category] c ON ec.CategoryID = c.CategoryID
LEFT JOIN [Result] r ON en.EnrolmentID = r.EnrolmentID;
GO

CREATE VIEW vw_EventLeaderboard AS
SELECT 
    e.EventID,
    e.Name AS EventName,
    ec.EventCategoryID,
    c.Name AS CategoryName,
    u.FirstName + ' ' + u.LastName AS ParticipantName,
    r.FinishTime,
    r.OverallPosition,
    r.CategoryPosition,
    r.Status AS ResultStatus
FROM [Result] r
INNER JOIN [Enrolment] en ON r.EnrolmentID = en.EnrolmentID
INNER JOIN [EventCategory] ec ON r.EventCategoryID = ec.EventCategoryID
INNER JOIN [Event] e ON ec.EventID = e.EventID
INNER JOIN [Category] c ON ec.CategoryID = c.CategoryID
INNER JOIN [User] u ON en.ParticipantID = u.UserID
WHERE r.Status = 'Completed';
GO


-- CREATE STORED PROCEDURES


CREATE PROCEDURE sp_GetUpcomingEvents
    @DaysAhead INT = 30
AS
BEGIN
    SELECT 
        EventID,
        Name,
        Location,
        Province,
        StartDate,
        EndDate,
        DATEDIFF(day, GETDATE(), StartDate) AS DaysUntilEvent
    FROM [Event]
    WHERE StartDate >= GETDATE()
        AND StartDate <= DATEADD(day, @DaysAhead, GETDATE())
    ORDER BY StartDate ASC;
END
GO

CREATE PROCEDURE sp_EnrolParticipant
    @ParticipantID INT,
    @EventCategoryID INT,
    @RaceNumber NVARCHAR(20) OUTPUT
AS
BEGIN
    DECLARE @CurrentParticipants INT;
    DECLARE @MaxParticipants INT;
    DECLARE @RegistrationEnd DATETIME;
    
    SELECT @RegistrationEnd = RegistrationEnd 
    FROM [EventCategory] 
    WHERE EventCategoryID = @EventCategoryID;
    
    IF @RegistrationEnd < GETDATE()
    BEGIN
        RAISERROR('Registration for this category is now closed.', 16, 1);
        RETURN;
    END
    
    SELECT 
        @CurrentParticipants = CurrentParticipants,
        @MaxParticipants = MaxParticipants
    FROM [EventCategory] 
    WHERE EventCategoryID = @EventCategoryID;
    
    IF @CurrentParticipants >= @MaxParticipants
    BEGIN
        RAISERROR('This category is now full.', 16, 1);
        RETURN;
    END
    
    IF EXISTS (
        SELECT 1 FROM [Enrolment] 
        WHERE ParticipantID = @ParticipantID 
            AND EventCategoryID = @EventCategoryID 
            AND Status != 'Cancelled'
    )
    BEGIN
        RAISERROR('You are already enrolled in this category.', 16, 1);
        RETURN;
    END
    
    SET @RaceNumber = 'R-' + CAST(@EventCategoryID AS VARCHAR) + '-' + CAST(@ParticipantID AS VARCHAR);
    
    INSERT INTO [Enrolment] (ParticipantID, EventCategoryID, Status, PaymentStatus, RaceNumber)
    VALUES (@ParticipantID, @EventCategoryID, 'Pending', 'Unpaid', @RaceNumber);
    
    UPDATE [EventCategory] 
    SET CurrentParticipants = CurrentParticipants + 1
    WHERE EventCategoryID = @EventCategoryID;
    
    SELECT @RaceNumber AS GeneratedRaceNumber;
END
GO


-- CREATE TRIGGERS


CREATE TRIGGER trg_User_UpdatedAt
ON [User]
AFTER UPDATE
AS
BEGIN
    UPDATE [User]
    SET UpdatedAt = GETDATE()
    WHERE UserID IN (SELECT UserID FROM inserted);
END
GO

CREATE TRIGGER trg_Event_UpdatedAt
ON [Event]
AFTER UPDATE
AS
BEGIN
    UPDATE [Event]
    SET UpdatedAt = GETDATE()
    WHERE EventID IN (SELECT EventID FROM inserted);
END
GO

CREATE TRIGGER trg_Enrolment_UpdatedAt
ON [Enrolment]
AFTER UPDATE
AS
BEGIN
    UPDATE [Enrolment]
    SET UpdatedAt = GETDATE()
    WHERE EnrolmentID IN (SELECT EnrolmentID FROM inserted);
END
GO

CREATE TRIGGER trg_Enrolment_CreateResult
ON [Enrolment]
AFTER UPDATE
AS
BEGIN
    INSERT INTO [Result] (EnrolmentID, EventCategoryID, Status)
    SELECT 
        i.EnrolmentID,
        i.EventCategoryID,
        'Pending'
    FROM inserted i
    INNER JOIN deleted d ON i.EnrolmentID = d.EnrolmentID
    WHERE i.Status = 'Confirmed' 
        AND d.Status != 'Confirmed'
        AND NOT EXISTS (
            SELECT 1 FROM [Result] r 
            WHERE r.EnrolmentID = i.EnrolmentID
        );
END
GO


-- VERIFICATION 


PRINT '============================================';
PRINT '✅ DATABASE SETUP COMPLETE!';
PRINT '============================================';
PRINT '';

-- Show all tables and their row counts
SELECT 'Role' AS TableName, COUNT(*) AS Count FROM [Role]
UNION ALL
SELECT 'User', COUNT(*) FROM [User]
UNION ALL
SELECT 'Event', COUNT(*) FROM [Event]
UNION ALL
SELECT 'Category', COUNT(*) FROM [Category]
UNION ALL
SELECT 'EventCategory', COUNT(*) FROM [EventCategory]
UNION ALL
SELECT 'Enrolment', COUNT(*) FROM [Enrolment]
UNION ALL
SELECT 'Result', COUNT(*) FROM [Result]
UNION ALL
SELECT 'Weather', COUNT(*) FROM [Weather];
GO

PRINT '';
PRINT '============================================';
PRINT 'DATABASE READY FOR USE!';
PRINT '============================================';
PRINT '';
PRINT 'Objects Created:';
PRINT '  - 8 Tables';
PRINT '  - 4 Views';
PRINT '  - 2 Stored Procedures';
PRINT '  - 4 Triggers';
PRINT '  - Sample Data Loaded';
PRINT '';
PRINT 'You can now see everything in Object Explorer';
PRINT 'under Databases > RaceDayDB';
PRINT '============================================';