using MongoDB.Entities;
using MongoDB.Driver;
using SearchService.Entities;
using SearchService.Services;

namespace SearchService.Data;

public class DbInit
{

    public static async Task InitAsync(WebApplication app)
    {
        await DB.InitAsync("SearchDB"
        , MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnectionString")));

        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.CountAsync<Item>();
        using var scope = app.Services.CreateScope();
        var httpService = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();
        var items = await httpService.GetItemsForSearchDb();
        Console.WriteLine($"Items from auction service: {items.Count}");
        if (items.Count > 0)
        {
            await DB.SaveAsync(items);
            Console.WriteLine($"Items saved to DB: {items.Count}");
        }
        else
        {
            Console.WriteLine("No items to save");
        }
    }

}
