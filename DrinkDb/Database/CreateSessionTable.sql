CREATE TABLE Sessions (
    sessionId UNIQUEIDENTIFIER PRIMARY KEY,
    userId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (userId) REFERENCES Users(userId)
);
GO

CREATE OR ALTER PROCEDURE create_session
    @userId UNIQUEIDENTIFIER,
    @sessionId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET @sessionId = NEWID();
    
    INSERT INTO Sessions (sessionId, userId)
    VALUES (@sessionId, @userId);
END;
GO

CREATE OR ALTER PROCEDURE end_session
    @sessionId UNIQUEIDENTIFIER
AS
BEGIN
    DELETE FROM Sessions
    WHERE sessionId = @sessionId;
    
    RETURN @@ROWCOUNT;
END;
GO 