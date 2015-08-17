CREATE TABLE [dbo].[Notes_History]
(
	[HistId] INT NOT NULL PRIMARY KEY NONCLUSTERED IDENTITY,
	[HistTimestamp] DATETIME NOT NULL,
	[HistAction] varchar(1) NOT NULL,
	[HistUser] nvarchar(50) NOT NULL,
	[NoteId] INT NOT NULL,
    [NoteTypeID] INT NOT NULL,
	[SubredditID] INT NOT NULL,
    [Submitter] NVARCHAR(50) NOT NULL, 
    [Message] NVARCHAR(500) NOT NULL, 
    [AppliesToUsername] NVARCHAR(50) NOT NULL, 
    [Url] NVARCHAR(250) NULL, 
    [Timestamp] DATETIME NULL, 
)

GO

CREATE CLUSTERED INDEX [IX_Notes_History_SubredditID] ON [dbo].[Notes_History] ([SubredditID])
