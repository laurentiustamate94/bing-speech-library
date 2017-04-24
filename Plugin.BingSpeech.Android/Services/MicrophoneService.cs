using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Media;
using Plugin.BingSpeech.Abstractions.Interfaces;
using Plugin.BingSpeech.Exceptions;
using Plugin.CurrentActivity;
using Plugin.NetStandardStorage;
using Plugin.NetStandardStorage.Abstractions.Types;

namespace Plugin.BingSpeech.Services
{
    internal class MicrophoneService : IMicrophoneService
    {
        private readonly int WAV_FILE_PADDING = 36;
        private readonly int BITS_PER_SAMPLE = 16;
        private readonly Encoding ENCODING = Encoding.Pcm16bit;
        private readonly ChannelIn MONO_CHANNEL = ChannelIn.Mono;
        private readonly ChannelIn STEREO_CHANNEL = ChannelIn.Stereo;
        private readonly string OUTPUT_TEMP_FILE = "plugin-bingspeech-audio-temp.wav";

        private int _sampleRate = -1;
        private int _bufferSize = -1;
        private string outputFilename = null;
        private bool _isRecording = false;
        private AudioRecord _audioRecord = null;
        private CancellationTokenSource _cancellationToken = null;

        public void ContinuousDictation()
        {
            throw new NotImplementedException();
        }

        public void StartRecording(string recordingFilename)
        {
            try
            {
                this.outputFilename = recordingFilename;

                InitialiseRecorder();

                if (this._audioRecord == null)
                {
                    throw new MicrophoneServiceException("AudioRecorder initialisation error !");
                }

                this._audioRecord.StartRecording();

                this._isRecording = true;

                this._cancellationToken = new CancellationTokenSource();
                Task.Run(() => PersistAudioFeedToTempFile(), this._cancellationToken.Token);
            }
            catch (Exception ex)
            {
                throw new MicrophoneServiceException(ex.Message);
            }
        }

        private void InitialiseRecorder()
        {
            var context = CrossCurrentActivity.Current.Activity;
            var audioManager = context.GetSystemService(Context.AudioService) as AudioManager;

            this._sampleRate = Int32.Parse(audioManager.GetProperty(AudioManager.PropertyOutputSampleRate));

            if (this._audioRecord != null)
            {
                this._audioRecord.Release();
            }

            this._bufferSize = AudioRecord.GetMinBufferSize(this._sampleRate, this.MONO_CHANNEL, this.ENCODING);
            this._audioRecord = new AudioRecord(AudioSource.Mic, this._sampleRate, this.STEREO_CHANNEL, this.ENCODING, this._bufferSize);
        }

        private void PersistAudioFeedToTempFile()
        {
            var data = new byte[this._bufferSize];
            var tempFile = CrossStorage.FileSystem.LocalStorage
                .CreateFile(this.OUTPUT_TEMP_FILE, CreationCollisionOption.ReplaceExisting);

            using (var tempFileStream = tempFile.Open(FileAccess.Write))
            {
                while (this._isRecording)
                {
                    this._audioRecord.Read(data, 0, this._bufferSize);

                    tempFileStream.Write(data, 0, data.Length);
                }
            }
        }

        public void StopRecording()
        {
            try
            {
                if (this._audioRecord == null)
                {
                    throw new MicrophoneServiceException("You have to start recording first !");
                }

                if (this.outputFilename == null)
                {
                    throw new MicrophoneServiceException("You have to start recording first !");
                }

                this._audioRecord.Stop();

                this._isRecording = false;
                this._cancellationToken.Cancel();

                this._audioRecord.Release();
                this._audioRecord = null;

                CreateOutputFileFromTempFile();
            }
            catch (Exception ex)
            {
                throw new MicrophoneServiceException(ex.Message);
            }
        }

        private void CreateOutputFileFromTempFile()
        {
            var tempFile = CrossStorage.FileSystem.LocalStorage
                .GetFile(this.OUTPUT_TEMP_FILE);
            var outputFile = CrossStorage.FileSystem.LocalStorage
                .CreateFile(this.outputFilename, CreationCollisionOption.ReplaceExisting);

            using (var tempFileStream = tempFile.Open(FileAccess.Read))
            using (var outputFileStream = outputFile.Open(FileAccess.Write))
            {
                var channels = 2;
                var data = new byte[this._bufferSize];
                var byteRate = this.BITS_PER_SAMPLE * (long)this._sampleRate * channels / 8;

                var audioLength = tempFileStream.Length;
                var dataLength = audioLength + this.WAV_FILE_PADDING;

                WriteWaveFileHeader(outputFileStream, audioLength, dataLength, channels, byteRate);

                while (tempFileStream.Read(data, 0, data.Length) != 0)
                {
                    outputFileStream.Write(data, 0, data.Length);
                }

                tempFile.Delete();
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

        #region WAV_HEADER_SPECIFICATION

        private void WriteWaveFileHeader(System.IO.Stream outputStream, long audioLength, long dataLength, int channels, long byteRate)
        {
            byte[] header = new byte[44];

            /* RIFF header */
            header[0] = Convert.ToByte('R');
            header[1] = Convert.ToByte('I');
            header[2] = Convert.ToByte('F');
            header[3] = Convert.ToByte('F');

            /* Data length */
            header[4] = (byte)(dataLength & 0xff);
            header[5] = (byte)((dataLength >> 8) & 0xff);
            header[6] = (byte)((dataLength >> 16) & 0xff);
            header[7] = (byte)((dataLength >> 24) & 0xff);

            /* WAVE header */
            header[8] = Convert.ToByte('W');
            header[9] = Convert.ToByte('A');
            header[10] = Convert.ToByte('V');
            header[11] = Convert.ToByte('E');
            header[12] = Convert.ToByte('f');
            header[13] = Convert.ToByte('m');
            header[14] = Convert.ToByte('t');
            header[15] = (byte)' ';

            /* size of 'fmt ' chunk */
            header[16] = 16;
            header[17] = 0;
            header[18] = 0;
            header[19] = 0;
            header[20] = 1;
            header[21] = 0;

            /* channels */
            header[22] = Convert.ToByte(channels);
            header[23] = 0;

            /* sample rate */
            header[24] = (byte)(((long)this._sampleRate) & 0xff);
            header[25] = (byte)((((long)this._sampleRate) >> 8) & 0xff);
            header[26] = (byte)((((long)this._sampleRate) >> 16) & 0xff);
            header[27] = (byte)((((long)this._sampleRate) >> 24) & 0xff);

            /* byte rate */
            header[28] = (byte)(byteRate & 0xff);
            header[29] = (byte)((byteRate >> 8) & 0xff);
            header[30] = (byte)((byteRate >> 16) & 0xff);
            header[31] = (byte)((byteRate >> 24) & 0xff);

            /* alignment */
            header[32] = (byte)(2 * 16 / 8);
            header[33] = 0;

            /* bits per sample */
            header[34] = Convert.ToByte(this.BITS_PER_SAMPLE);
            header[35] = 0;

            /* Data header */
            header[36] = Convert.ToByte('d');
            header[37] = Convert.ToByte('a');
            header[38] = Convert.ToByte('t');
            header[39] = Convert.ToByte('a');

            /* audio length */
            header[40] = (byte)(audioLength & 0xff);
            header[41] = (byte)((audioLength >> 8) & 0xff);
            header[42] = (byte)((audioLength >> 16) & 0xff);
            header[43] = (byte)((audioLength >> 24) & 0xff);

            outputStream.Write(header, 0, 44);
        }

        #endregion
    }
}
