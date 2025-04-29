use DrinkDB_Dev
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Permissions' AND xtype='U')
BEGIN
    CREATE TABLE Permissions (
        permissionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        permissionName NVARCHAR(50) NOT NULL,
        resource NVARCHAR(100) NOT NULL,
        action NVARCHAR(50) NOT NULL
    );
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE Roles (
        roleId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        roleName NVARCHAR(50) NOT NULL UNIQUE,
    );
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RolePermissions' AND xtype='U')
BEGIN
    CREATE TABLE RolePermissions (
        roleId UNIQUEIDENTIFIER NOT NULL,
        permissionId UNIQUEIDENTIFIER NOT NULL,
        PRIMARY KEY (roleId, permissionId),
        FOREIGN KEY (roleId) REFERENCES Roles(roleId),
        FOREIGN KEY (permissionId) REFERENCES Permissions(permissionId)
    );
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserRoles' AND xtype='U')
BEGIN
    CREATE TABLE UserRoles (
        userId UNIQUEIDENTIFIER NOT NULL,
        roleId UNIQUEIDENTIFIER NOT NULL,
        PRIMARY KEY (userId, roleId),
        FOREIGN KEY (userId) REFERENCES Users(userId),
        FOREIGN KEY (roleId) REFERENCES Roles(roleId)
    );
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        userId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        userName NVARCHAR(50) NOT NULL UNIQUE,
        passwordHash NVARCHAR(255),
        twoFASecret NVARCHAR(255),
    );
END