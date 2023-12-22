using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Photos.Models;
using Microsoft.Azure.Cosmos.Linq;

namespace Photos
{
    public static class PhotosSearch
    {
        [FunctionName("PhotosSearch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [CosmosDB("photos", "metadata", Connection = Constants.CosmosDBConnectionString)] IEnumerable<PhotoUploadModel> photos,
            ILogger logger)
        {
            logger?.LogInformation("Searching...");

            string searchTerm = req.Query["searchTerm"];

            if (string.IsNullOrEmpty(searchTerm))
            {
                return new NotFoundObjectResult("NotFound");
            }

            var result = photos.Where(p => p.Description.Contains(searchTerm));

            //var uri = photos.Select(s=> new )
            return new OkObjectResult(result);
        }
    }
}
