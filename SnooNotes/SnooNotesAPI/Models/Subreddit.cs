using Newtonsoft.Json;

namespace SnooNotesAPI.Models
{
    public class Subreddit
    {
        public int SubredditID { get; set; }
        public string SubName { get; set; }
        public bool Active { get; set; }
        public string DirtbagUrl { get; set; }
        public string DirtbagUsername { get; set; }
        [JsonIgnore]
        public string DirtbagPassword { get; set; }
        
        public SubredditSettings Settings { get; set; }

    }
}