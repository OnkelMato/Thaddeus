using Microsoft.Extensions.Options;
using OnkelMato.Thaddeus.Telegram.Config;
using OnkelMato.Thaddeus.Telegram.PublishSubscribe;
using OnkelMato.Thaddeus.Telegram.Requests;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace OnkelMato.Thaddeus.Telegram.BackgroundServices;

public class BotBackgroundService : IHostedService, IHandle<SendBotMessageRequest>
{
    public static TelegramBotClient Bot;
    private readonly IBotMessageToRequestConverter _botRequestToRequestConverter = new BotMessageToRequestConverter();
    private readonly IEventAggregator _eventAggregator;
    private readonly IOptionsMonitor<BotConfig> _botConfig;
    private readonly IOptionsMonitor<IEnumerable<UserConfig>> _userConfig;

    public BotBackgroundService(IEventAggregator eventAggregator,
        IOptionsMonitor<BotConfig> botConfig,
        IOptionsMonitor<List<UserConfig>> userConfig)
    {
        _eventAggregator = eventAggregator;
        _botConfig = botConfig;
        _userConfig = userConfig;

        _eventAggregator.Subscribe(this);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Bot = new TelegramBotClient(new TelegramBotClientOptions(_botConfig.CurrentValue.TelegramBotToken));

        Bot.OnMessage += async (sender, e) =>
        {
            if (e == UpdateType.Message)
            {
                try
                {

                    var message = _botRequestToRequestConverter.Convert(sender, e);
                    await _eventAggregator.PublishAsync(message, cancellationToken);

                    
                }
                catch (Exception ex)
                {
                    Bot.SendMessage(sender.Chat.Id, "Fehler: " + ex.Message);
                }
            }
        };

        var me = await Bot.GetMe();
        Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }

    public async Task HandleAsync(SendBotMessageRequest message, CancellationToken cancellationToken)
    {
        await Bot.SendMessage(message.ChatId, message.Message, ParseMode.Markdown);
    }
}