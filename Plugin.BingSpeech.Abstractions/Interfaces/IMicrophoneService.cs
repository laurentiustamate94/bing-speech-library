namespace Plugin.BingSpeech.Abstractions.Interfaces
{
    public interface IMicrophoneService
    {
        void StartRecording(string recordingFilename);

        void StopRecording();

        void ContinuousDictation();

        void RemoveRecording();
    }
}
