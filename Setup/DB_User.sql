USE [master]

GO

USE [ManagementExamDB]

GO

CREATE USER [ExamDBUser] FOR LOGIN [ExamDBUser];

GO

EXEC sp_addrolemember 'db_owner', 'ExamDBUser';

GO