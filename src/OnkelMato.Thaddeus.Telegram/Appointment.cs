namespace OnkelMato.Thaddeus.Telegram;

public class Appointment
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Summary { get; set; } = null!;
    public string Title { get; set; } = null!;
}