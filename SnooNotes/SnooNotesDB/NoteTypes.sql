CREATE TABLE [dbo].[NoteTypes]
(
	[NoteTypeID] INT NOT NULL PRIMARY KEY NONCLUSTERED IDENTITY, 
    [SubredditID] INT NOT NULL, 
    [DisplayName] NVARCHAR(25) NOT NULL, 
    [ColorCode] NVARCHAR(6) NOT NULL, 
    [DisplayOrder] INT NOT NULL, 
    [Bold] BIT NOT NULL, 
    [Italic] BIT NOT NULL, 
    [Disabled] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_NoteTypes_Subreddit] FOREIGN KEY ([SubRedditID]) REFERENCES SubReddits([SubRedditID]) 
)

GO


CREATE CLUSTERED INDEX [IX_NoteTypes_SubredditID] ON [dbo].[NoteTypes] ([SubredditID])
