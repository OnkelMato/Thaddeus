using OnkelMato.Thaddeus.Telegram.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OnkelMato.Thaddeus.Telegram;

internal interface IBotMessageToRequestConverter
{
    RequestBase Convert(Message message, UpdateType type);
}