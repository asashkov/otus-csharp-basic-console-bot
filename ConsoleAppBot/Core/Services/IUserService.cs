using ConsoleBot.Core.Entities;

namespace ConsoleBot.Core.Services;

internal interface IUserService
{
    ToDoUser RegisterUser(long telegramUserId, string telegramUserName);
    ToDoUser? GetUser(long telegramUserId);
}
