CREATE TABLE [dbo].[SubredditSettings]
(
	[SubRedditID] INT NOT NULL PRIMARY KEY, 
	[AccessMask] SMALLINT NOT NULL, 
    CONSTRAINT [FK_SubredditSettings_Subreddit] FOREIGN KEY ([SubRedditID]) REFERENCES SubReddits([SubRedditID]),
)
