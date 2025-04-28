# Combined Project Integration Guide

This project is a combination of the App1 and DrinkDb_Auth projects into a single WinUI application.

## Project Structure

- **Assets**: Contains all image and icon resources
- **Authentication**: Code from DrinkDb_Auth related to authentication
- **Database**: Database access code from both projects
- **Models**: Data models from both projects
- **Services**: Services and utilities from both projects
- **Views**: UI pages and controls

## Integration Steps

### 1. Copy Assets

Copy the Assets from both projects:

- Copy from `Admin/App1/Assets` to `CombinedProject/Assets`
- Copy from `Authentication/Implementation/Assets` to `CombinedProject/Assets` (resolve naming conflicts)

### 2. Copy Authentication Code

Copy the authentication code:

- Copy all files from `Authentication/Implementation/AuthProviders` to `CombinedProject/Authentication/Providers`
- Copy all XAML views related to authentication to `CombinedProject/Views`

### 3. Copy Database Code

Migrate database access code:

- Copy database access code from both projects to `CombinedProject/Database`
- Unify connection strings and database access patterns

### 4. Copy Models and Services

Migrate model classes:

- Copy model classes from both projects to `CombinedProject/Models`
- Copy service implementations to `CombinedProject/Services`

### 5. Copy Views

Copy UI components:

- Copy XAML views from `Admin/App1/Views` to `CombinedProject/Views`
- Copy any additional views from both projects

### 6. Create Page Navigation

Implement the page navigation:

1. Create actual page classes for the navigation items in MainWindow.xaml.cs
2. Update the page type mapping in the NavigateToPage method

### 7. Resolve Conflicts

- Check for naming conflicts across the codebase
- Ensure consistent namespaces (rename namespace references to `CombinedProject`)
- Resolve any duplicate functionality between the two projects

### 8. Test

- Build the project and fix any compile errors
- Test all functionality from both original applications

## Additional Notes

- You may need to adjust XAML references to resources
- Update any hard-coded references to the original application names
- Both projects use StyleCop - maintain consistent code styling
- Both projects were targeting .NET 8.0 and Windows 10, so there should be no framework compatibility issues
