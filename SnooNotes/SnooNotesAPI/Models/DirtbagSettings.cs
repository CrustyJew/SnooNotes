using Newtonsoft.Json;
namespace SnooNotesAPI.Models {
    public class DirtbagSettings {
        public string DirtbagUrl { get; set; }
        public string DirtbagUsername { get; set; }
        [JsonIgnore]
        public string DirtbagPassword { get; set; }
    }
}