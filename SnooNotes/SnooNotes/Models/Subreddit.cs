using Newtonsoft.Json;

namespace SnooNotesAPI.Models
{
    public class Subreddit
    {
        public int SubredditID { get; set; }
        public string SubName { get; set; }
        public bool Active { get; set; }
        public DirtbagSettings BotSettings  { get; set; }

        public SubredditSettings Settings { get; set; }

    }
}