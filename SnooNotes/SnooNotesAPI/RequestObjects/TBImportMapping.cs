using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

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

        [Required]
        public string subName { get; set; }

        [Required]
        [DataAnnotationsExtensions.Min(0)]
        public int snNone { get; set; }
        [Required]
        [DataAnnotationsExtensions.Min(0)]
        public int snGoodUser { get; set; }
        [Required]
        [DataAnnotationsExtensions.Min(0)]
        public int snSpamWatch { get; set; }
        [Required]
        [DataAnnotationsExtensions.Min(0)]
        public int snSpamWarn { get; set; }
        [Required]
        [DataAnnotationsExtensions.Min(0)]
        public int snAbuseWarn { get; set; }
        [Required]
        [DataAnnotationsExtensions.Min(0)]
        public int snBan { get; set; }
        [Required]
        [DataAnnotationsExtensions.Min(0)]
        public int snPermBan { get; set; }
        [Required]
        [DataAnnotationsExtensions.Min(0)]
        public int snBotBan { get; set; }

        public TBImportMapping()
        {
            snAbuseWarn = snBan = snBotBan = snGoodUser= snNone = snPermBan = snSpamWarn = snSpamWatch = -1;
        }

        public Dictionary<string, int> GetNoteTypeMapping()
        {
            Dictionary<string, int> mapping = new Dictionary<string, int>();

            if(snAbuseWarn < 0 || snBan < 0 || snBotBan < 0 || snGoodUser < 0 || snNone < 0 || snPermBan < 0 || snSpamWarn < 0 || snSpamWatch < 0 )
            {
                throw new Exception("Invalid mapping for one or more note types!"); //TODO better exception or get rid of it because of annotations
            }

            mapping.Add(tbNTAbuseWarn, snAbuseWarn);
            mapping.Add(tbNTBan, snBan);
            mapping.Add(tbNTBotBan, snBotBan);
            mapping.Add(tbNTGoodUser, snGoodUser);
            mapping.Add(tbNTNone, snNone);
            mapping.Add(tbNTPermBan, snPermBan);
            mapping.Add(tbNTSpamWarn, snSpamWarn);
            mapping.Add(tbNTSpamWatch, snSpamWatch);

            return mapping;
        }
    }
}