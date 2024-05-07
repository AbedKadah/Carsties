using Polly;
using Polly.Extensions.Http;
using SearchService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());

var app = builder.Build();

app.UseAuthentication();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {

        await DbInitializer.InitDb(app);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
{
    return HttpPolicyExtensions
     .HandleTransientHttpError()
     .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
     .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
}