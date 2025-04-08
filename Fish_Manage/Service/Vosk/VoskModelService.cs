using Vosk;

namespace Fish_Manage.Service.Vosk
{
    public class VoskModelService
    {
        private readonly IConfiguration _configuration;
        public Model SpeechModel { get; set; }
        public SpkModel SpeakerModel { get; set; }

        public VoskModelService(IConfiguration configuration)
        {
            _configuration = configuration;

            try
            {
                var speechModelPath = _configuration["Vosk:En-Us"];
                var speakerModelPath = _configuration["Vosk:Model-spk"];

                SpeechModel = new Model(speechModelPath);
                SpeakerModel = new SpkModel(speakerModelPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Vosk models: {ex.Message}");
                throw;
            }
        }
    }
}
