use DrinkDB_Dev
-- Create the following functions in the database
-- Get user by id
CREATE OR ALTER FUNCTION fnGetUserById
(
    @userId UNIQUEIDENTIFIER
)
RETURNS TABLE
AS
RETURN
    SELECT *
    FROM Users
    WHERE userId = @userId;
GO


--Get user by username
CREATE OR ALTER FUNCTION fnGetUserByUsername
(
    @username NVARCHAR(50)
)
RETURNS TABLE
AS
RETURN
    SELECT *
    FROM Users
    WHERE userName = @username;
GO
