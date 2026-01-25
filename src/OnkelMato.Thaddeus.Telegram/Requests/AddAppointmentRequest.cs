namespace OnkelMato.Thaddeus.Telegram.Requests;

public class RequestBase
{
    public RequestBase(long chatId, string telegramUserId)
    {
        ChatId = chatId;
        TelegramUserId = telegramUserId;
    }
    public long ChatId { get; set; }
    public string TelegramUserId { get; set; }
}

public class EmptyRequestBase : RequestBase
{
    public EmptyRequestBase(long chatId, string telegramUserId) : base(chatId, telegramUserId)
    {
    }
}

public class AddAppointmentRequest : RequestBase
{
    public AddAppointmentRequest(long chatId, string telegramUserId, Appointment appointment) : base(chatId, telegramUserId)
    {
        Appointment = appointment;
    }

    public Appointment Appointment { get; set; }
}