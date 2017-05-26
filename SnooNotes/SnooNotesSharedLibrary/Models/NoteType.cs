using System;
using System.Collections.Generic;
using System.Linq;


namespace SnooNotes.Models
{
    public class NoteType
    {
        public int NoteTypeID { get; set; }
        public string SubName { get; set; }
        public string DisplayName { get; set; }
        public string ColorCode { get; set; }
        public int DisplayOrder { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public string IconString { get; set; }
		public bool Disabled { get; set; }
        public NoteType()
        {
            NoteTypeID = -1;
        }
        

    }
}