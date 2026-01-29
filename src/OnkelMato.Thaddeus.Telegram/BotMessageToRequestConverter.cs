using System.Reflection;
using OnkelMato.Thaddeus.Telegram.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OnkelMato.Thaddeus.Telegram;

public class BotMessageToRequestConverter : IBotMessageToRequestConverter
{
    public RequestBase Convert(Message message, UpdateType type)
    {
        if (message.Text.StartsWith("Wann ist ", StringComparison.InvariantCultureIgnoreCase))
        {
            var parts = message.Text.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) // todo return result with error message
                return new EmptyRequestBase(message.Chat.Id, message.Chat.Id.ToString());

            return new FindAppointmentTimeRequest(message.Chat.Id, message.Chat.Id.ToString(), parts[2]);
        }

        if (message.Text.StartsWith("Termine ", StringComparison.InvariantCultureIgnoreCase))
        {
            var date = ParseDate(message.Text.Split(' ')[1]);
            if (date == null) // todo return result with error message
                return new EmptyRequestBase(message.Chat.Id, message.Chat.Id.ToString());

            return new GetAppointmentsRequest(message.Chat.Id, message.Chat.Id.ToString(), date.Value);
        }

        if (message.Text.StartsWith("Termin ", StringComparison.InvariantCultureIgnoreCase))
        {
            var part = message.Text.Split(' ', 4, StringSplitOptions.TrimEntries);
            if (part.Length < 3) // todo return result with error message
                return new EmptyRequestBase(message.Chat.Id, message.Chat.Id.ToString());

            if (string.IsNullOrWhiteSpace(part[3])) // todo return result with error message
                return new EmptyRequestBase(message.Chat.Id, message.Chat.Id.ToString());

            var date = ParseDate(part[1]);
            if (date == null) // todo return result with error message
                return new EmptyRequestBase(message.Chat.Id, message.Chat.Id.ToString());

            var time = ParseTime(part[2]);
            string title;
            TimeOnly endTime;
            if (time == null) // all day event, so re-split text
            {
                part = message.Text.Split(' ', 3, StringSplitOptions.TrimEntries);
                title = part[2];
                time = new TimeOnly(0, 0);
                endTime = new TimeOnly(23, 59);
            }
            else
            {
                title = part[3];
                endTime = time.Value.AddMinutes(29); // because we add a minute later. yeah, quite a hack
            }

            return new AddAppointmentRequest(message.Chat.Id, message.Chat.Id.ToString(), new Appointment()
            {
                Start = date.Value.ToDateTime(time.Value),
                End = date.Value.ToDateTime(endTime).AddMinutes(1), // so we get a full-day event
                Title = title
            });
        }

        // make some factory here. or as extension method link message.GetRequestBase();
        return new EmptyRequestBase(message.Chat.Id, message.Chat.Id.ToString());
    }

    private TimeOnly? ParseTime(string time)
    {
        time = time.Trim(',', '.'); // get rid of trailing comma
        if (TimeOnly.TryParseExact(time, "H:mm", out var timeResult))
            return timeResult;

        var parts = time.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1 && int.TryParse(parts[0], out var hour))
        {
            return new TimeOnly(hour, 0);
        }

        return null;
    }

    private DateOnly? ParseDate(string date)
    {
        if (DateOnly.TryParseExact(date, "d.M.yyyy", out var dteResult))
            return dteResult;

        switch (date.ToLower())
        {
            case "gestern": return DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
            case "heute": return DateOnly.FromDateTime(DateTime.Now);
            case "morgen": return DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        }

        var parts = date.Split('.', StringSplitOptions.RemoveEmptyEntries);
        switch (parts.Length)
        {
            case 2:
                if (int.TryParse(parts[0], out var day) && int.TryParse(parts[1], out var month))
                {
                    var now = DateTime.Now;
                    return new DateOnly(now.Year, month, day);
                }
                break;
            case 1:
                if (int.TryParse(parts[0], out var dayOnly))
                {
                    var now = DateTime.Now;
                    return new DateOnly(now.Year, now.Month, dayOnly);
                }
                break;
        }

        return null;
    }
}