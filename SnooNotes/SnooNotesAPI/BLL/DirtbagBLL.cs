using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Runtime.Caching;
using System.Net.Http;

namespace SnooNotesAPI.BLL {
    public class DirtbagBLL {
        private static MemoryCache cache = MemoryCache.Default;
        private const string CACHE_PREFIX = "BotSettings:";

        public async Task<bool> SaveSettings(Models.DirtbagSettings settings, string subName ) {
            DAL.SubredditDAL subDAL = new DAL.SubredditDAL();
            await subDAL.UpdateBotSettings( settings, subName );
            cache.Set( CACHE_PREFIX + subName, settings, DateTimeOffset.Now.AddMinutes( 30 ) );
            return true;
        }

        public async Task<bool> TestConnection(Models.DirtbagSettings newSettings, string subName ) {
            Models.DirtbagSettings curSettings = await GetSettings( subName );
            DAL.DirtbagDAL dirtbag = new DAL.DirtbagDAL();
            
            if( curSettings == null || curSettings.DirtbagUrl != newSettings.DirtbagUrl ) {
                //no current settings OR url changed, don't allow using saved password.
                return await dirtbag.TestConnection( newSettings, subName );
            }
            else if(string.IsNullOrWhiteSpace( newSettings.DirtbagPassword)) {
                //url did NOT change, and password wasn't updated
                return await dirtbag.TestConnection( curSettings.DirtbagUrl, newSettings.DirtbagUsername, curSettings.DirtbagPassword, subName );
            }
            else {
                return await dirtbag.TestConnection( newSettings, subName );
            }
        }

        public async Task<bool> TestConnection(string subName ) {
            DAL.DirtbagDAL dirtbag = new DAL.DirtbagDAL();
            var curSettings = await GetSettings( subName );
            return await dirtbag.TestConnection( curSettings, subName );
        }

        public async Task<IEnumerable<Models.BannedEntity>> GetBanList(string subName ) {
            var curSettings = await GetSettings( subName );
            if ( curSettings == null || string.IsNullOrWhiteSpace( curSettings.DirtbagUrl ) )
                throw new HttpRequestException( $"No valid settings for {subName} could be found!" );

            DAL.DirtbagDAL dirtbag = new DAL.DirtbagDAL();
            return await dirtbag.GetBanList( curSettings, subName );
        }

        public async Task<bool> RemoveBan(int id, string modName, string subName ) {
            var curSettings = await GetSettings( subName );
            if ( curSettings == null || string.IsNullOrWhiteSpace( curSettings.DirtbagUrl ) )
                throw new HttpRequestException( $"No valid settings for {subName} could be found!" );

            DAL.DirtbagDAL dirtbag = new DAL.DirtbagDAL();
            return await dirtbag.RemoveFromBanList( curSettings, id, modName, subName );
        }

        private async Task<Models.DirtbagSettings> GetSettings(string subName ) {
            DAL.SubredditDAL subDAL = new DAL.SubredditDAL();
            var cacheVal = cache[CACHE_PREFIX + subName];
            if(cacheVal == null ) {
                var botSets = await subDAL.GetBotSettings( subName );
                cache.Set( CACHE_PREFIX + subName, botSets, DateTimeOffset.Now.AddMinutes( 30 ) );
                return botSets;
            }
            return (Models.DirtbagSettings) cacheVal;
        }

        public async Task UpdateBanReason( string subName, int id, string reason, string modname ) {
            var conn = await GetSettings( subName );
            DAL.DirtbagDAL dirtbag = new DAL.DirtbagDAL();
            await dirtbag.UpdateBanReason(conn, subName, id, reason, modname );
        }

        public async Task BanChannel( string subName, string url, string reason, string thingID, string bannedBy ) {
            DAL.YouTubeDAL ytDAL = new DAL.YouTubeDAL();
            string ytVidID = Helpers.YouTubeHelpers.ExtractVideoId( url );
            if ( string.IsNullOrWhiteSpace( ytVidID ) )
                throw new ArgumentException( $"Couldn't extract YouTube video ID from url: {url}" );
            string channelID = await ytDAL.GetChannelID( ytVidID );
            Models.BannedEntity toBan = new Models.BannedEntity() {
                BanDate = DateTime.UtcNow,
                BannedBy = bannedBy,
                BanReason = reason,
                EntityString = channelID,
                SubName = subName,
                ThingID = thingID,
                Type = Models.BannedEntity.EntityType.Channel
            };
            var conn = await GetSettings( subName );
            DAL.DirtbagDAL dirtbag = new DAL.DirtbagDAL();
            await dirtbag.AddToBanList(conn, new List<Models.BannedEntity>() { toBan } );
        }

        public async Task BanUser( string subName, string username, string reason, string thingID, string bannedBy ) {
            Models.BannedEntity toBan = new Models.BannedEntity() {
                BanDate = DateTime.UtcNow,
                BannedBy = bannedBy,
                BanReason = reason,
                EntityString = username,
                SubName = subName,
                ThingID = thingID,
                Type = Models.BannedEntity.EntityType.User
            };
            var conn = await GetSettings( subName );
            DAL.DirtbagDAL dirtbag = new DAL.DirtbagDAL();
            await dirtbag.AddToBanList( conn, new List<Models.BannedEntity>() { toBan } );
        }
    }
}