using System;
using System.IO;
using DeepSpeechClient;
using DeepSpeechClient.Interfaces;

namespace MyRecorder
{
    public class SpeechToText
    {
        private const string MODEL_FILENAME = "deepspeech-0.8.1-models.pbmm";
        private const string SCORER_FILENAME = "deepspeech-0.8.1-models.scorer";

        public void Run(short[] buffer, int numSamples)
        {
            string modelFilePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "deepspeech",
                MODEL_FILENAME
            );

            string scorerFilePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "deepspeech",
                SCORER_FILENAME
            );

            try
            {
                using IDeepSpeech sttClient = new DeepSpeech(modelFilePath);
                sttClient.EnableExternalScorer(scorerFilePath);

                var speechResultWave = sttClient.SpeechToText(buffer, Convert.ToUInt32(numSamples));
                Console.WriteLine("Reconized text: " + speechResultWave);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}