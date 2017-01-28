using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SnooNotes.Models {
    public class BannedEntity {
        //copy pasta from Dirtbag source, could shove it in some DLL in the future. Probably should even..
        public int ID { get; set; }
        public string SubName { get; set; }
        public string EntityString { get; set; }
        public EntityType Type { get; set; }
        public string BannedBy { get; set; }
        public string BanReason { get; set; }
        public DateTime? BanDate { get; set; }
        public string ThingID { get; set; }

        [JsonConverter( typeof( StringEnumConverter ) )]
        public enum EntityType {
            Channel = 1, User = 2
        }
    }
}