using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DeepSpeechClient;
using DeepSpeechClient.Interfaces;

namespace MyRecorder
{
    class Program
    {
        private bool _isFiledSaved = false;

        static void Main(string[] args)
        {
            Program program = new Program();
            Recorder audioCapture = new Recorder(program.OnFileSaved);
            UI ui = new UI(audioCapture.OnStartRecording, audioCapture.OnStopRecording, audioCapture.OnExit);
            Thread audioCaptureThread = new Thread(audioCapture.Run);
            audioCaptureThread.Start();

            ui.Run();

            // We wait for the audioCapture logic to finish
            while(audioCapture.IsAlive)
            {
                Thread.Sleep(100);
            }

            if (program._isFiledSaved)
            {
                Console.WriteLine("File saved. Have a nice day");
            }
            else
            {
                Console.WriteLine("Unable to save the file.");
            }
            Console.WriteLine("Good bye.");
        }

        public void OnFileSaved(bool isFiledSaved)
        {
            _isFiledSaved = isFiledSaved;
        }
    }
}
