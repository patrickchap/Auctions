

using SearchService.Data;
using SearchService.Services;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInit.InitAsync(app);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
});




app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy() => HttpPolicyExtensions.HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
    .WaitAndRetryAsync(10, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} encountered an error: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}. Waiting {timespan} before next retry.");
        });
