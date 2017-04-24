using System;
using System.Threading.Tasks;
using Plugin.BingSpeech.Abstractions.Interfaces;
using Plugin.BingSpeech.Exceptions;
using Plugin.NetStandardStorage;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.Storage;

namespace Plugin.BingSpeech.Services
{
    internal class MicrophoneService : IMicrophoneService
    {
        private readonly uint CHANNEL = 1;
        private readonly uint BITS_PER_SAMPLE = 16;
        private readonly uint SAMPLE_RATE = 16000;

        private AudioGraph _audioGraph = null;
        private AudioFileOutputNode _audioFileOutputNode = null;
        private StorageFile _storageFile = null;
        private string outputFilename = null;

        public void ContinuousDictation()
        {
            throw new NotImplementedException();
        }

        public void StartRecording(string recordingFilename)
        {
            try
            {
                this.outputFilename = recordingFilename;

                this.StartRecordingAsync()
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception ex)
            {
                throw new MicrophoneServiceException(ex.Message);
            }
        }

        public void StopRecording()
        {
            try
            {
                var task = Task.Run(() => this.StopRecordingAsync());

                task.Wait();
            }
            catch (Exception ex)
            {
                throw new MicrophoneServiceException(ex.Message);
            }
        }

        private async Task StartRecordingAsync()
        {
            var outputFilePath = CrossStorage.FileSystem.LocalStorage
               .CreateFile(this.outputFilename, NetStandardStorage.Abstractions.Types.CreationCollisionOption.ReplaceExisting)
               .FullPath;

            this._storageFile = await StorageFile.GetFileFromPathAsync(outputFilePath);

            await InitialiseAudioGraph();
            await InitialiseAudioFileOutputNode();
            await InitialiseAudioFeed();

            this._audioGraph.Start();
        }

        private async Task InitialiseAudioFeed()
        {
            var defaultAudioCaptureId = MediaDevice.GetDefaultAudioCaptureId(AudioDeviceRole.Default);
            var microphone = await DeviceInformation.CreateFromIdAsync(defaultAudioCaptureId);

            var inputProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
            var inputResult = await this._audioGraph.CreateDeviceInputNodeAsync(MediaCategory.Media, inputProfile.Audio, microphone);

            if (inputResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                throw new MicrophoneServiceException("AudioDeviceNode creation error !");
            }

            inputResult.DeviceInputNode.AddOutgoingConnection(this._audioFileOutputNode);
        }

        private async Task InitialiseAudioFileOutputNode()
        {
            var outputProfile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Low);
            outputProfile.Audio = AudioEncodingProperties.CreatePcm(this.SAMPLE_RATE, this.CHANNEL, this.BITS_PER_SAMPLE);

            var outputResult = await this._audioGraph.CreateFileOutputNodeAsync(this._storageFile, outputProfile);

            if (outputResult.Status != AudioFileNodeCreationStatus.Success)
            {
                throw new MicrophoneServiceException("AudioFileNode creation error !");
            }

            this._audioFileOutputNode = outputResult.FileOutputNode;
        }

        private async Task InitialiseAudioGraph()
        {
            var audioGraphSettings = new AudioGraphSettings(AudioRenderCategory.Media);
            var audioGraphResult = await AudioGraph.CreateAsync(audioGraphSettings);

            if (audioGraphResult.Status != AudioGraphCreationStatus.Success)
            {
                throw new MicrophoneServiceException("AudioGraph creation error !");
            }

            this._audioGraph = audioGraphResult.Graph;
        }

        private async Task StopRecordingAsync()
        {
            if (this._audioGraph == null)
            {
                throw new MicrophoneServiceException("You have to start recording first !");
            }

            if (this.outputFilename == null)
            {
                throw new MicrophoneServiceException("You have to start recording first !");
            }

            this._audioGraph.Stop();
            await this._audioFileOutputNode.FinalizeAsync();

            this._audioGraph.Dispose();
            this._audioGraph = null;
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
