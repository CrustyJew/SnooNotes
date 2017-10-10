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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="subredditNames"></param>
        /// <param name="limit">limit result rows per page</param>
        /// <param name="page">page of results to return</param>
        /// <param name="searchTerm">serach term to use if any</param>
        /// <param name="orderBy">"username","bannedby","date","reason", defaults to "username"</param>
        /// <param name="ascending">true = sort ascending; false = sort descending</param>
        /// <returns></returns>
        public async Task<Models.TableResults<Models.BannedEntity>> SearchBannedUsers(IEnumerable<string> subredditNames, int limit, int page, string searchTerm, string orderBy, bool ascending)
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
                case "subreddit":
                    orderByColumn = "subname"; break;
                default:
                    orderByColumn = "bandate"; break;
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
ORDER BY {orderByColumn} {(ascending ? "asc" : "desc")} OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;
";

            using (var multi = await snConn.QueryMultipleAsync(query, new { searchTerm, offset = (limit * (page - 1)), subredditNames, limit }))
            {
                int totalCount = 0;
                totalCount = await multi.ReadFirstAsync<int>();
                var results = await multi.ReadAsync<Models.BannedEntity>();

                return new Models.TableResults<Models.BannedEntity>
                {
                    TotalResults = totalCount,
                    CurrentPage = page,
                    ResultsPerPage = limit,
                    DataTable = results
                };
            }
        }

        public async Task<bool> DeleteUserBan(string sub, int id, string user) {
            string query = @"
DELETE be
OUTPUT DELETED.SubredditID, DELETED.UserName, DELETED.BannedBy, DELETED.BanDate, DELETED.BanReason, DELETED.ThingUrl, DELETED.AdditionalInfo, null, GETUTCDATE(), @user, 'D' into BotBannedUsers_Audit (SubredditID, UserName, BannedBy, BanDate, BanReason, ThingUrl, AdditionalInfoOld, AdditionalInfoNew, ModifiedDate, ModifiedBy, AuditAction)
FROM BotBannedUsers be
INNER JOIN Subreddits sub on sub.SubredditID = be.SubredditID
where be.ID = @id
AND sub.SubName = @sub
";
            return await snConn.ExecuteAsync(query, new { id, sub, user }) > 0;
        }

        public async Task<bool> UpdateAdditionalInfo(string sub, int id, string additionalInfo, string user)
        {
            string query = @"
UPDATE be
SET be.AdditionalInfo = @additionalInfo, modifiedBy = @user, modifiedDate = GETUTCDATE()
FROM BotBannedUsers be
INNER JOIN Subreddits sub on sub.SubredditID = be.SubredditID
where be.ID = @id
AND sub.SubName = @sub
";
            return await snConn.ExecuteAsync(query, new { sub, id, additionalInfo, user}) > 0;
        }

        public Task<Models.BannedEntity> GetBanByID(int id)
        {
            string query = @"
SELECT be.Id, sub.SubName, be.UserName, be.BannedBy, be.BanReason, be.BanDate, be.ThingURL, be.AdditionalInfo
FROM BotBannedUsers be
INNER JOIN Subreddits sub on sub.SubredditID = be.SubredditID
WHERE be.ID = @id
";
            return snConn.QuerySingleOrDefaultAsync<Models.BannedEntity>(query, new { id });
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
