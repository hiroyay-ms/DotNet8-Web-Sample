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

    public void OnGet()
    {

    }
}