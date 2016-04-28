using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Runtime.Caching;


namespace SnooNotesAPI.BLL {
    public class DirtbagBLL {
        private static MemoryCache cache = MemoryCache.Default;
        private const string CACHE_PREFIX = "BotSettings:";


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