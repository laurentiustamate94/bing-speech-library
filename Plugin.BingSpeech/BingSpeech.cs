using System;
using Plugin.BingSpeech.Abstractions.Interfaces;
using Plugin.BingSpeech.Services;

namespace Plugin.BingSpeech
{
    public sealed class BingSpeech
    {
        private static Lazy<BingSpeech> bingSpeech 
            = new Lazy<BingSpeech>(() => new BingSpeech(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

        public readonly IBingSpeechService BingSpeechService = null;
        public readonly IMicrophoneService MicrophoneService = null;

        private BingSpeech()
        {
#if !PORTABLE
            this.MicrophoneService = new MicrophoneService();
#endif
            this.BingSpeechService = new BingSpeechService();
        }

        public static BingSpeech Current
        {
            get
            {
                var service = bingSpeech.Value;

                if (service == null)
                {
                    throw new NotImplementedException();
                }

                return service;
            }
        }

        public void InitializePlugin(string subscriptionKey)
        {
            var authenticationService = new AuthenticationService(subscriptionKey);

            this.BingSpeechService.InitializeService(authenticationService);
        }
    }
}
