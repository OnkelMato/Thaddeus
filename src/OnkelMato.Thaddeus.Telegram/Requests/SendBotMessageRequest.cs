namespace OnkelMato.Thaddeus.Telegram.Requests;

public class SendBotMessageRequest : RequestBase
{
    public SendBotMessageRequest(long chatId, string telegramUserId, string message) : base(chatId, telegramUserId)
    {
        Message = message;
    }

    public string Message { get; }
}