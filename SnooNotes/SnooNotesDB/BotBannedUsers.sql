CREATE TABLE [dbo].[BotBannedUsers]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SubredditID] [int] NOT NULL,
	[UserName] [varchar](100) NOT NULL,
	[BannedBy] [varchar](50) NOT NULL,
	[BanReason] [varchar](255) NULL,
	[BanDate] [datetime] NOT NULL,
	[ThingUrl] [varchar](max) NULL, 
    [AdditionalInfo] NVARCHAR(MAX) NULL, 
    [ModifiedDate] DATETIME NULL, 
    [ModifiedBy] VARCHAR(50) NULL, 
    CONSTRAINT [FK_BotBannedUsers_Subreddits] FOREIGN KEY (SubredditID) REFERENCES Subreddits([SubredditID]), 
    CONSTRAINT [PK_BotBannedUsers] PRIMARY KEY ([ID]), 
    CONSTRAINT [AK_BotBannedUsers_SubredditID_UserName] UNIQUE ([SubredditID],[UserName]) 
)

GO


CREATE TRIGGER [dbo].[Trigger_BotBannedUsers_Audit_Update]
    ON [dbo].[BotBannedUsers]
    FOR UPDATE
    AS
    BEGIN
        SET NoCount ON
		declare @oldInfo nvarchar(max);
		select @oldInfo = AdditionalInfo from deleted;
		insert into BotBannedUsers_Audit(SubredditID, UserName, BannedBy, BanDate, BanReason, ThingUrl, AdditionalInfoOld, AdditionalInfoNew, ModifiedDate, ModifiedBy, AuditAction)
		select SubredditID, UserName, BannedBy, BanDate, BanReason, ThingUrl, @oldInfo, AdditionalInfo, ModifiedDate, ModifiedBy, 'U'  from inserted
    END
GO
