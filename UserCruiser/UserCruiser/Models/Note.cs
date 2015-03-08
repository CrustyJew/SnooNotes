using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserCruiser.Models {
	public class Note {
		public string Submitter { get; set; }
		public string Subreddit { get; set; }
		public NoteType Type { get; set; }
		public string Message { get; set; }
		public string Link { get; set; }
	}

	class NoteType {
		public string DisplayName { get; set; }
		public string ColorCode { get; set; }
		public bool Bold { get; set; }
		public int Order { get; set; }
	}
}