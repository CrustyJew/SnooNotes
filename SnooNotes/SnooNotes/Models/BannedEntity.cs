using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SnooNotes.Models {
    public class BannedEntity {
        public int ID { get; set; }
        public string SubName { get; set; }
        public string UserName { get; set; }
        public string ChannelURL { get; set; }
        public string BannedBy { get; set; }
        public string BanReason { get; set; }
        public DateTime? BanDate { get; set; }
        public string ThingURL { get; set; }
        public string AdditionalInfo { get; set; }

    }
}