using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SnooNotesAPI.Utilities
{
    public static class TBNoteUtils
    {
        public static List<Models.Note> ConvertTBNotesToSnooNotes(string subName, Dictionary<string,int> tbToSNMapping, List<RedditSharp.Things.tbUserNote> tbNotes)
        {
            var toReturn = tbNotes.Select(tb => new Models.Note { AppliesToUsername = tb.AppliesToUsername.ToLower(), Message = tb.Message, NoteTypeID = tbToSNMapping[tb.NoteType], Submitter = tb.Submitter, SubName = subName, Timestamp = tb.Timestamp, Url=tb.Url });
            return toReturn.ToList();
        }
    }
}