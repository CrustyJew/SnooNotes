using System;
using SnooNotesAPI.Models;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace SnooNotesAPI.DAL {
    public class DirtbagDAL {
        private const string TEST_CONNECTION_ENDPOINT = "api/Info/TestConnection?subreddit={0}";
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

    }
}