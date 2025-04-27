# DrinkDb Authentication

A Windows desktop application providing authentication services for DrinkDb. Built with WinUI 3 and .NET 8, this application allows users to authenticate using various OAuth providers including GitHub, Google, Facebook, LinkedIn, and Twitter.

## Features

- **Multiple OAuth Providers**: Support for GitHub, Google, Facebook, LinkedIn, and Twitter authentication
- **Local OAuth Servers**: Integrated local servers for handling OAuth redirects
- **User Management**: Complete user account system with roles and permissions
- **Session Management**: Secure session handling and persistence
- **SQL Database Integration**: User data stored in SQL Server
- **Role-Based Access Control**: Fine-grained permissions management based on user roles
- **Two-Factor Authentication**: Support for 2FA security

## Requirements

- Windows 10/11 (minimum version 10.0.17763.0)
- .NET 8.0
- SQL Server database
- Visual Studio 2022 with Windows App SDK development tools

## Setup

1. Clone the repository
   ```
   git clone https://github.com/yourusername/DrinkDb_Auth.git
   cd DrinkDb_Auth
   ```

2. Configure OAuth credentials
   - Create application registrations with the OAuth providers (GitHub, Google, Facebook, LinkedIn, Twitter)
   - Update `App.config` with your client IDs and secrets
   - Configure redirect URLs for each OAuth provider to point to the appropriate local server ports:
     - GitHub: http://localhost:8890/
     - Facebook: http://localhost:8888/
     - LinkedIn: http://localhost:8891/
     - Google: Uses WebView2-based authentication

3. Set up the database
   - Run the SQL scripts in the `Database` folder to create the necessary tables:
     - `CreateUserRolePermissionTables.sql`: Creates tables for users, roles, permissions, and their relationships
     - `CreateSessionTable.sql`: Creates tables for session management
   - Ensure your connection string is properly configured in `App.config` or `App.config.user`

4. Build and run the application
   ```
   dotnet build
   dotnet run
   ```

## Architecture

### Authentication Flow

1. **User initiates login**: User selects an authentication method (credentials or OAuth provider)
2. **OAuth flow (if applicable)**:
   - Application redirects to the provider's authorization page
   - User consents to requested permissions
   - Provider redirects back to local server with authorization code
   - Local server exchanges code for access token
   - Application retrieves user profile from provider
3. **User verification**:
   - System checks if user exists in the database
   - If not, a new user account is created (for OAuth users)
4. **Session creation**:
   - System generates a new session ID
   - Session details are stored in the database
   - User is redirected to the success page

### Data Model

The application uses the following core data models:

- **User**: Represents a user account with properties like UserId, Username, PasswordHash, and TwoFASecret
- **Role**: Represents a user role (e.g., Admin, User)
- **Permission**: Defines a specific permission with resource and action attributes
- **Session**: Stores session information including tokens and expiration time

### Security Features

- **OAuth Token Management**: Secure handling of OAuth tokens
- **Role-Based Access Control**: Fine-grained permission system
- **Two-Factor Authentication**: Additional security layer
- **Session Management**: Secure session handling with timeout mechanisms

## Database Schema

The application uses the following SQL Server tables:

- **Users**: Stores user account information
  ```sql
  CREATE TABLE Users (
      userId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
      userName NVARCHAR(50) NOT NULL UNIQUE,
      passwordHash NVARCHAR(255),
      twoFASecret NVARCHAR(255)
  );
  ```

- **Roles**: Defines user roles
  ```sql
  CREATE TABLE Roles (
      roleId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
      roleName NVARCHAR(50) NOT NULL UNIQUE
  );
  ```

- **Permissions**: Defines system permissions
  ```sql
  CREATE TABLE Permissions (
      permissionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
      permissionName NVARCHAR(50) NOT NULL,
      resource NVARCHAR(100) NOT NULL,
      action NVARCHAR(50) NOT NULL
  );
  ```

- **RolePermissions**: Maps roles to permissions
  ```sql
  CREATE TABLE RolePermissions (
      roleId UNIQUEIDENTIFIER NOT NULL,
      permissionId UNIQUEIDENTIFIER NOT NULL,
      PRIMARY KEY (roleId, permissionId),
      FOREIGN KEY (roleId) REFERENCES Roles(roleId),
      FOREIGN KEY (permissionId) REFERENCES Permissions(permissionId)
  );
  ```

- **UserRoles**: Maps users to roles
  ```sql
  CREATE TABLE UserRoles (
      userId UNIQUEIDENTIFIER NOT NULL,
      roleId UNIQUEIDENTIFIER NOT NULL,
      PRIMARY KEY (userId, roleId),
      FOREIGN KEY (userId) REFERENCES Users(userId),
      FOREIGN KEY (roleId) REFERENCES Roles(roleId)
  );
  ```

- **Sessions**: Stores active user sessions
  ```sql
  CREATE TABLE Sessions (
      sessionId UNIQUEIDENTIFIER PRIMARY KEY,
      userId UNIQUEIDENTIFIER NOT NULL,
      createdAt DATETIME NOT NULL,
      expiresAt DATETIME NOT NULL,
      active BIT NOT NULL DEFAULT 1,
      FOREIGN KEY (userId) REFERENCES Users(userId)
  );
  ```

## Advanced Configuration

### App.config Options

The application uses an App.config file for configuration settings including:

- Database connection strings
- OAuth client IDs and secrets
- Timeout settings
- Feature toggles

Example configuration:
```xml
<configuration>
  <connectionStrings>
    <add name="DrinkDbConnection" connectionString="Server=myServer;Database=DrinkDB_Dev;Trusted_Connection=True;" />
  </connectionStrings>
  <appSettings>
    <add key="GitHubClientId" value="your-github-client-id" />
    <add key="GitHubClientSecret" value="your-github-client-secret" />
    <!-- Additional settings -->
  </appSettings>
</configuration>
```

### OAuth Provider Customization

Each OAuth provider can be customized by modifying the corresponding provider class in the `OAuthProviders` directory:

- `GitHubOAuth2Provider.cs`
- `GoogleOAuth2Provider.cs`
- `FacebookOAuth2Provider.cs`
- `LinkedInOAuth2Provider.cs`
- `TwitterOAuth2Provider.cs`

## Troubleshooting

### Common Issues

1. **OAuth Authentication Failure**
   - Verify client ID and secret are correct
   - Ensure redirect URLs match those registered with the provider
   - Check that all local OAuth servers are running

2. **Database Connection Issues**
   - Verify connection string in App.config
   - Ensure SQL Server is running and accessible
   - Check that database tables are properly created

3. **UI Rendering Problems**
   - Update to the latest version of the Windows App SDK
   - Verify WinUI 3 dependencies
   - Check Windows version compatibility


## Usage

1. Launch the application
2. Choose your preferred authentication method:
   - Username and password
   - GitHub
   - Google
   - Facebook
   - LinkedIn
   - Twitter
3. Complete the authentication flow
4. Upon successful authentication, you'll be redirected to the success page

## Project Structure

- **OAuthProviders/**: Contains implementation for various OAuth providers
- **Model/**: Data models for users, roles, permissions, and sessions
- **Database/**: SQL scripts for database setup
- **Assets/**: Application icons and images
- **Service/**: Core business logic implementation
- **Adapter/**: Database and external service adapters
