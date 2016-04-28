using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SnooNotesAPI.Controllers
{
    [RoutePrefix("api/Dirtbag")]
    public class DirtbagController : ApiController
    {
        [HttpPost][Route("{subname}/TestConnection")]
        public bool TestConnection(string url, string uname, string pword, string subname ) {
            return true;
        }
    }
}
