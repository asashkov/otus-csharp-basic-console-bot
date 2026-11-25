using ConsoleBot.Core.Services;
using ConsoleBot.Infrastructure.DataAccess;
using ConsoleBot.TelegramBot;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace ConsoleBot;

public class Program
{
    private const int MinTasksCount = 1;
    private const int MinTaskLength = 1;

    private static int _maxTasksCount = 8;
    private static int _maxTaskLength = 128;

    private static bool _initialized = false;

    static async Task Main()
    {
        try
        {
            string token = Environment.GetEnvironmentVariable("OTUS_TG_BOT_TOKEN", EnvironmentVariableTarget.User)!;
            ITelegramBotClient botClient = new TelegramBotClient(token);

            if (!_initialized)
            {
                Console.WriteLine("Введите максимально допустимое количество задач: ");

                _maxTasksCount = ParseAndValidateInt(Console.ReadLine()!, MinTasksCount, _maxTasksCount);

                Console.WriteLine("Введите максимально допустимую длину задачи: ");

                _maxTaskLength = ParseAndValidateInt(Console.ReadLine()!, MinTaskLength, _maxTaskLength);

                _initialized = true;
            }

            using var cts = new CancellationTokenSource();
            var userRepository = new InMemoryUserRepository();
            var userService = new UserService(userRepository);
            var toDoRepository = new InMemoryToDoRepository();
            var toDoService = new ToDoService(toDoRepository, _maxTasksCount, _maxTaskLength);
            var toDoReportService = new ToDoReportService(toDoRepository);
            var updateHandler = new UpdateHandler(userService, toDoService, toDoReportService);
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message],
                DropPendingUpdates = true
            };

            botClient.StartReceiving(updateHandler, receiverOptions, cts.Token);

            Console.WriteLine();
            Console.WriteLine("Нажмите 'A' для выхода");
            
            if (Console.ReadKey().Key.Equals(ConsoleKey.A))
            {
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    cts.Cancel();
                };

                Console.WriteLine();
                Console.WriteLine("Завершение работы бота...");
                return;
            }
            else
            {
                var me = await botClient.GetMe();

                Console.WriteLine();
                Console.WriteLine($"Бот запущен: @{me.Username}");
                Console.WriteLine("Для взаимодействия доступны команды: /help, /info");
            }            

            await Task.Delay(Timeout.Infinite, cts.Token);
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