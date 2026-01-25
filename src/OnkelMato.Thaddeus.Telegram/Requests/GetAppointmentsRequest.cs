namespace OnkelMato.Thaddeus.Telegram.Requests;

public class GetAppointmentsRequest : RequestBase
{
    public GetAppointmentsRequest(long chatId, string telegramUserId, DateOnly date) : base(chatId, telegramUserId)
    {
        Date = date;
    }

    public DateOnly Date { get; set; }
    public IEnumerable<Appointment> Appointments { get; set; } = [];
}