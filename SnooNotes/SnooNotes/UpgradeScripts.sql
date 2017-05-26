update notetypes
set ColorCode = (LEFT(ColorCode,1)+Left(ColorCode,1)+SUBSTRING(ColorCode,2,1)+SUBSTRING(ColorCode,2,1)+RIGHT(ColorCode,1)+RIGHT(ColorCode,1))
where LEN(ColorCode) = 3;
GO

update nt
set IconString = 'thumb_up'
from notetypes nt inner join
subreddits s on s.subredditid = nt.subredditid
where s.SubName = 'SpamCabal'
and nt.DisplayName = 'Good'
GO

update nt
set IconString = 'flag'
from notetypes nt inner join
subreddits s on s.subredditid = nt.subredditid
where s.SubName = 'SpamCabal'
and nt.DisplayName = 'Spam Watch'
GO

update nt
set IconString = 'warning'
from notetypes nt inner join
subreddits s on s.subredditid = nt.subredditid
where s.SubName = 'SpamCabal'
and nt.DisplayName = 'Abuse Warning'
GO


  DROP TABLE [dbo].[__MigrationHistory]
DROP TABLE [dbo].[AspNetRoles]
DROP TABLE [dbo].aspnetuserclaims
drop table [dbo].aspnetuserlogins
drop table [dbo].aspnetusers

GO