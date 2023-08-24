using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;

class Program 
{
    IConfigurationRoot configuration;

    public Program()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
        configuration = builder.Build();
    }

    public async Task RunAsync()
    {
        string speechKey = configuration["Speech_Key"];
        string speechRegion = configuration["Speech_Region"];
        
        var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);        
        speechConfig.SpeechRecognitionLanguage = "en-US";

        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

        Console.WriteLine("Welcome, I am Marvin, how can I help you?");
        var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
        OutputSpeechRecognitionResult(speechRecognitionResult);
    }

    static void OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
    {
        switch (speechRecognitionResult.Reason)
        {
            case ResultReason.RecognizedSpeech:
                Console.WriteLine($"RECOGNIZED: Text={speechRecognitionResult.Text}");
                break;
            case ResultReason.NoMatch:
                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                break;
            case ResultReason.Canceled:
                var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                    Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                }
                break;
        }
    }

    static async Task Main(string[] args)
    {
        var program = new Program();
        await program.RunAsync();
    }
}
