namespace Plugin.BingSpeech.Abstractions.Interfaces
{
    public interface IMicrophoneService
    {
        void StartRecording();

        void StopRecording();

        void ContinuousDictation();

        void RemoveRecording();
    }
}
