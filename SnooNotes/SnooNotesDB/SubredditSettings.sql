CREATE TABLE [dbo].[SubredditSettings]
(
	[SubRedditID] INT NOT NULL PRIMARY KEY, 
	[AccessMask] SMALLINT NOT NULL, 
    [LastUpdated] DATETIME NOT NULL DEFAULT GETUTCDATE() , 
    [PermBanID] INT NULL, 
    [TempBanID] INT NULL, 
    CONSTRAINT [FK_SubredditSettings_Subreddit] FOREIGN KEY ([SubRedditID]) REFERENCES SubReddits([SubRedditID]), 
    CONSTRAINT [FK_SubredditSettings_NoteTypes_PermBan] FOREIGN KEY ([PermBanID]) REFERENCES NoteTypes([NoteTypeID]),
	CONSTRAINT [FK_SubredditSettings_NoteTypes_TempBan] FOREIGN KEY ([TempBanID]) REFERENCES NoteTypes([NoteTypeID]), 

)
