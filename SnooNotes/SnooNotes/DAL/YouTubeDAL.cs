using System;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;

namespace SnooNotes.DAL {
    public class YouTubeDAL : IYouTubeDAL {
        public string YouTubeAPIKey { get; set; }

        public YouTubeDAL(IConfigurationRoot config ) {
            var key = config["YouTubeAPIKey"];
            if ( string.IsNullOrEmpty( key ) ) throw new Exception( "Provide setting 'YouTubeAPIKey' in AppConfig" );
            YouTubeAPIKey = key;
        }

        public async Task<string> GetChannelID(string vidID ) {
            var yt = new YouTubeService( new BaseClientService.Initializer { ApiKey = YouTubeAPIKey } );
            var req = yt.Videos.List( "snippet" );
            req.Id = vidID;
            var response = await req.ExecuteAsync();
            if ( response.Items.Count == 0 ) throw new Exception( $"Can't find video with id {vidID}" );

            return response.Items[0].Snippet.ChannelId;
        }
    }
}