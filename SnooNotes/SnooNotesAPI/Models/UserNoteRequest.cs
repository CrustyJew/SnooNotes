using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SnooNotesAPI.Models
{
    public class UserNoteRequest
    {
        public string SubName { get; set; }
        public IEnumerable<string> Users { get; set; }

    }
}