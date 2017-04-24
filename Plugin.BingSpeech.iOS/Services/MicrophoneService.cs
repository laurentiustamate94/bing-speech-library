using System;
using AVFoundation;
using Foundation;
using Plugin.BingSpeech.Abstractions.Interfaces;
using Plugin.BingSpeech.Exceptions;
using Plugin.NetStandardStorage;
using Plugin.NetStandardStorage.Abstractions.Types;

namespace Plugin.BingSpeech.Services
{
    internal class MicrophoneService : IMicrophoneService
    {
        private readonly int CHANNEL = 1;
        private readonly int BITS_PER_SAMPLE = 16;
        private readonly float SAMPLE_RATE = 8000.0f;
        private readonly int AUDIO_FORMAT_TYPE = (int)AudioToolbox.AudioFormatType.LinearPCM;

        private readonly bool IS_FLOAT_KEY = false;
        private readonly bool IS_BIG_ENDIAN_KEY = false;

        private string outputFilename = null;

        private AVAudioRecorder _audioRecorder = null;

        public void ContinuousDictation()
        {
            throw new NotImplementedException();
        }

        public void StartRecording(string recordingFilename)
        {
            try
            {
                this.outputFilename = recordingFilename;

                if (this._audioRecorder == null)
                {
                    InitializeRecorder();
                }

                this._audioRecorder.Record();
            }
            catch (Exception ex)
            {
                throw new MicrophoneServiceException(ex.Message);
            }
        }

        private void InitializeRecorder()
        {
            var audioSession = AVAudioSession.SharedInstance();

            if (audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord) != null)
            {
                throw new MicrophoneServiceException("AudioSession category setting error !");
            }

            if (audioSession.SetActive(true) != null)
            {
                throw new MicrophoneServiceException("AudioSession activation error !");
            }

            InitialiseAudioRecorder();

            this._audioRecorder.PrepareToRecord();
        }

        private void InitialiseAudioRecorder()
        {
            var outputFilePath = CrossStorage.FileSystem.LocalStorage
                .CreateFile(this.outputFilename, CreationCollisionOption.ReplaceExisting)
                .FullPath;

            var url = NSUrl.FromFilename(outputFilePath);

            var values = new NSObject[]
            {
                NSNumber.FromFloat(this.SAMPLE_RATE),
                NSNumber.FromInt32(this.AUDIO_FORMAT_TYPE),
                NSNumber.FromInt32(this.CHANNEL),
                NSNumber.FromInt32(this.BITS_PER_SAMPLE),
                NSNumber.FromBoolean(this.IS_BIG_ENDIAN_KEY),
                NSNumber.FromBoolean(this.IS_FLOAT_KEY)
            };

            var keys = new NSObject[]
            {
                AVAudioSettings.AVSampleRateKey,
                AVAudioSettings.AVFormatIDKey,
                AVAudioSettings.AVNumberOfChannelsKey,
                AVAudioSettings.AVLinearPCMBitDepthKey,
                AVAudioSettings.AVLinearPCMIsBigEndianKey,
                AVAudioSettings.AVLinearPCMIsFloatKey
            };

            var settingsDictionary = NSDictionary.FromObjectsAndKeys(values, keys);
            var audioSettings = new AudioSettings(settingsDictionary);

            this._audioRecorder = AVAudioRecorder.Create(url, audioSettings, out NSError error);

            if (error != null)
            {
                throw new MicrophoneServiceException("AudioRecorder creation error !");
            }
        }

        public void StopRecording()
        {
            try
            {
                if (this._audioRecorder == null)
                {
                    throw new MicrophoneServiceException("You have to start recording first !");
                }

                if (this.outputFilename == null)
                {
                    throw new MicrophoneServiceException("You have to start recording first !");
                }

                this._audioRecorder.Stop();
            }
            catch (Exception ex)
            {
                throw new MicrophoneServiceException(ex.Message);
            }
        }

        public void RemoveRecording()
        {
            if (this.outputFilename == null)
            {
                throw new MicrophoneServiceException("You have to start recording first !");
            }

            CrossStorage.FileSystem.LocalStorage
                .GetFile(this.outputFilename)
                .Delete();

            this.outputFilename = null;
        }
    }
}
