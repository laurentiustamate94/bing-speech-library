using System.Threading.Tasks;

namespace Plugin.BingSpeech.Abstractions.Interfaces
{
    public interface IBingSpeechService
    {
        Task<string> GetTextResult(string recordedFilename);

        void InitializeService(string subscriptionKey);
    }
}
