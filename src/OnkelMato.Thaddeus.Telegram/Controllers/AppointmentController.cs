using CalDAV.NET;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnkelMato.Thaddeus.Telegram.Config;
using OnkelMato.Thaddeus.Telegram.Requests;
using Telegram.Bot.Types;

namespace OnkelMato.Thaddeus.Telegram.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppointmentController : ControllerBase
    {
        // it is time to refactor this controller to a service, but for now it is easier to keep it here
        private readonly IOptionsMonitor<BotConfig> _botConfig;
        private readonly IOptionsMonitor<List<UserConfig>> _usersConfig;
        private readonly Dictionary<string, Lazy<Client>> _userClient = new();
        private readonly Dictionary<string, string> _userDefaultCalendar = new();

        public AppointmentController(IOptionsMonitor<BotConfig> botConfig,
            IOptionsMonitor<List<UserConfig>> usersConfig)
        {
            _botConfig = botConfig ?? throw new ArgumentNullException(nameof(botConfig));
            _usersConfig = usersConfig ?? throw new ArgumentNullException(nameof(usersConfig));

            // Connect to server.
            var serverUrl = new Uri(_botConfig.CurrentValue.RadicaleBaseUrl);

            foreach (var user in _usersConfig.CurrentValue)
            {
                _userClient.Add(user.TelegramUserId, new Lazy<Client>(() => new Client(serverUrl, user.RadicaleUser, user.RadicalePassword)));
                _userDefaultCalendar.Add(user.TelegramUserId, user.RadicaleDefaultCalendar);
            }
        }

        [HttpGet(Name = "GetAppointments")]
        public async Task<IEnumerable<Appointment>> Get([FromQuery]string pat)
        {
            var user = _usersConfig.CurrentValue.FirstOrDefault(x => x.CalendarPAT == pat);

            if (!_userClient.TryGetValue(user.TelegramUserId, out var lazyClient))
                return Enumerable.Empty<Appointment>();

            var client = lazyClient.Value;
            var calendarId = _userDefaultCalendar[user.TelegramUserId];

            var cal = client.GetCalendarAsync(calendarId).Result
                      ?? client.GetCalendarsAsync().Result.FirstOrDefault();

            if (cal is null)
                return Enumerable.Empty<Appointment>();
            
            var allCalendars = (await client.GetCalendarsAsync()).ToArray();

            if (allCalendars.Length == 0)
            {
                return Enumerable.Empty<Appointment>();
            }

            var startOfDay = DateTime.Now;
            var endOfDay = startOfDay.AddDays(40);

            var entries = allCalendars.SelectMany(cal => cal
                    .Events
                    .Where(x => x.Start >= startOfDay && x.End <= endOfDay)
                    .Select(x => new { Calendar = cal.DisplayName, Event = x, CalUid = cal.Uid }))
                .ToList();


            // todo fix me tmr


            var res = entries.Select(x => new Appointment
            {
                Start = x.Event.Start,
                End = x.Event.End,
                Title = x.Event.Summary,
                Calendar = x.CalUid == calendarId ? "" : $"[{x.Calendar}] "
            }).OrderBy(x => x.Calendar).ThenBy(x => x.Start).ToArray();

            return res;
        }
    }
}
