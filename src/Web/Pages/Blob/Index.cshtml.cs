using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Azure.Identity;
using Azure.Storage.Blobs;
using Web.Data;
using Web.Models;

namespace Web.Pages.Blob;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly AdventureWorksContext _context;

    public IndexModel(ILogger<IndexModel> logger, AdventureWorksContext context)
    {
        _logger = logger;
        _context = context;
    }

    public string JsonData = string.Empty;

    public async Task OnGet()
    {
        var filename = $"Products_{DateTime.Now.ToString("yyyyMMddHHmm")}_web.csv";

        var storageAccountName = System.Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_NAME") ?? throw new InvalidOperationException("Connection string 'STORAGE_ACCOUNT_NAME' not found.");
        DefaultAzureCredential credential = new();
        BlobServiceClient blobServiceClient = new BlobServiceClient(new System.Uri($"https://{storageAccountName}.blob.core.windows.net"), credential);
        BlobContainerClient container = blobServiceClient.GetBlobContainerClient("output-from-web");

        BlobClient blobClient = container.GetBlobClient(filename);

        var data = await _context.Product.ToListAsync();

        string headerLine = string.Join(",", data.First().GetType().GetProperties().Select(property => property.Name));

        IEnumerable<string> lines = from p in data 
                                    let line = string.Join(",", p.GetType().GetProperties().Select(property => property.GetValue(p)))
                                    select line;
        
        List<string> csvData = new List<string>();
        csvData.AddRange(lines);

        StringBuilder csv = new StringBuilder();
        csv.AppendLine(headerLine);
        csvData.ForEach(line => csv.AppendLine(line));

        blobClient.Upload(BinaryData.FromString(csv.ToString()), overwrite: true);

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true
        };

        JsonData = JsonSerializer.Serialize(data, options);
    }
}