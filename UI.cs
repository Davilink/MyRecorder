using System;

namespace MyRecorder
{
    public class UI
    {
        private readonly Action _startRecording, _stopRecording, _exit;

        public UI(Action startRecording, Action stopRecording, Action exit)
        {
            _startRecording = startRecording;
            _stopRecording = stopRecording;
            _exit = exit;
        }

        public void Run()
        {
            Console.WriteLine("Type 'record' and press « enter » to start recording your beautiful voice or 'exit' to close the program.");
            string line;
            while ((line = Console.ReadLine()) != "exit")
            {
                if (line == "record")
                {
                    Console.WriteLine("Press any key to stop the recording.");
                    _startRecording();
                    Console.Read();
                    _stopRecording();
                }
                else
                {
                    Console.WriteLine("unreconized command.");
                }
            }
            _exit();
        }
    }
}