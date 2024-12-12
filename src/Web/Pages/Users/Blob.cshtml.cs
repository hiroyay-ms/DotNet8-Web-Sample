using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace Web.Pages.Users;

public class BlobModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public BlobModel (ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? userId {get; set;} = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? containerName { get; set; } = string.Empty;

    public void OnGet()
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(containerName)) {
            ViewData["message"] = "Please provide the user id and container name";
            return;
        }

        var filename = $"HelloWorld_{DateTime.Now.ToString("yyyyMMddHHmm")}_web.csv";

        try {
            var storageAccountName = System.Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_NAME") ?? throw new InvalidOperationException("Connection string 'STORAGE_ACCOUNT_NAME' not found.");
            DefaultAzureCredential credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = userId });

            //BlobServiceClient blobServiceClient = new BlobServiceClient(new System.Uri($"https://{storageAccountName}.blob.core.windows.net"), credential);
            BlobContainerClient container = new BlobContainerClient(new System.Uri($"https://{storageAccountName}.blob.core.windows.net/{containerName}"), credential);

            BlobClient blobClient = container.GetBlobClient(filename);

            string text = $"Hello World from {userId}";
            blobClient.Upload(BinaryData.FromString(text), overwrite: true);

            ViewData["message"] = $"The file was successfully uploaded on blob storage with name {filename}";
        } catch (Exception ex) {
            ViewData["message"] = $"The file was not successfully uploaded on blob storage. {ex.Message}";
        }
    }
}