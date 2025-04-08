using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Entities;

namespace AUCTIONS.SearchService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ILogger<SearchController> _logger;

    public SearchController(ILogger<SearchController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] string? searchTerm, 
        [FromQuery] string sortBy = "make",
        [FromQuery] int? pageNumber = 1, 
        [FromQuery] int? pageSize = 4)
    {
        var query = DB.PagedSearch<Item>();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query.Match(Search.Full, searchTerm).SortByTextScore();
        }

        switch(sortBy){
            case "make":
                query.Sort(x => x.Ascending(a => a.Make));
                break;
            case "new":
                query.Sort(x => x.Descending(a => a.CreatedAt));
                break;
        }

        query.PageNumber(pageNumber ?? 1);
        query.PageSize(pageSize ?? 4);

        var result = await query.ExecuteAsync();

        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }

} 