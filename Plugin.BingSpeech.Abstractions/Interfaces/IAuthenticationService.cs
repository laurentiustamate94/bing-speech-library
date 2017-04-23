namespace Plugin.BingSpeech.Abstractions.Interfaces
{
    public interface IAuthenticationService
    {
        string Token { get; }

        void InitializeService(string subscriptionKey);
    }
}
