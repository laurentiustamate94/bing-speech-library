using System.Threading.Tasks;

namespace Plugin.BingSpeech.Abstractions.Interfaces
{
    public interface IBingSpeechService
    {
        Task<string> GetTextResult(string filename);

        void InitializeService(IAuthenticationService authenticationService);
    }
}
