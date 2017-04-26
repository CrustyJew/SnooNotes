CREATE TABLE [dbo].[BotBannedUsers]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SubredditID] [int] NOT NULL,
	[UserName] [varchar](100) NOT NULL,
	[BannedBy] [varchar](50) NOT NULL,
	[BanReason] [varchar](255) NULL,
	[BanDate] [datetime] NOT NULL,
	[ThingUrl] [varchar](max) NULL, 
    CONSTRAINT [FK_BotBannedUsers_Subreddits] FOREIGN KEY (SubredditID) REFERENCES Subreddits([SubredditID]), 
    CONSTRAINT [PK_BotBannedUsers] PRIMARY KEY ([ID]),
)

GO
