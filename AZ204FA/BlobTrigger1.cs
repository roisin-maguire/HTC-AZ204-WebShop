using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;

namespace Company.Function
{
    public class BlobTrigger1
    {
        private readonly ILogger<BlobTrigger1> _logger;

        public BlobTrigger1(ILogger<BlobTrigger1> logger)
        {
            _logger = logger;
        }

        public BlobServiceClient GetBlobServiceClient()
        {
            BlobServiceClient client = new(
            "DefaultEndpointsProtocol=https;AccountName=contosostorage110325;AccountKey=AX7U2cY9zlKVfozczgMOtM5EqMl8nFh51k9wFVA08idzCmtXb7Qk50aTy6+PlvS6gKZ7y6cvG3m6+AStHDK1YQ==;EndpointSuffix=core.windows.net");

            return client;
        }

        [Function(nameof(BlobTrigger1))]
        public async Task Run([BlobTrigger("dbtorestore/{name}", Connection = "")] Stream stream, string name)
        {
            using var blobStreamReader = new StreamReader(stream);
            var content = await blobStreamReader.ReadToEndAsync();

            stream.Position = 0;

            var format = Image.DetectFormat(stream);
            if (format is not null)
            {
                using (Image image = Image.Load(stream))
                { // configuration, pixelType, ImageMetadata, size
                    
                    int width = 100;
                    int height = 100;
                    image.Mutate(x => x.Resize(width, height));
                    var blobServiceClient = GetBlobServiceClient(); // Gets the connection to the Storage Account
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("resize-dbtorestore"); // Connects to the container
                                                                                                                          // var data = BinaryData.FromBytes(image); // Gets the data in the correct format to be uploaded
                    
                    var blobClient = containerClient.GetBlobClient($"thumb_{name}");

                    using (var ms = new MemoryStream())
                    {
                        image.Save(ms, format);
                        ms.Position = 0;
                        await blobClient.UploadAsync(ms, true); // Uploads the file

                    }

                }
            }
            else
            {
                _logger.LogInformation($"@@@@@@@@@@@@Format is null");
            }


            // var carImage = car.ImageFile.OpenReadStream()


            // using Flurl; using Flurl.Http;
            // string imageFilePathUrl = GetImageFilePathUrlFromAzureBlob();
            // Stream stream = await imageFilePathUrl.GetStreamAsync(); using (Image<Rgba32> image = Image.Load<Rgba32>(stream))
            // {
            //     //Resize the loaded image URL...
            // }






        }

        // [Function("HttpTrigger1")]
        // public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        // {
        //     _logger.LogInformation("C# HTTP trigger function processed a request.");
        //     Console.WriteLine("Hello world.");
        //     return new OkObjectResult("Welcome to Azure Functions!");
        // }
    }

}


