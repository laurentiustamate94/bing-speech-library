using System;

namespace Plugin.BingSpeech.Exceptions
{
    public sealed class MicrophoneServiceException : Exception
    {
        public MicrophoneServiceException()
        {
        }

        public MicrophoneServiceException(string message)
            : base(message)
        {
        }

        public MicrophoneServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
