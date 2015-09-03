using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace RedditSharp
{
    static class ToolBoxUserNotes
    {
        private const string ToolBoxUserNotesWiki = "/r/{0}/wiki/usernotes";
        public static IEnumerable<Things.tbUserNote> GetUserNotes(Reddit reddit, IWebAgent webAgent, Things.Subreddit sub)
        {
            var request = webAgent.CreateGet(String.Format(ToolBoxUserNotesWiki, sub.Name));
            var reqResponse = webAgent.ExecuteRequest(request);
            var response = JObject.Parse(reqResponse["data"]["content_md"].Value<string>());

            int version = response["ver"].Value<int>();
            string[] mods = response["constants"]["users"].Values<string>().ToArray();

            string[] warnings = response["constants"]["warnings"].Values<string>().ToArray();

            if (version < 6)
            {
                throw new ToolBoxUserNotesException("Unsupported ToolBox version");
            }

            var data = Convert.FromBase64String(response["blob"].Value<string>());

            string uncompressed;
            using (System.IO.MemoryStream compressedStream = new System.IO.MemoryStream(data))
            {
                compressedStream.ReadByte();
                compressedStream.ReadByte(); //skips first to bytes to fix zlib block size
                using (DeflateStream blobStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                {
                    using (var decompressedReader = new System.IO.StreamReader(blobStream))
                    {
                        uncompressed = decompressedReader.ReadToEnd();
                    }

                }
            }

            JObject users = JObject.Parse(uncompressed);

            List<Things.tbUserNote> toReturn = new List<Things.tbUserNote>();
            foreach(KeyValuePair<string, JToken> user in users)
            {
                var x = user.Value;
                foreach(JToken note in x["ns"].Children())
                {
                    //TODO
                }
            }
            throw new NotImplementedException();
        }

    }
}
