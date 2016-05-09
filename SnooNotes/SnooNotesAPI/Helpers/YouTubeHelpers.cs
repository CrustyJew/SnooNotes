using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SnooNotesAPI.Helpers {
    public class YouTubeHelpers {
        public static string ExtractVideoId( string url ) {
            string id = null;
            url = System.Net.WebUtility.UrlDecode( url ); //decode for attribution links
            var lowerUrl = url.ToLower();
            if ( lowerUrl.Contains( "youtube" ) ) {
                //it's a YouTube link
                if ( url.Contains( "v=" ) ) {
                    id = url.Substring( url.IndexOf( "v=", StringComparison.Ordinal ) + 2 ).Split( '&' )[0];
                }
            }
            else if ( lowerUrl.Contains( "youtu.be" ) ) {
                id = url.Substring( url.IndexOf( ".be/", StringComparison.Ordinal ) + 4 ).Split( '?' )[0];
            }
            return id;
        }
    }
}