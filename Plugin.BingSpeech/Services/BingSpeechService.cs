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
        private readonly string requestUri;
        private IAuthenticationService authenticationService;

        public BingSpeechService()
        {
            this.requestUri = string.Format("{0}?{1}&{2}&{3}&{4}&{5}&{6}&{7}&{8}",
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

        public void InitializeService(IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
        }

        public async Task<string> GetTextResult(string filename)
        {
            var file = CrossStorage.FileSystem.LocalStorage.GetFile(filename);

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
                    new AuthenticationHeaderValue("Bearer", authenticationService.Token);

                var response = await client.PostAsync(requestUri, content);

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
