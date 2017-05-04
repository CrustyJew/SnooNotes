CREATE TABLE [dbo].[Subreddits]
(
	[SubredditID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [SubName] NVARCHAR(50) NOT NULL UNIQUE CLUSTERED, 
    [Active] BIT NOT NULL, 
	[SentinelEnabled] BIT NOT NULL DEFAULT 0,
    [DirtbagUrl] NVARCHAR(500) NULL, 
    [DirtbagUsername] NVARCHAR(100) NULL, 
    [DirtbagPassword] NVARCHAR(100) NULL 
)

GO

