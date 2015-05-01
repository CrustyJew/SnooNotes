using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SnooNotesAPI.Models
{
    public class BasicNote
    {
        public int NoteID { get; set; }
        public int NoteTypeID { get; set; }
        public string SubName { get; set; }
        public string Submitter { get; set; }
        public string Message { get; set; }
        
    }
}