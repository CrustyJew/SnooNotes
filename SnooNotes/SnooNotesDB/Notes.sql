CREATE TABLE [dbo].[Notes]
(
	[NoteID] INT NOT NULL PRIMARY KEY NONCLUSTERED IDENTITY, 
    [NoteTypeID] INT NOT NULL,
	[SubredditID] INT NOT NULL,
	
    [Submitter] NVARCHAR(50) NOT NULL, 
    [Message] NVARCHAR(500) NOT NULL, 
    [AppliesToUsername] NVARCHAR(50) NOT NULL, 
    [Url] NVARCHAR(250) NULL, 
    [Timestamp] DATETIME NULL, 
    CONSTRAINT [FK_Notes_Subreddit] FOREIGN KEY ([SubRedditID]) REFERENCES SubReddits([SubRedditID]),
	CONSTRAINT [FK_Notes_NoteType] FOREIGN KEY ([NoteTypeID]) REFERENCES NoteTypes([NoteTypeID])
)

GO

CREATE CLUSTERED INDEX [IX_Notes_SubredditID] ON [dbo].[Notes] ([SubredditID])
