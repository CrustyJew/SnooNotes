using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SnooNotesAPI.RequestObjects
{
    public class TBImportMapping
    {
        private const string tbNTNone = "none";
        private const string tbNTGoodUser = "gooduser";
        private const string tbNTAbuseWarn = "abusewarn";
        private const string tbNTSpamWatch = "spamwatch";
        private const string tbNTSpamWarn = "spamwarn";
        private const string tbNTBan = "ban";
        private const string tbNTPermBan = "permban";
        private const string tbNTBotBan = "botban";

        public int snNone { get; set; }
    }
}