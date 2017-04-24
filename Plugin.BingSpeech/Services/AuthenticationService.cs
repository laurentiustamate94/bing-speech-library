using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BingSpeech.Abstractions.Interfaces;

namespace Plugin.BingSpeech.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        private string subscriptionKey;
        private string _token;

        private readonly string baseUri;
        private readonly int refreshTokenMinutes;

        public string Token
        {
            get
            {
                return this._token;
            }
        }

        private AuthenticationService()
        {
            this.baseUri = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
            this.refreshTokenMinutes = 9;
        }

        public AuthenticationService(string subscriptionKey)
            : this()
        {
            this.InitializeService(subscriptionKey);
        }

        public void InitializeService(string subscriptionKey)
        {
            this.subscriptionKey = subscriptionKey;
            var interval = TimeSpan.FromMinutes(this.refreshTokenMinutes);

            RenewAccessTokenAsync(OnExpire, interval, CancellationToken.None)
                .ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task RenewAccessTokenAsync(Action onExpire, TimeSpan interval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                onExpire?.Invoke();

                if (interval > TimeSpan.Zero)
                    await Task.Delay(interval, cancellationToken);
            }
        }

        private void OnExpire()
        {
            var task = Task.Run(() => FetchToken(this.baseUri));

            task.Wait();

            this._token = task.Result;
        }

        private async Task<string> FetchToken(string fetchUri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key",
                    this.subscriptionKey);

                var result = await client.PostAsync(fetchUri, null);

                return await result.Content.ReadAsStringAsync();
            }
        }
    }
}
