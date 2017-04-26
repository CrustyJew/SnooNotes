using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

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

        public async Task BanUser(IEnumerable<Models.BannedEntity> entities)
        {
            string query = @"
insert into BotBannedUsers (SubredditID, UserName, BannedBy, BanReason, BanDate, ThingUrl)
select sub.SubredditID,@UserName,@BannedBy,@BanReason,@BanDate,@ThingURL from Subreddits sub where sub.SubName like @SubName
;";
           
                await snConn.ExecuteAsync(query, entities);
                return;
            
        }

        public async Task<IEnumerable<Models.BannedEntity>> GetBannedUsers(string subredditName)
        {
            string query = @"
select be.Id, sub.SubName, be.UserName, be.BannedBy, be.BanReason, be.BanDate, be.ThingURL
from BotBannedUsers be
inner join Subreddits sub on sub.SubredditID = be.SubredditID
where sub.SubName like @subredditName
;";
            return await snConn.QueryAsync<Models.BannedEntity>(query, new { subredditName });
            
        }

        public async Task BanChannel(Models.BannedEntity entity, string channelID, string mediaAuthor, Models.VideoProvider vidProvider)
        {
            string query = @"
insert into public.sentinel_blacklist(subreddit_id, media_channel_id, media_author, media_playform_id, blacklist_utc, blacklist_by, media_channel_url)
SELECT subreddit_id, @channelID, @mediaAuthor, @platformID, @BanDate, @BannedBy, @ChannelURL
from public.subreddit where subreddit_name like @SubName 
";
            await sentinelConn.ExecuteAsync(query, new { channelID, mediaAuthor, platformID = (int)vidProvider, entity.BanDate, entity.BannedBy, entity.ChannelURL, entity.SubName });
            return;
        }
    }
}
