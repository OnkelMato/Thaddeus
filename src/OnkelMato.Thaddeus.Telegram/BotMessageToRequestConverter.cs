using OnkelMato.Thaddeus.Telegram.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OnkelMato.Thaddeus.Telegram;

internal class BotMessageToRequestConverter : IBotMessageToRequestConverter
{
    public RequestBase Convert(Message message, UpdateType type)
    {
        if (message.Text.StartsWith("Termine ", StringComparison.InvariantCultureIgnoreCase))
        {
            var now = DateTime.Now;
            var part = message.Text.Split(' ')[1];
            return new GetAppointmentsRequest(message.Chat.Id, message.Chat.Id.ToString(), DateOnly.ParseExact(part, ["d.M.yyyy"]));
        }

        if (message.Text.StartsWith("Termin ", StringComparison.InvariantCultureIgnoreCase))
        {
            var now = DateTime.Now;
            var part = message.Text.Split(' ',4, StringSplitOptions.TrimEntries);
            var date = DateOnly.ParseExact(part[1], ["d.M.yyyy"]);
            var time = TimeOnly.ParseExact(part[2], ["h:mm", "h"]);

            return new AddAppointmentRequest(message.Chat.Id, message.Chat.Id.ToString(), new Appointment()
            {
                Start = date.ToDateTime(time),
                End = date.ToDateTime(time).AddMinutes(30),
                Title = part[3]
            });
        }

        // make some factory here. or as extension method link message.GetRequestBase();
        return new EmptyRequestBase(message.Chat.Id, message.Chat.Id.ToString());
    }
}