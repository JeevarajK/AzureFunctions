using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photos
{
    public class PhotosResizer
    {
        [FunctionName("PhotosResizer")]
        public async Task Run([BlobTrigger("photos/{name}", Connection = Constants.StorageConnectionString)]Stream myBlob,
            [Blob("photos-small/{name}", FileAccess.Write, Connection = Constants.StorageConnectionString)] Stream imageSmall,
            [Blob("photos-medium/{name}", FileAccess.Write, Connection = Constants.StorageConnectionString)] Stream imageMedium,
            string name, 
            ILogger logger)
        {
            logger?.LogInformation("Resizing...");

            try
            {
                await UploadResizedImage(myBlob, imageSmall, ImageSize.Small);
                await UploadResizedImage(myBlob, imageMedium, ImageSize.Medium);

                logger?.LogInformation($"Successfully resized");
            }
            catch (Exception ex)
            {

                logger?.LogError(ex.ToString());
            }
        }

        [FunctionName("PhotosResizerContainer")]
        public async Task RunContainer([BlobTrigger("photos/{name}", Connection = Constants.StorageConnectionString)] Stream myBlob,
            [Blob("photos-small-1", FileAccess.Write, Connection = Constants.StorageConnectionString)] BlobContainerClient blobContainerSmall,
            [Blob("photos-medium-1", FileAccess.Write, Connection = Constants.StorageConnectionString)] BlobContainerClient blobContainerMedium,
            string name,
            ILogger logger)
        {
            logger?.LogInformation("Resizing with container...");

            try
            {
                await blobContainerSmall.CreateIfNotExistsAsync();
                await blobContainerMedium.CreateIfNotExistsAsync();

                var blobClientSmall = blobContainerSmall.GetBlobClient(name);

                var blobClientMedium = blobContainerMedium.GetBlobClient(name);

                MemoryStream msSmall = new MemoryStream();
                await UploadResizedImage(myBlob, msSmall, ImageSize.Small);
                msSmall.Position = 0;
                await blobClientSmall.UploadAsync(msSmall);

                MemoryStream msMedium = new MemoryStream();
                await UploadResizedImage(myBlob, msMedium, ImageSize.Medium);
                msMedium.Position = 0;
                await blobClientMedium.UploadAsync(msMedium);

                logger?.LogInformation($"Successfully resized with container");
            }
            catch (Exception ex)
            {

                logger?.LogError(ex.ToString());
            }
        }

        private async static Task UploadResizedImage(Stream myBlob, Stream targetBlob, ImageSize imageSize)
        {
            using (var image = await SixLabors.ImageSharp.Image.LoadAsync(myBlob))
            {
                // Adjust the size as needed
                int targetWidth = imageSize == ImageSize.Medium ? image.Width / 2 : image.Width / 4;
                int targetHeight = imageSize == ImageSize.Medium ? image.Height / 2 : image.Height / 4;

                image.Mutate(x => x
                    .Resize(new ResizeOptions
                    {
                        Size = new SixLabors.ImageSharp.Size(targetWidth, targetHeight),
                        Mode = ResizeMode.Pad
                    }));

                // Save the resized image to the destination container
                await image.SaveAsync(targetBlob, new JpegEncoder());

                //Reset source stream position to start for next read
                myBlob.Position = 0;
            }
        }

        private enum ImageSize
        {
            Medium,
            Small
        }

    }
}
