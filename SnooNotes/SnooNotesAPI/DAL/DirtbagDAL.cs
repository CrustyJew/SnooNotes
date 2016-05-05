using System;
using SnooNotesAPI.Models;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SnooNotesAPI.DAL {
    public class DirtbagDAL {
        private const string TEST_CONNECTION_ENDPOINT = "api/Info/TestConnection?subreddit={0}";
        private const string BAN_LIST_ENDPOINT = "api/AutomodBanned?subname={0}"; //TODO: dear god why can't I name shit consistently
        private const string REMOVE_BAN_ENDPOINT = "api/AutomodBanned?subname={0}&id={1}&modname={2}";
        public Task<bool> TestConnection( DirtbagSettings botSettings, string subreddit ) {
            return TestConnection( botSettings.DirtbagUrl, botSettings.DirtbagUsername, botSettings.DirtbagPassword, subreddit );
        }

        public async Task<bool> TestConnection( string dirtbagUrl, string dirtbagUsername, string dirtbagPassword, string subreddit ) {
            using ( var client = new HttpClient() ) {
                string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes( $"{dirtbagUsername}:{dirtbagPassword}" ));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue( "Basic", auth );
               
                    var response = await client.GetAsync( string.Format( dirtbagUrl + TEST_CONNECTION_ENDPOINT, subreddit ) );
                if ( response.IsSuccessStatusCode ) return true;
                else {
                    throw new HttpRequestException( response.StatusCode +": " + await response.Content.ReadAsStringAsync() );
                }
                
            }
        }

        public async Task<IEnumerable<BannedEntity>> GetBanList(DirtbagSettings conn, string subreddit ) {
            using (var client = new HttpClient() ) {
                string auth = Convert.ToBase64String( Encoding.ASCII.GetBytes( $"{conn.DirtbagUsername}:{conn.DirtbagPassword}" ) );
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue( "Basic", auth );

                var response = await client.GetAsync( string.Format( conn.DirtbagUrl + BAN_LIST_ENDPOINT, subreddit ) );

                if(!response.IsSuccessStatusCode)
                    throw new HttpRequestException( response.StatusCode + ": " + await response.Content.ReadAsStringAsync() );

                string responseString = await response.Content.ReadAsStringAsync();

                IEnumerable<BannedEntity> toReturn = JsonConvert.DeserializeObject<IEnumerable<BannedEntity>>( responseString );
                return toReturn;
            }
        }

        public async Task<bool> RemoveFromBanList(DirtbagSettings conn, int id, string modName, string subreddit ) {
            using ( var client = new HttpClient() ) {
                string auth = Convert.ToBase64String( Encoding.ASCII.GetBytes( $"{conn.DirtbagUsername}:{conn.DirtbagPassword}" ) );
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue( "Basic", auth );

                var response = await client.DeleteAsync( string.Format( conn.DirtbagUrl + REMOVE_BAN_ENDPOINT, subreddit, id, modName ) );

                if ( response.IsSuccessStatusCode )
                    return true;

                return false;
            }
        }

    }
}