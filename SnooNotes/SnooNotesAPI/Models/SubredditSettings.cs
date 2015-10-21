using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SnooNotesAPI.Models
{
    public class SubredditSettings
    {
        public int AccessMask { get; set; }
        public List<NoteType> NoteTypes { get; set; }

        public SubredditSettings()
        {
            AccessMask = 64;
            NoteTypes = new List<NoteType>();
        }

        public static List<NoteType> DefaultNoteTypes(string subName)
        {
            var ret = new List<NoteType>()
            {
                new NoteType() { DisplayName="None", ColorCode="369", DisplayOrder=0, Bold = false, Italic=false,SubName=subName  },
                new NoteType() { DisplayName="Good Contributor", ColorCode="008000", DisplayOrder=1, Bold = false, Italic=false,SubName=subName  },
                new NoteType() { DisplayName="Spam Watch", ColorCode="FF00FF", DisplayOrder=2, Bold = false, Italic=false,SubName=subName  },
                new NoteType() { DisplayName="Spam Warning", ColorCode="800080", DisplayOrder=3, Bold = false, Italic=false,SubName=subName  },
                new NoteType() { DisplayName="Abuse Warning", ColorCode="FFA500", DisplayOrder=4, Bold = false, Italic=false,SubName=subName  },
                new NoteType() { DisplayName="Ban", ColorCode="FF0000", DisplayOrder=5, Bold = false, Italic=false,SubName=subName  },
                new NoteType() { DisplayName="Perma Ban", ColorCode="8B0000", DisplayOrder=6, Bold = false, Italic=false,SubName=subName  },
                new NoteType() { DisplayName="Bot Ban", ColorCode="000000", DisplayOrder=7, Bold = false, Italic=false,SubName=subName  },
            };
            return ret;
        }
    }
}