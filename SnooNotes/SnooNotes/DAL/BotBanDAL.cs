using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;

namespace SnooNotes.DAL
{
    public class BotBanDAL : IBotBanDAL
    {
        private IDbConnection snConn;
        private IDbConnection sentinelConn;

        public BotBanDAL(IDbConnection banlistConn, IDbConnection channelBanConn)
        {
            snConn = banlistConn;
            sentinelConn = channelBanConn;
        }

        public async Task<bool> BanUser(IEnumerable<Models.BannedEntity> entities)
        {
            string query = @"
insert into BotBannedUsers (SubredditID, UserName, BannedBy, BanReason, BanDate, ThingUrl)
select sub.SubredditID,@UserName,@BannedBy,@BanReason,@BanDate,@ThingURL from Subreddits sub where sub.SubName like @SubName
;";
            try
            {
                await snConn.ExecuteAsync(query, entities);
            }
            catch(SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                    return false;

                throw;
            }
            return true;
            
        }

        public Task<IEnumerable<string>> GetBannedUserNames(string subredditName)
        {
            string query = @"
select be.UserName
from BotBannedUsers be
inner join Subreddits sub on sub.SubredditID = be.SubredditID
where sub.SubName like @subredditName
;";
            return snConn.QueryAsync<string>(query, new { subredditName });
            
        }

        public async Task<Models.TableResults<IEnumerable<Models.BannedEntity>>> SearchBannedUsers(IEnumerable<string> subredditNames, int limit, int page, string searchTerm = "", string orderBy = "")
        {
            string orderByColumn = "";
            switch (orderBy.ToLower())
            {
                case "username":
                    orderByColumn = "username"; break;
                case "bannedby":
                    orderByColumn = "bannedby"; break;
                case "date":
                    orderByColumn = "bandate"; break;
                case "reason":
                    orderByColumn = "banreason"; break;
                default:
                    orderByColumn = "username"; break;
            }
            string query = $@"
SELECT Count(*)
FROM BotBannedUsers be
INNER JOIN Subreddits sub on sub.SubredditID = be.SubredditID
where sub.SubName in @subredditNames
{(string.IsNullOrWhiteSpace(searchTerm) ? "" : @"
AND (
    be.UserName like '%' + @searchTerm + '%'
    OR be.BannedBy like '%' + @searchTerm + '%'
    OR be.ThingURL like '%' + @searchTerm + '%'
    OR be.AdditionalInfo like '%' + @searchTerm + '%'
    OR sub.SubName like '%' + @searchTerm + '%'
    OR be.BanReason like '%' + @searchTerm + '%'
)
")}

SELECT be.Id, sub.SubName, be.UserName, be.BannedBy, be.BanReason, be.BanDate, be.ThingURL, be.AdditionalInfo
FROM BotBannedUsers be
INNER JOIN Subreddits sub on sub.SubredditID = be.SubredditID
where sub.SubName in @subredditNames
{(string.IsNullOrWhiteSpace(searchTerm) ? "" : @"
AND (
    be.UserName like '%' + @searchTerm + '%'
    OR be.BannedBy like '%' + @searchTerm + '%'
    OR be.ThingURL like '%' + @searchTerm + '%'
    OR be.AdditionalInfo like '%' + @searchTerm + '%'
    OR sub.SubName like '%' + @searchTerm + '%'
    OR be.BanReason like '%' + @searchTerm + '%'
)
")}
ORDER BY {orderByColumn} OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;
";

            using (var multi = await snConn.QueryMultipleAsync(query, new { searchTerm, offset = (limit * (page - 1)), subredditNames, limit }))
            {
                int totalCount = 0;
                totalCount = await multi.ReadFirstAsync<int>();
                var results = await multi.ReadAsync<Models.BannedEntity>();

                return new Models.TableResults<IEnumerable<Models.BannedEntity>>
                {
                    TotalResults = totalCount,
                    CurrentPage = page,
                    ResultsPerPage = limit,
                    DataTable = results
                };
            }
        }

        public async Task<bool> BanChannel(Models.BannedEntity entity, string channelID, string mediaAuthor, Models.VideoProvider vidProvider)
        {
            string query = @"
insert into public.sentinel_blacklist(subreddit_id, media_channel_id, media_author, media_platform_id, blacklist_utc, blacklist_by, media_channel_url)
SELECT id, @channelID, @mediaAuthor, @platformID, @BanDate, @BannedBy, @ChannelURL
from public.subreddit where subreddit_name like @SubName 
";
            try
            {
                await sentinelConn.ExecuteAsync(query, new { channelID, mediaAuthor, platformID = (int)vidProvider, entity.BanDate, entity.BannedBy, entity.ChannelURL, entity.SubName });
            }
            catch(Npgsql.PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    return false;

                throw;
            }
            return true;
        }
    }
}
