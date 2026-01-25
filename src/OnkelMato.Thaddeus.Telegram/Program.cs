
using CalDAV.NET;
using Ical.Net;
using OnkelMato.Thaddeus.Telegram.BackgroundServices;
using OnkelMato.Thaddeus.Telegram.Config;
using OnkelMato.Thaddeus.Telegram.PublishSubscribe;

namespace OnkelMato.Thaddeus.Telegram;

public class Program
{
    public static Client Client;
    public static string CalendarId = "a5cf09b9-c993-82c8-7e50-5af5b831b588";

    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddSingleton<IHostedService, BotBackgroundService>();
        builder.Services.AddSingleton<IHostedService, RadicaleAdapterService>();

        builder.Services.AddSingleton<IBotMessageToRequestConverter, BotMessageToRequestConverter>();

        builder.Services.AddSingleton<IEventAggregator, EventAggregator>();

        builder.Services.Configure<List<UserConfig>>(builder.Configuration.GetSection("User"));
        builder.Services.Configure<BotConfig>(builder.Configuration.GetSection("Bot"));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}