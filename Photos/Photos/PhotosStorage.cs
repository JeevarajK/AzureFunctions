using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Photos.AnalyzerService.Abstraction;
using Photos.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photos
{
    public class PhotosStorage
    {
        private readonly IAnalyzerService _analyzerService;

        public PhotosStorage(IAnalyzerService analyzerService)
        {
            this._analyzerService = analyzerService;
        }

        [FunctionName("PhotosStorage")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Blob("photos", FileAccess.ReadWrite, Connection = Constants.StorageConnectionString)] BlobContainerClient containerClient,
            [CosmosDB("photos", 
                        "metadata", 
                        Connection = Constants.CosmosDBConnectionString, 
                        PartitionKey = "/id",
                        CreateIfNotExists = true)] IAsyncCollector<dynamic> items,
            ILogger logger)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var reqest = JsonConvert.DeserializeObject<PhotoUploadModel>(body);

            var newId = Guid.NewGuid();
            var blobName = $"{newId}.jpg";

            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(blobName);

            // Convert base64 string to byte array
            byte[] imageBytes = Convert.FromBase64String(reqest.Photo);

            var analysisResult = await _analyzerService.AnalyzeAsync(imageBytes);

            // Upload the image to the blob
            using (var stream = new MemoryStream(imageBytes))
            {
                await blobClient.UploadAsync(stream, true);
            }

            var newItem = new
            {
                id = newId,
                name = reqest.Name,
                description = reqest.Description,
                tags = reqest.Tags,
                analysis = analysisResult

            };

            await items.AddAsync(newItem);

            logger?.LogInformation($"Successfully uploaded {blobName} file and its metadata at {DateTime.Now.ToString()}");

            return new OkObjectResult(blobName);

        }
    }
}
