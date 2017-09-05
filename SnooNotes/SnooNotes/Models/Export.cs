using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnooNotes.Models
{
    public class Export
    {
        public IEnumerable<NoteType> NoteTypes { get; set; }
        public IEnumerable<Note> Notes { get; set; }
    }
}
