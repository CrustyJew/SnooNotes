using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SnooNotes.Controllers {
    [Authorize]
	public class ValuesController : Controller {
		// GET api/values
		public IEnumerable<string> Get() {
            ClaimsIdentity id = (ClaimsIdentity)User.Identity;
            return id.Claims.Select(x => x.Type + ":" + x.Value).ToArray();
			//return new string[] { "value1", "value2" };
		}

		// GET api/values/5
		public string Get( int id ) {
			return "value";
		}

		// POST api/values
		public void Post( [FromBody]string value ) {
		}

		// PUT api/values/5
		public void Put( int id, [FromBody]string value ) {
		}

		// DELETE api/values/5
		public void Delete( int id ) {
		}
	}
}
