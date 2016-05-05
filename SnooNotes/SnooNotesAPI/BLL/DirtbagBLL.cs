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
                cache.AddOrGetExisting( CACHE_PREFIX + subName, botSets, DateTimeOffset.Now.AddMinutes( 30 ) );
                cacheVal = cache[CACHE_PREFIX + subName]; //This may be redundant. I don't remember how object references interact with arrays like this but I'm too lazy to find out at the moment;
            }
            return (Models.DirtbagSettings) cacheVal;
        }
    }
}