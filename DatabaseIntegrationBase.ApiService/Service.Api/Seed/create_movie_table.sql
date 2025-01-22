USE master;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'movie' AND type = 'U')
BEGIN
    CREATE TABLE movie (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(255),
        Gender NVARCHAR(255),
        IsActive BIT NOT NULL
    );
END;
GO