using System;
using System.Collections.Generic;
using System.Text;

namespace PermissionMonitor
{
    class SentinelSettings
    {
        public int ID { get; set; }
        public string SubredditName { get; set; }
        public bool SentinelEnabled { get; set; }
        public bool DirtbagEnabled { get; set; }

    }
}
