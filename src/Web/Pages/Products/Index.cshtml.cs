using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Web.Data;
using Web.Models;

namespace Web.Pages.Products;

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

    public async Task OnGetAsync()
    {
        _logger.LogInformation("Getting product categories");

        var query = from a in _context.ProductCategory 
                    join b in _context.ProductCategory on a.ProductCategoryID equals b.ParentProductCategoryID 
                    join p in _context.Product on b.ProductCategoryID equals p.ProductCategoryID 
                    group new { a, b, p } by new { b.ProductCategoryID, Category = a.Name, SubCategory = b.Name } into g 
                    orderby g.Key.ProductCategoryID  
                    select new 
                    {
                        ProductCategoryID = g.Key.ProductCategoryID, 
                        Category = g.Key.Category,
                        SubCategory = g.Key.SubCategory,
                        ProductCount = g.Count()
                    };

        var productCategories = await query.ToListAsync();

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true
        };

        JsonData = JsonSerializer.Serialize(productCategories, options);
    }
}