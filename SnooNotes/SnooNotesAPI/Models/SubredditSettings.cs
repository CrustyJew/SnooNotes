using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SnooNotesAPI.Models
{
    public class SubredditSettings
    {
        public int AccessMask { get; set; }

        public SubredditSettings()
        {
            AccessMask = 64;
        }
    }
}