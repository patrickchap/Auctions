namespace SearchService.Services;
using SearchService.Entities;
using MongoDB.Entities;

public class AuctionServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuctionServiceHttpClient> _logger;
    private readonly IConfiguration _config;

    public AuctionServiceHttpClient(HttpClient httpClient, IConfiguration config, ILogger<AuctionServiceHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
    }


    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        return await _httpClient
            .GetFromJsonAsync<List<Item>>($"{_config["AuctionServiceUrl"]}/api/auctions?date={lastUpdated}");
    }
}
