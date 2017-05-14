using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SnooNotes.Models
{
    public class BasicNoteType
    {
        public int NoteTypeID { get; set; }
        public string DisplayName { get; set; }
        public string ColorCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public string IconString { get; set; }
    }
}