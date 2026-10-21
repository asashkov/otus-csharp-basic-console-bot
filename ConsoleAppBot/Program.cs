using Otus.ToDoList.ConsoleBot;

namespace ConsoleBot;

public class Program
{
    private const int MinTasksCount = 1;
    private const int MinTaskLength = 1;

    private static int _maxTasksCount = 8;
    private static int _maxTaskLength = 128;

    private static bool _initialized = false;

    static void Main()
    {
        try
        {
            if (!_initialized)
            {
                Console.WriteLine("Введите максимально допустимое количество задач: ");

                _maxTasksCount = ParseAndValidateInt(Console.ReadLine()!, MinTasksCount, _maxTasksCount);

                Console.WriteLine("Введите максимально допустимую длину задачи: ");

                _maxTaskLength = ParseAndValidateInt(Console.ReadLine()!, MinTaskLength, _maxTaskLength);

                _initialized = true;
            }

            Console.WriteLine("Для взаимодействия доступны команды: /help, /info");

            var userService = new UserService();
            var toDoService = new ToDoService(_maxTasksCount, _maxTaskLength);
            var handler = new UpdateHandler(userService, toDoService);
            var botClient = new ConsoleBotClient();

            botClient.StartReceiving(handler);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Произошла непредвиденная ошибка:\n" +
                $"Type: {e.GetType}\n" +
                $"Message: {e.Message}\n" +
                $"Stacktrace: {e.StackTrace}\n" +
                $"InnerException: {e?.InnerException?.Message}");
        }
    }

    private static int ParseAndValidateInt(string? str, int min, int max)
    {
        if (int.TryParse(str!, out int parsedValue))
        {
            if (parsedValue >= min && parsedValue <= max)
            {
                return parsedValue;
            }
        }

        throw new ArgumentException($"Допустимое значение от {min} до {max}.");
    }
}