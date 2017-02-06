using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace SnooNotes.BLL {
    public class DirtbagBLL : IDirtbagBLL {
        private IMemoryCache cache;
        private const string CACHE_PREFIX = "BotSettings:";
        private DAL.IDirtbagDAL dirtbag;
        private DAL.ISubredditDAL subDAL;
        private DAL.IYouTubeDAL ytDAL;
        public DirtbagBLL(IMemoryCache memoryCache, DAL.IDirtbagDAL dirtbagDAL, DAL.ISubredditDAL subredditDAL, DAL.IYouTubeDAL youTubeDAL) {
            cache = memoryCache;
            dirtbag = dirtbagDAL;
            subDAL = subredditDAL;
            ytDAL = youTubeDAL;
        }

        public async Task<bool> SaveSettings(Models.DirtbagSettings settings, string subName ) {
            await subDAL.UpdateBotSettings( settings, subName );
            cache.Set( CACHE_PREFIX + subName, settings, DateTimeOffset.Now.AddMinutes( 30 ) );
            return true;
        }

        public async Task<bool> TestConnection(Models.DirtbagSettings newSettings, string subName ) {
            Models.DirtbagSettings curSettings = await GetSettings( subName );
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
            var curSettings = await GetSettings( subName );
            return await dirtbag.TestConnection( curSettings, subName );
        }

        public async Task<IEnumerable<Models.BannedEntity>> GetBanList(string subName ) {
            var curSettings = await GetSettings( subName );
            if ( curSettings == null || string.IsNullOrWhiteSpace( curSettings.DirtbagUrl ) )
                throw new HttpRequestException( $"No valid settings for {subName} could be found!" );
            
            return await dirtbag.GetBanList( curSettings, subName );
        }

        public async Task<bool> RemoveBan(int id, string modName, string subName ) {
            var curSettings = await GetSettings( subName );
            if ( curSettings == null || string.IsNullOrWhiteSpace( curSettings.DirtbagUrl ) )
                throw new HttpRequestException( $"No valid settings for {subName} could be found!" );
            
            return await dirtbag.RemoveFromBanList( curSettings, id, modName, subName );
        }

        private async Task<Models.DirtbagSettings> GetSettings(string subName ) {
            object cacheVal;
             
            if( !cache.TryGetValue( CACHE_PREFIX + subName, out cacheVal ) ) {
                var botSets = await subDAL.GetBotSettings( subName );
                cache.Set( CACHE_PREFIX + subName, botSets, DateTimeOffset.Now.AddMinutes( 30 ) );
                return botSets;
            }
            return (Models.DirtbagSettings) cacheVal;
        }

        public async Task UpdateBanReason( string subName, int id, string reason, string modname ) {
            var conn = await GetSettings( subName );
            await dirtbag.UpdateBanReason(conn, subName, id, reason, modname );
        }

        public async Task BanChannel( string subName, string url, string reason, string thingID, string bannedBy ) {
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
            await dirtbag.AddToBanList( conn, new List<Models.BannedEntity>() { toBan } );
        }
    }
}