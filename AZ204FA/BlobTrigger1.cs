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
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");
            using (Image image = Image.Load(stream))
            {
                int width = 100;
                int height = 100;
                image.Mutate(x => x.Resize(width, height));

                var sasToken = "sv=2022-11-02&ss=b&srt=sco&sp=rtfx&se=2025-03-15T23:33:12Z&st=2025-03-11T15:33:12Z&spr=https,http&sig=4ei3MaQpFpjoQQ7D50cSvt9KoTMli4vIl5NUxvPsNhg%3D";
                var blobServiceClient = GetBlobServiceClient(); // Gets the connection to the Storage Account
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("resize-dbtorestore"); // Connects to the container
                // var data = BinaryData.FromBytes(image); // Gets the data in the correct format to be uploaded
                var blobClient = containerClient.GetBlobClient($"thumb_{name}");
                var outPath = $"/temp_images/thumb_{name}";
                image.Save(outPath);
                await blobClient.UploadAsync(outPath, true); // Uploads the file


            }

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


