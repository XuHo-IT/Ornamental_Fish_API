using Vosk;

namespace Fish_Manage.Service.Vosk
{
    public class VoskModelService
    {
        public Model SpeechModel { get; set; }
        public SpkModel SpeakerModel { get; set; }

        public VoskModelService()
        {
            try
            {
                SpeechModel = new Model(@"E:\vosk\vosk-model-en-us-0.22");
                SpeakerModel = new SpkModel(@"E:\vosk-spk\vosk-model-spk-0.4");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

        }
    }
}
