using System;
using System.Text.RegularExpressions;

namespace SnooNotes.Helpers {
    public class YouTubeHelpers {
        public static string ExtractVideoId( string url ) {
            string id = null;
            url = System.Net.WebUtility.UrlDecode( url ); //decode for attribution links
            string ddecode = System.Net.WebUtility.UrlDecode( url );
            while ( url != ddecode ) {
                url = ddecode;
                ddecode = System.Net.WebUtility.UrlDecode( url );
            }
            var lowerUrl = url.ToLower();
            if ( lowerUrl.Contains( "youtube" ) ) {
                //it's a YouTube link
                if ( url.Contains( "v=" ) ) {
                    id = url.Substring( url.IndexOf( "v=", StringComparison.Ordinal ) + 2 ).Split( '&' )[0];
                    id = Regex.Match( id, "[a-zA-Z0-9_-]{11}" ).Value;
                }
            }
            else if ( lowerUrl.Contains( "youtu.be" ) ) {
                id = url.Substring( url.IndexOf( ".be/", StringComparison.Ordinal ) + 4 ).Split( '?' )[0];
                id = Regex.Match( id, "[a-zA-Z0-9_-]{11}" ).Value;
            }
            return id;
        }
    }
}