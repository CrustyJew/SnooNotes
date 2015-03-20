CREATE TABLE [dbo].[Subreddits]
(
	[SubredditID] INT NOT NULL PRIMARY KEY, 
    [SubName] NVARCHAR(50) NOT NULL, 
    [AppID] NVARCHAR(50) NOT NULL, 
    [PrivateKey] NVARCHAR(50) NOT NULL
)
