using Discord;
using Discord.WebSocket;
using MdReactionist;
using MdReactionist.HostedServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<BotOptions>(builder.Configuration.GetSection("BotOptions"));
builder.Services.AddSingleton(
    _ =>
    {
        var client = new DiscordSocketClient(
            new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            });
        var token = Environment.GetEnvironmentVariable("MD_BOT_TOKEN");
        client.LoginAsync(TokenType.Bot, token).Wait();
        client.StartAsync().Wait();

        return client;
    });

builder.Services.AddHostedService<DiscordBotHostedService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
