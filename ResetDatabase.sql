USE [AdventureWorks2012]
GO

IF(EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory'))
BEGIN
	DROP TABLE [dbo].[__MigrationHistory]
END
GO

IF(EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__TransactionHistory'))
BEGIN
	DROP TABLE [dbo].[__TransactionHistory]
END
GO

IF(EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_Rating'))
BEGIN
	DROP INDEX [IX_Rating] ON [HumanResources].[Department]
END
GO

DECLARE @ObjectName NVARCHAR(100)
SELECT @ObjectName = OBJECT_NAME([default_object_id]) FROM SYS.COLUMNS
WHERE [object_id] = OBJECT_ID('[HumanResources].[Department]') AND [name] = 'Rating';
IF(@ObjectName IS NOT NULL) 
BEGIN 
	EXEC('ALTER TABLE [HumanResources].[Department] DROP CONSTRAINT ' + @ObjectName)
END
GO

IF(EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME = 'RATING' AND TABLE_NAME = 'Department' AND TABLE_SCHEMA = 'HumanResources'))
BEGIN
	ALTER TABLE [HumanResources].[Department] DROP COLUMN [Rating]
END
GO