using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SnooNotes.Utilities
{
    public static class TBNoteUtils
    {
        public static List<Models.Note> ConvertTBNotesToSnooNotes(string subName, Dictionary<string,int> tbToSNMapping, List<RedditSharp.TBUserNote> tbNotes)
        {
            var mappedTypes = tbToSNMapping.Where(d => d.Value != -1).ToList();
            var toReturn = tbNotes.Where(tb=>mappedTypes.Exists(mt=>mt.Key == (tb.NoteType ?? "null") )).Select(tb => new Models.Note { AppliesToUsername = tb.AppliesToUsername.ToLower(), Message = tb.Message, NoteTypeID = tbToSNMapping[(tb.NoteType ?? "null")], Submitter = tb.Submitter, SubName = subName, Timestamp = tb.Timestamp, Url=tb.Url });
            return toReturn.ToList();
        }
    }
}