# UBB-SE-2025-Grupa-4

# Admin Dashboard with Email Reporting

## Features
- Background email job scheduling with Quartz.NET
- Admin report generation
- Gmail SMTP integration
- Environment variable configuration

  ## Install requiered packages
- dotnet add package Quartz
- dotnet add package Quartz.AspNetCore
- dotnet add package MailKit

   ## Gmail setup
- enable 2FA on your Google account
- generate an app password:
  - go to Google App Passwords
  - select "Mail" and create a custom app
  - use the generated password in your environment variables
    
  ### PowerShell Setup
Run these commands to set your environment variables:
    [System.Environment]::SetEnvironmentVariable("SMTP_MODERATOR_EMAIL", "example@gmail.com", "User")
    [System.Environment]::SetEnvironmentVariable("SMTP_MODERATOR_PASSWORD", "your_app_passwovrd", "User") 
- to test the network for emails: ping smtp.gmail.com

  ## AI Check
- requires an API key

  ## Datbase setup 
  - check connection string parameters

  - CREATE DATABASE DrinksImdb;

USE DrinksImdb;
CREATE TABLE UpgradeRequests (
    RequestId INT PRIMARY KEY IDENTITY(1,1),
    RequestingUserId INT NOT NULL,
    RequestingUserName NVARCHAR(100) NOT NULL
);
GO

INSERT INTO UpgradeRequests (RequestingUserId, RequestingUserName) VALUES (3, 'Admin One');
INSERT INTO UpgradeRequests (RequestingUserId, RequestingUserName) VALUES (5, 'Admin Two');


CREATE TABLE OffensiveWords (
    OffensiveWordId INT PRIMARY KEY IDENTITY(1,1),
    Word NVARCHAR(100) NOT NULL
);

INSERT INTO OffensiveWords (Word)
VALUES 
    ('stupid'),
    ('dumb'),
    ('fool'),
    ('loser'),
    ('jerk'),
    ('imbecile'),
    ('dunce'),
    ('bonehead'),
    ('nitwit'),
    ('blockhead');
