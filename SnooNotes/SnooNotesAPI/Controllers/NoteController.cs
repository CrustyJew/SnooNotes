using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SnooNotesAPI.Controllers
{
    public class NoteController : ApiController
    {
        Models.NoteMain nm = new Models.NoteMain();
        // GET: api/Note
        public IEnumerable<Models.Note> Get()
        {
            

        }

        // GET: api/Note/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Note
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Note/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Note/5
        public void Delete(int id)
        {
        }
    }
}
