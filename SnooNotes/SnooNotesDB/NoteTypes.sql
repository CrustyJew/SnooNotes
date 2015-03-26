CREATE TABLE [dbo].[NoteTypes]
(
	[NoteTypeID] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SubredditID] INT NOT NULL, 
    [DisplayName] NVARCHAR(25) NOT NULL, 
    [ColorCode] NVARCHAR(6) NOT NULL, 
    [DisplayOrder] INT NOT NULL, 
    [Bold] BIT NOT NULL, 
    [Italic] BIT NOT NULL, 
    CONSTRAINT [FK_NoteTypes_Subreddit] FOREIGN KEY ([SubRedditID]) REFERENCES SubReddits([SubRedditID])
)
