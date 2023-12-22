using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photos
{
    public static class PhotosDownload
    {
        [FunctionName("PhotosDownload")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "photos/{id}")] HttpRequest req,
            [Blob("photos-small/{id}.jpg", FileAccess.Read, Connection = Constants.StorageConnectionString)] Stream smallStream,
            [Blob("photos-medium/{id}.jpg", FileAccess.Read, Connection = Constants.StorageConnectionString)] Stream mediumStream,
            [Blob("photos/{id}.jpg", FileAccess.Read, Connection = Constants.StorageConnectionString)] Stream originalStream,
            Guid id,
            ILogger logger)
        {
            logger?.LogInformation($"Downloading {id}...");

            byte[] data;
            string size = req.Query["size"];

            if (size == "small")
            {
                logger?.LogInformation($"Downloading small size image...");
                data = await GetBytesFromStream(smallStream);
            }
            else if (size == "medium")
            {
                logger?.LogInformation($"Downloading medium size image...");
                data = await GetBytesFromStream(mediumStream);
            }
            else
            {
                logger?.LogInformation($"Downloading original size image...");
                data = await GetBytesFromStream(originalStream);
            }

            return new FileContentResult(data, "image/jpeg")
            {
                FileDownloadName = $"{id}.jpeg"
            };
        }

        private static async Task<byte[]> GetBytesFromStream(Stream stream)
        {
            byte[] data = new byte[stream.Length];
            await stream.ReadAsync(data, 0, data.Length);
            return data;
        }
    }
}
