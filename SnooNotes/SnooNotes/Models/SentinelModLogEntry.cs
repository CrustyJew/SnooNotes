using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnooNotes.Models
{
    public class SentinelModLogEntry
    {
        public string ThingID { get; set; }
        public string Mod { get; set; }
        public string ModAction { get; set; }
        public string ActionReason { get; set; }
        public string Description { get; set; }
        public string ModActionID { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
