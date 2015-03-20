using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserCruiser.Models {
	public class Subreddit {
		public string Name { get; set; }
		public string AppID { get; set; }
		public string PrivateKey { get; set; }
		public List<User> Users { get; set; }
		public List<NoteType> NoteTypes { get; set; }
	}

	class User {
		public string UserName { get; set; }
	}
}