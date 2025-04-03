using Fish_Manage.Service.Vosk;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using Vosk;

namespace Fish_Manage.Controllers
{
    [ApiController]
    [Route("api/voice")]
    public class VoiceSearchController : ControllerBase
    {
        private readonly Model _model;
        private readonly SpkModel _spkModel;

        public VoiceSearchController(VoskModelService modelService)
        {
            _model = modelService.SpeechModel;
            _spkModel = modelService.SpeakerModel;
        }

        [HttpPost("recognize/bytes")]
        public async Task<IActionResult> RecognizeBytes([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No audio file uploaded.");

            string tempFilePath = Path.GetTempFileName();
            using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Convert WebM to WAV (16000 Hz) using FFmpeg 
            string wavFilePath = Path.ChangeExtension(tempFilePath, ".wav");
            var ffmpeg = new ProcessStartInfo
            {
                FileName = @"E:\ffmpeg\ffmpeg-main\bin\ffmpeg.exe",
                Arguments = $"-i {tempFilePath} -ac 1 -ar 16000 -sample_fmt s16 {wavFilePath}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(ffmpeg).WaitForExit();

            string finalResult;
            using (var stream = new FileStream(wavFilePath, FileMode.Open))
            {
                var rec = new VoskRecognizer(_model, 16000.0f);
                rec.SetWords(true);

                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    rec.AcceptWaveform(buffer, bytesRead);
                }
                finalResult = rec.FinalResult();
            }

            System.IO.File.Delete(tempFilePath);
            System.IO.File.Delete(wavFilePath);

            return Ok(new { result = finalResult });
        }
        private static string ExtractTextFromJson(string json)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("text").GetString() ?? "";
            }
            catch (Exception)
            {
                return "Error parsing result";
            }
        }
    }
}
