using System;
using System.IO;
using System.Threading;
using OpenTK.Audio.OpenAL;

namespace MyRecorder
{
    public class Recorder
    {
        private bool _quit = false;
        private bool _record = false;

        public bool IsAlive { get; private set; }

        private readonly Action<bool> _savedFileCb;

        public Recorder(Action<bool> savedFileCb)
        {
            _savedFileCb = savedFileCb;
        }
        
        public void Run()
        {
            IsAlive = true;
            while(!_quit)
            {
                if (_record)
                    DoRecording();
                Thread.Sleep(100);
            }
            IsAlive = false;
        }

        private void DoRecording()
        {
            int sampleRate = 16000; // 44100;
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            short[] recording = new short[1024];
            int numSamples = 0;
            Thread.Sleep(200);
            ALCaptureDevice captureDevice = ALC.CaptureOpenDevice(null, sampleRate, ALFormat.Mono16, 1024);
            {
                ALC.CaptureStart(captureDevice);
                while (_record)
                {
                    int current = 0;
                    while (current < recording.Length)
                    {
                        int samplesAvailable = ALC.GetAvailableSamples(captureDevice);
                        if (samplesAvailable > 512)
                        {
                            int samplesToRead = Math.Min(samplesAvailable, recording.Length - current);
                            ALC.CaptureSamples(captureDevice, ref recording[current], samplesToRead);
                            current += samplesToRead;
                        }
                        Thread.Yield();
                    }
                    byte[] result = new byte[current * sizeof(short)];
                    Buffer.BlockCopy(recording, 0, result, 0, result.Length);
                    writer.Write(result);
                    numSamples += current;
                }
                ALC.CaptureStop(captureDevice);
                ALC.CaptureCloseDevice(captureDevice);
            }
            writer.Flush();
            stream.Flush();
            var buffer = stream.ToArray();
            WriteDataToFile(buffer, numSamples, sampleRate);
            Transcribe(buffer, numSamples);
        }

        private void WriteDataToFile(byte[] dataStream, int numSamples, int sampleRate)
        {
            try
            {
                // http://soundfile.sapp.org/doc/WaveFormat/
                // ALFormat.Mono16 = 1 channel, 16 bits
                int numChannels = 1; // Mono
                int bitsPerSample = 16; // Mono16
                int Subchunk2Size = numSamples * numChannels * bitsPerSample / 8;
                using FileStream stream = new FileStream("test.wav", FileMode.Create);
                using BinaryWriter writer = new BinaryWriter(stream);
                // header
                // ChunkID
                writer.Write(new [] { 'R', 'I', 'F', 'F' });
                // ChunkSize
                writer.Write(36 + Subchunk2Size);
                // Format
                writer.Write(new [] { 'W', 'A', 'V', 'E' });
                // Subchunk1ID
                writer.Write(new [] { 'f', 'm', 't', ' '});
                // Subchunk1Size
                writer.Write(16); // 16 for PCM
                // AudioFormat
                writer.Write(Convert.ToInt16(1)); // 1 = PCM (Linear quantization)
                // NumChannels
                writer.Write(Convert.ToInt16(numChannels));
                // SampleRate
                writer.Write(sampleRate);
                // ByteRate
                writer.Write(sampleRate * numChannels * bitsPerSample / 8);
                // BlockAlign
                writer.Write(Convert.ToInt16(numChannels * bitsPerSample / 8));
                // BitsPerSample
                writer.Write(Convert.ToInt16(16));
                // SubChunk2ID
                writer.Write(new [] {'d', 'a', 't', 'a'});
                // Subchunk2Size
                writer.Write(Subchunk2Size);
                writer.Flush();
                // data
                // dataStream.WriteTo(stream);
                writer.Write(dataStream);
                stream.Flush();
                _savedFileCb(true);
            }
            catch
            {
                _savedFileCb(false);
            }
        }

        private void Transcribe(byte[] buffer, int numSamples)
        {
            short[] sdata = new short[(int)Math.Ceiling(buffer.Length / 2d)];
            Buffer.BlockCopy(buffer, 0, sdata, 0, buffer.Length);

            new SpeechToText()
                .Run(sdata, numSamples);
        }

        public void OnStartRecording()
        {
            _record = true;
        }

        public void OnStopRecording()
        {
            _record = false;
            Console.WriteLine("Recording stopped.");
        }

        public void OnExit()
        {
            _quit = true;
        }
    }
}