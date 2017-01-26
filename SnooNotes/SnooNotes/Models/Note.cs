using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace SnooNotes.Models
{
    public class Note
    {
        public int NoteID { get; set; }
        public int NoteTypeID { get; set; }
        public string SubName { get; set; }
        public string Submitter { get; set; }
        public string Message { get; set; }
        public string AppliesToUsername { get; set; }
        public string Url { get; set; }
        private DateTime _timestamp;
        public DateTime Timestamp
        {
            get { return _timestamp; }
            set
            {
                _timestamp = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }
        public string ParentSubreddit { get; set; }

    }
}