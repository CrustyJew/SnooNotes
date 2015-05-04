using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SnooNotesAPI.Controllers
{
    public class SubredditController : ApiController
    {
        // GET: api/Subreddit
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Subreddit/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Subreddit
        public void Post([FromBody]Models.Subreddit newSub)
        {

            Models.Subreddit.AddSubreddit(newSub);
        }

        // PUT: api/Subreddit/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Subreddit/5
        public void Delete(int id)
        {
        }
    }
}
