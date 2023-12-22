using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Configuration;

//using Microsoft.Extensions.Configuration;
using Photos.AnalyzerService.Abstraction;

namespace Photos.AnalyzerService
{
    public class ComputerVisionAnalyzerService : IAnalyzerService
    {
        private readonly ComputerVisionClient _computerVisionClient;

        public ComputerVisionAnalyzerService(IConfiguration config) 
        {
            var visionKey = config["visionKey"];
            var visionEndpoint = config["visionEndpoint"];

            _computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(visionKey))
            {
                Endpoint = visionEndpoint
            };
        }

        public async Task<dynamic> AnalyzeAsync(byte[] data)
        {
            using var ms = new MemoryStream(data);
            var analysisResult = await _computerVisionClient.AnalyzeImageInStreamAsync(ms);

            var result = new
            {
                metadata = new
                {
                    width = analysisResult.Metadata.Width,
                    height = analysisResult.Metadata.Height,
                    format = analysisResult.Metadata.Format,
                },
                categories = analysisResult.Categories.Select(s => s.Name).ToArray()
            };

            return result;
        }
    }
}
