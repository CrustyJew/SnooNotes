CREATE TABLE [dbo].[Subreddits]
(
	[SubredditID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [SubName] NVARCHAR(50) NOT NULL, 
    [AppID] NVARCHAR(25) NOT NULL, 
    [PrivateKey] NVARCHAR(50) NOT NULL, 
    [Active] BIT NOT NULL
)
