using Otus.ToDoList.ConsoleBot;

namespace ConsoleBot;

public class Program
{
    static void Main()
    {
        var userService = new UserService();
        var toDoService = new ToDoService();
        var handler = new UpdateHandler(userService, toDoService);
        var botClient = new ConsoleBotClient();

        botClient.StartReceiving(handler);
    }
}