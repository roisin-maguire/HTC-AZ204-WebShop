using System.Runtime.CompilerServices;
using AutoMapper;
using Contoso.Api.Data;
using Contoso.Api.Models;
using Microsoft.EntityFrameworkCore;

using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Contoso.Api.Services;

public class ProductsService : IProductsService
{
    private readonly ContosoDbContext _context;
    private readonly IMapper _mapper;

    public ProductsService(ContosoDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(QueryParameters queryParameters)
    {
        var products = await _context.Products
                            .Where(p => p.Category == queryParameters.filterText || string.IsNullOrEmpty(queryParameters.filterText))
                            .Skip(queryParameters.StartIndex)
                            .Take(queryParameters.PageSize)
                            .ToListAsync();

        var totalCount = await _context.Products
                                        .Where(p => p.Category == queryParameters.filterText || string.IsNullOrEmpty(queryParameters.filterText))
                                        .CountAsync();

        var blobServiceClient = GetBlobServiceClient(); // Gets the connection to the Storage Account
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("dbtorestore"); // Connects to the container
        var sasToken = "sv=2022-11-02&ss=b&srt=sco&sp=rtfx&se=2025-03-15T23:33:12Z&st=2025-03-11T15:33:12Z&spr=https,http&sig=4ei3MaQpFpjoQQ7D50cSvt9KoTMli4vIl5NUxvPsNhg%3D";

        foreach(var aProduct in products) {
            var blobClient = containerClient.GetBlobClient(aProduct.Name);
            BlobProperties properties = await blobClient.GetPropertiesAsync();
            foreach (var metadataItem in properties.Metadata)  {
                if (metadataItem.Key == "releaseDate") {
                    if (DateTime.Parse(metadataItem.Value) > DateTime.Now) {
                        aProduct.ImageUrl = "https://contosostorage110325.blob.core.windows.net/dbtorestore/istockphoto-1412730098-612x612.jpg?"+sasToken;
                    }
                }
            }

        }

        var pagedProducts = new PagedResult<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(products),
            TotalCount = totalCount,
            PageSize = queryParameters.PageSize,
            PageNumber = queryParameters.PageNumber
        };

        return pagedProducts;
    }

    public async Task<ProductDto> GetProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        return _mapper.Map<ProductDto>(product);
    }


    // Used to initialise the Blob Service Client
    public BlobServiceClient GetBlobServiceClient()
    {
        BlobServiceClient client = new(
           "DefaultEndpointsProtocol=https;AccountName=contosostorage110325;AccountKey=AX7U2cY9zlKVfozczgMOtM5EqMl8nFh51k9wFVA08idzCmtXb7Qk50aTy6+PlvS6gKZ7y6cvG3m6+AStHDK1YQ==;EndpointSuffix=core.windows.net");

        return client;
    }


    public async Task<ProductDto> CreateProductAsync(ProductDto product)
    {
        var sasToken = "sv=2022-11-02&ss=b&srt=sco&sp=rtfx&se=2025-03-15T23:33:12Z&st=2025-03-11T15:33:12Z&spr=https,http&sig=4ei3MaQpFpjoQQ7D50cSvt9KoTMli4vIl5NUxvPsNhg%3D";
        var blobServiceClient = GetBlobServiceClient(); // Gets the connection to the Storage Account
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("dbtorestore"); // Connects to the container
        var data = BinaryData.FromBytes(product.Image); // Gets the data in the correct format to be uploaded
        var blobClient = containerClient.GetBlobClient(product.Name);
        await blobClient.UploadAsync(data, true); // Uploads the file
        product.ImageUrl = blobClient.Uri.ToString() + "?" + sasToken; // Sets the image URL to the product object also appends the SAS token



        IDictionary<string, string> metadata = new Dictionary<string, string>();
        // Add metadata to the dictionary by calling the Add method
        metadata.Add("releaseDate", "text");
        metadata["releaseDate"] = DateTime.Now.AddMinutes(4).ToString();
        // Set the blob's metadata.
        await blobClient.SetMetadataAsync(metadata);


        // Below adds the product to the DB
        var productModel = _mapper.Map<Product>(product);
        _context.Products.Add(productModel);
        await _context.SaveChangesAsync();
        return _mapper.Map<ProductDto>(productModel);
    }

    public async Task<ProductDto> UpdateProductAsync(ProductDto product)
    {
        var existingProduct = await _context.Products.AsNoTracking().FirstAsync(x => x.Id == product.Id);

        if (existingProduct == null)
        {
            return null;
        }

        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;

        if (existingProduct.ImageUrl != product.ImageUrl)
        {
            existingProduct.ImageUrl = product.ImageUrl;
        }


        _context.Entry(existingProduct).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        return _mapper.Map<ProductDto>(existingProduct);
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _context.Products.AsNoTracking().FirstAsync(x => x.Id == id);

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();
    }

    public async Task<List<string>> GetProductCategories()
    {
        return await _context.Products.Select(x => x.Category).Distinct().ToListAsync();
    }
}