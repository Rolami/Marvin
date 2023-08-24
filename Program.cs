
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Configuration;
using Azure.AI.Language.QuestionAnswering;
using Azure;
using Azure.AI.Language;

namespace Marvin
{
    class Program
    {
        IConfigurationRoot configuration;
        //inits of strings
        private string speechKey;
        private string speechRegion;
        private string endpoint;
        private AzureKeyCredential credential;
        private string projectName;
        private string deploymentName;

        private SpeechConfig speechConfig;

        public Program()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            configuration = builder.Build();
            speechKey = configuration["Speech_Key"];
            speechRegion = configuration["Speech_Region"];
            endpoint = "https://INSERTWEBADRESSHERE/";
            credential = new AzureKeyCredential("INSERTMUMBOJUMBOKEY HERE 29aeebdf023 etc");
            projectName = "LANGUAGEPROJECTNAME";//qna needs this for some reason 
            deploymentName = "production"; //qna needs this for some reason
        }

        public async Task RunAsync()
        {
            speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechRecognitionLanguage = "en-US";

            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
            
            string greetings = "Welcome, I am Marvin, how can I help you?";
            MarvinSpeak(greetings);

            while (true)
            {   
                // if the input is either of these, the program stops.
                string[] exitPhrase = {"Stop.", "Exit.", "Quit."};
                var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();

                if (exitPhrase.Contains(speechRecognitionResult.Text))
                    break;

                // Console.WriteLine($"RECOGNIZED: Text={speechRecognitionResult.Text}");
                await MarvinThink(speechRecognitionResult.Text);
            }
        }

        void MarvinSpeak(string responseText)
        {
            speechConfig.SpeechSynthesisVoiceName = "en-GB-RyanNeural";
            using SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig);

            Task.Run(async () =>
            {
                SpeechSynthesisResult speak = await speechSynthesizer.SpeakTextAsync(responseText);
                if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
                {
                    Console.WriteLine(speak.Reason);
                }

                // Console.WriteLine(responseText);
            }).Wait(); 
        }

        public async Task MarvinThink(string question)
        {
            QuestionAnsweringClient client = new QuestionAnsweringClient(new Uri(endpoint), credential);
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

            Response<AnswersResult> response = await client.GetAnswersAsync(question, project);

            foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
            {
                Console.WriteLine($"Q:{question}");
                MarvinSpeak(answer.Answer);
                Console.WriteLine($"A:{answer.Answer}");
            }
        }

        static async Task Main(string[] args)
        {
            var program = new Program();
            await program.RunAsync();
        }
    }
}
