using CalDAV.NET;
using Microsoft.Extensions.Options;
using OnkelMato.Thaddeus.Telegram.Config;
using OnkelMato.Thaddeus.Telegram.PublishSubscribe;
using OnkelMato.Thaddeus.Telegram.Requests;

namespace OnkelMato.Thaddeus.Telegram.BackgroundServices;

public class RadicaleAdapterService : IHostedService, IHandle<AddAppointmentRequest>, IHandle<GetAppointmentsRequest>
{
    private readonly Dictionary<string, Lazy<Client>> _userClient = new();
    private readonly Dictionary<string, string> _userDefaultCalendar = new();
    private readonly IEventAggregator _eventAggregator;
    private readonly IOptionsMonitor<BotConfig> _botConfig;
    private readonly IOptionsMonitor<IEnumerable<UserConfig>> _usersConfig;

    public RadicaleAdapterService(IEventAggregator eventAggregator,
        IOptionsMonitor<BotConfig> botConfig,
        IOptionsMonitor<List<UserConfig>> usersConfig)
    {
        _eventAggregator = eventAggregator;
        _botConfig = botConfig;
        _usersConfig = usersConfig;

        eventAggregator.Subscribe(this);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Connect to server.
        var serverUrl = new Uri(_botConfig.CurrentValue.RadicaleBaseUrl);

        foreach (var user in _usersConfig.CurrentValue)
        {
            _userClient.Add(user.TelegramUserId, new Lazy<Client>(() => new Client(serverUrl, user.RadicaleUser, user.RadicalePassword)));
            _userDefaultCalendar.Add(user.TelegramUserId, user.RadicaleDefaultCalendar);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _userClient.Clear();
    }

    public async Task HandleAsync(AddAppointmentRequest message, CancellationToken cancellationToken)
    {
        if (!_userClient.TryGetValue(message.TelegramUserId, out var lazyClient)) return;

        var client = lazyClient.Value;
        var calendarId = _userDefaultCalendar[message.TelegramUserId];

        var cal = client.GetCalendarAsync(calendarId).Result
                  ?? client.GetCalendarsAsync().Result.FirstOrDefault();

        if (cal is null)
        {
            await _eventAggregator.PublishAsync(new SendBotMessageRequest(message.ChatId, message.TelegramUserId, "cannot get calendar."), cancellationToken);
            return;
        }

        var evt = cal.CreateEvent(message.Appointment.Title, message.Appointment.Start, message.Appointment.End);
        await cal.SaveChangesAsync();
    }

    public async Task HandleAsync(GetAppointmentsRequest message, CancellationToken cancellationToken)
    {
        if (!_userClient.TryGetValue(message.TelegramUserId, out var lazyClient)) return;

        var client = lazyClient.Value;
        var calendarId = _userDefaultCalendar[message.TelegramUserId];

        var cal = client.GetCalendarAsync(calendarId).Result
                  ?? client.GetCalendarsAsync().Result.FirstOrDefault();

        if (cal is null)
        {
            await _eventAggregator.PublishAsync(new SendBotMessageRequest(message.ChatId, message.TelegramUserId, "cannot get calendar."), cancellationToken);
            return;
        }

        var startOfDay = new DateTime(message.Date.Year, message.Date.Month, message.Date.Day, 0, 0, 0);
        var endOfDay = startOfDay.AddDays(1);

        var entries = cal.Events.Where(x =>
            x.Start>= startOfDay && x.End <= endOfDay).ToList();

        message.Appointments = entries.Select(x => new Appointment
        {
            Start = x.Start,
            End = x.End,
            Title = x.Summary,
        });
        if (message.Appointments.Count() == 0)
        {
            await _eventAggregator.PublishAsync(new SendBotMessageRequest(message.ChatId, message.TelegramUserId, $"Keine Termine f&uuml;r den {message.Date:d.M.yyyy} eingetragen"), cancellationToken);
            return;
        }

        var response = string.Empty;
        foreach (var appointment in message.Appointments)
            response += $"{appointment.Start:HH:mm} -> {appointment.Title} (bis {appointment.End:HH:mm})\n";
        await _eventAggregator.PublishAsync(new SendBotMessageRequest(message.ChatId, message.TelegramUserId, response), cancellationToken);
    }
}