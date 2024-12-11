USE [master]
GO

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'ExamDBUser')
BEGIN
    CREATE LOGIN [ExamDBUser] WITH PASSWORD = N'ExamDBPassword';
END
GO

-- Script for each database
DECLARE @dbName NVARCHAR(128)

-- List of databases
DECLARE db_cursor CURSOR FOR
SELECT name
FROM sys.databases
WHERE name IN ('AccountExamDB', 'FeedbackExamDB', 'ManagementExamDB', 'OrderExamDB', 'SearchExamDB')

OPEN db_cursor
FETCH NEXT FROM db_cursor INTO @dbName

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Use the current database
    EXEC('USE [' + @dbName + '];')
    
    -- Create user for the login in the database
    EXEC('CREATE USER [ExamDBUser] FOR LOGIN [ExamDBUser];')
    
    -- Add the user to db_owner role
    EXEC('EXEC sp_addrolemember ''db_owner'', ''ExamDBUser'';')
    
    FETCH NEXT FROM db_cursor INTO @dbName
END

CLOSE db_cursor
DEALLOCATE db_cursor

GO