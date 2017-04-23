using Newtonsoft.Json;

namespace Plugin.BingSpeech.Abstractions.Models
{
    public class BingSpeechData
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("scenario")]
        public string Scenario { get; set; }

        [JsonProperty("name")]
        public string NormalForm { get; set; }

        [JsonProperty("lexical")]
        public string LexicalForm { get; set; }
    }
}
