CREATE VIEW [dbo].[NoteHashes]
	AS 

	select s.SubredditID, s.SubName, HASHBYTES('MD5',Lower(s.SubName + convert(nvarchar, n.NoteTypeID)  + n.AppliesToUsername + CONVERT(VARCHAR,n.Timestamp,120) + n.Message + n.Url)) as hashcode

  from Notes n 
  inner join Subreddits s on s.SubredditID = n.SubredditID
