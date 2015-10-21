using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SnooNotesAPI.Models
{
    public class UserIdentity
    {
        public string UserName { get; set; }
        public bool HasWikiRead { get; set; }
        public bool HasRead { get; set; }
    }
}