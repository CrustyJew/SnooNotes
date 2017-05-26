CREATE TABLE [dbo].[BotBannedUsers_Audit]
(
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SubredditID] [int] NOT NULL,
	[UserName] [varchar](100) NOT NULL,
	[BannedBy] [varchar](50) NOT NULL,
	[BanReason] [varchar](255) NULL,
	[BanDate] [datetime] NOT NULL,
	[ThingUrl] [varchar](max) NULL, 
    [AdditionalInfoOld] NVARCHAR(MAX) NULL, 
	[AdditionalInfoNew] NVARCHAR(MAX) NULL,
    [ModifiedDate] DATETIME NULL, 
    [ModifiedBy] VARCHAR(50) NULL, 
    [AuditAction] CHAR NOT NULL, 
    CONSTRAINT [PK_BotBannedUsers_Audit] PRIMARY KEY ([ID])
)
