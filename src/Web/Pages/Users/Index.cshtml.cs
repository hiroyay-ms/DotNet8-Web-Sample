using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Web.Models;

namespace Web.Pages.Users;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConfiguration _configuration;

    public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [BindProperty(SupportsGet = true)]
    public string? userid {get; set;} = "id-aw-saleslt";

    [BindProperty(SupportsGet = true)]
    public string? schema { get; set; } = "SalesLT";

    public async Task OnGet()
    {
        _logger.LogInformation($"User Id: {userid}; SChema: {schema}");

        ViewData["connection"] = $"User Id: {userid}; SChema: {schema}";

        var serverName = _configuration.GetValue<string>("SQL_SERVER_NAME") ?? throw new InvalidOperationException("SQL_SERVER_NAME not found in configuration.");
        var connectionString = $"Server=tcp:{serverName}.database.windows.net,1433;Initial Catalog=AdventureWorksLT;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default;User Id={userid}"

        _logger.LogInformation($"Connection String: {connectionString}");

        ViewData["connectionString"] = connectionString;

        var query = string.Empty;
        if (schema == "SalesLT") {
            query = "SELECT * FROM [SalesLT].[Product]";
        }
        else {
            query = "SELECT * FROM [Pubs].[titles]";
        }

        _logger.LogInformation($"Query: {query}");

        ViewData["query"] = query;

        
        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var data = new List<object>();
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.GetValue(i);
                        }
                        data.Add(row);
                    }

                    var options = new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                        WriteIndented = true
                    };

                    ViewData["json"] = JsonSerializer.Serialize(data, options);
                }
            }
        }
    }
}