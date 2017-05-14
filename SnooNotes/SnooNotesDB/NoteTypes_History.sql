CREATE TABLE [dbo].[NoteTypes_History]
(
	[HistId] INT NOT NULL PRIMARY KEY NONCLUSTERED IDENTITY,
	[HistTimestamp] DATETIME NOT NULL,
	[HistAction] varchar(1) NOT NULL,
	[HistUser] nvarchar(50) NOT NULL,
	[NoteTypeID] INT NOT NULL, 
    [SubredditID] INT NOT NULL, 
    [DisplayName] NVARCHAR(25) NOT NULL, 
    [ColorCode] NVARCHAR(6) NOT NULL, 
    [DisplayOrder] INT NOT NULL, 
    [Bold] BIT NOT NULL, 
    [Italic] BIT NOT NULL, 
    [Disabled] BIT NOT NULL DEFAULT 0, 
    [IconString] VARCHAR(50) NULL, 
   
   )
GO


CREATE CLUSTERED INDEX [IX_NoteTypes_History_SubredditID] ON [dbo].[NoteTypes_History] ([SubredditID])