using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.BingSpeech.Abstractions.Interfaces;
using Plugin.BingSpeech.Abstractions.Models;
using Plugin.NetStandardStorage;

namespace Plugin.BingSpeech.Services
{
    internal class BingSpeechService : IBingSpeechService
    {
        private readonly string _requestUri = null;
        private IAuthenticationService _authenticationService = null;

        public BingSpeechService()
        {
            this._requestUri = string.Format("{0}?{1}&{2}&{3}&{4}&{5}&{6}&{7}&{8}",
                "https://speech.platform.bing.com/recognize",
                "scenarios=smd",
                "appid=D4D52672-91D7-4C74-8AD8-42B1D98141A5",
                "locale=en-US",
                "device.os=multiple",
                "version=3.0",
                "format=json",
                "instanceid=565D69FF-E928-4B7E-87DA-9A750B96D9E3",
                "requestid=" + Guid.NewGuid().ToString());
        }

        public void InitializeService(string subscriptionKey)
        {
            this._authenticationService = new AuthenticationService(subscriptionKey);
        }

        public async Task<string> GetTextResult(string recordedFilename)
        {
            if(this._authenticationService == null)
            {
                throw new NullReferenceException("You must initialise the plugin first !");
            }

            var file = CrossStorage.FileSystem.LocalStorage.GetFile(recordedFilename);

            using (var fileStream = file.Open(FileAccess.Read))
            {
                var response = await SendRequestAsync(fileStream);
                var speechResults = JsonConvert.DeserializeObject<BingSpeechResult>(response);

                return speechResults.Data.NormalForm;
            }
        }

        private async Task<string> SendRequestAsync(Stream fileStream)
        {
            using (var client = new HttpClient())
            {
                var content = new StreamContent(fileStream);

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", this._authenticationService.Token);

                var response = await client.PostAsync(this._requestUri, content);

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
