using System.Globalization;
using Microsoft.Extensions.Localization;
using OnkelMato.Thaddeus.Telegram.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OnkelMato.Thaddeus.Telegram;

public class BotMessageToRequestConverter : IBotMessageToRequestConverter
{
    public RequestBase Convert(Message message, UpdateType type)
    {
        if (message.Text.StartsWith("Termine ", StringComparison.InvariantCultureIgnoreCase))
        {
            var date = ParseDate(message.Text.Split(' ')[1]);
            if (date == null) // todo return result with error message
                return new EmptyRequestBase(message.Chat.Id, message.Chat.Id.ToString());

            return new GetAppointmentsRequest(message.Chat.Id, message.Chat.Id.ToString(), date.Value);
        }

        if (message.Text.StartsWith("Termin ", StringComparison.InvariantCultureIgnoreCase))
        {
            var part = message.Text.Split(' ',4, StringSplitOptions.TrimEntries);
            if (part.Length < 4) // todo return result with error message
                return new EmptyRequestBase(message.Chat.Id, message.Chat.Id.ToString());

            if (string.IsNullOrWhiteSpace(part[3])) // todo return result with error message
                return new EmptyRequestBase(message.Chat.Id, message.Chat.Id.ToString());

            var date = ParseDate(part[1]);
            if (date == null) // todo return result with error message
                return new EmptyRequestBase(message.Chat.Id, message.Chat.Id.ToString());

            var time = ParseTime(part[2]);
            if (time == null) // todo return result with error message
                return new EmptyRequestBase(message.Chat.Id, message.Chat.Id.ToString());

            return new AddAppointmentRequest(message.Chat.Id, message.Chat.Id.ToString(), new Appointment()
            {
                Start = date.Value.ToDateTime(time.Value),
                End = date.Value.ToDateTime(time.Value).AddMinutes(30),
                Title = part[3]
            });
        }

        // make some factory here. or as extension method link message.GetRequestBase();
        return new EmptyRequestBase(message.Chat.Id, message.Chat.Id.ToString());
    }

    private TimeOnly? ParseTime(string time)
    {
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