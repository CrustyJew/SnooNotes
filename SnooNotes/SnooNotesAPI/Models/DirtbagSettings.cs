using Newtonsoft.Json;
namespace SnooNotesAPI.Models {
    public class DirtbagSettings {
        public string DirtbagUrl { get; set; }
        public string DirtbagUsername { get; set; }

        [JsonIgnore]
        private string _password;
        [JsonProperty( "DirtbagPassword" )]
        public string SerializePassword
        {
            set { _password = value; }
        }
        [JsonIgnore]
        public string DirtbagPassword
        {
            get { return _password; }
            set { _password = value; }
        }
    }
}