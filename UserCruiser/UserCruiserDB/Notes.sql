CREATE TABLE [dbo].[Notes]
(
	[NoteID] INT NOT NULL PRIMARY KEY, 
    [NoteTypeID] INT NOT NULL,
	[SubredditID] INT NOT NULL,
	
    [Submitter] NVARCHAR(50) NOT NULL, 
    [Message] NVARCHAR(500) NOT NULL, 
    CONSTRAINT [FK_Notes_Subreddit] FOREIGN KEY ([SubRedditID]) REFERENCES SubReddits([SubRedditID]),
	CONSTRAINT [FK_Notes_NoteType] FOREIGN KEY ([NoteTypeID]) REFERENCES NoteTypes([NoteTypeID])
)
