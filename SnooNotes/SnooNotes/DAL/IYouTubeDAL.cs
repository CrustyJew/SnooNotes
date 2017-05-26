using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnooNotes.DAL {
    public interface IYouTubeDAL {
        string YouTubeAPIKey { get; set; }

        Task<string> GetChannelID( string vidID );

        Task<KeyValuePair<string, string>> GetChannelIDAndName(string vidID);
    }
}