using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SnooNotesAPI.Models
{
    public class Subreddit
    {
        public int SubredditID { get; set; }
        public string SubName { get; set; }
        public string AppID { get; set; }
        public string PrivateKey { get; set; }

        public bool Active { get; set; }
    }
}