using Newtonsoft.Json;

namespace Plugin.BingSpeech.Abstractions.Models
{
    public class BingSpeechResult
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("header")]
        public BingSpeechData Data { get; set; }
    }
}
