using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Azure.Identity;
using Web.Models;

namespace Web.Pages.Users;

public class SqlModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConfiguration _configuration;

    public SqlModel (ILogger<IndexModel> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [BindProperty(SupportsGet = true)]
    public string? userid {get; set;} = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? schema { get; set; } = string.Empty;

    public async Task OnGet()
    {
        if (string.IsNullOrEmpty(userid) || string.IsNullOrEmpty(schema)) {
            ViewData["message"] = "Please provide the user id and schema";
            return;
        }

        ViewData["connection"] = $"User Id: {userid}; SChema: {schema}";
        
        var connectionString =  string.Format(_configuration.GetValue<string>("SQL_CONNECTION_STRING") + "User Id={0}", userid) ?? throw new InvalidOperationException("Connection string 'SQL_CONNECTION_STRING' not found.");

        var query = string.Empty;
        if (schema == "SalesLT") {
            query = "SELECT * FROM [SalesLT].[Product]";
        }
        else {
            query = "SELECT * FROM [Pubs].[titles]";
        }

        ViewData["connectionString"] = connectionString;
        _logger.LogInformation($"Connection String: {connectionString}");

        ViewData["query"] = query;
        _logger.LogInformation($"Query: {query}");

        try {
            using (var connection = new SqlConnection(connectionString))
            {
                var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = userid });
                var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }));
                    
                connection.AccessToken = token.Token;
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var data = new List<Dictionary<string, object>>();
                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            data.Add(row);
                        }

                        ViewData["message"] = data;
                    }
                }
            }
        } catch (Exception ex) {
            ViewData["message"] = $"The query was not successfully executed. {ex.Message}";
        }
    }
}