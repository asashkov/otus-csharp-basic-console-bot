using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace ConsoleBot;

internal class UpdateHandler(IUserService userService, IToDoService toDoService) : IUpdateHandler
{
    private static DateOnly _creationDate = new(2026, 9, 5);
    private const string AppVersion = "0.3.0";

    private const string StartEndpoint = "/start";
    private const string HelpEndpoint = "/help";
    private const string InfoEndpoint = "/info";
    private const string ExitEndpoint = "/exit";

    private const string AddTaskEndpoint = "/addtask";
    private const string ShowTasksEndpoint = "/showtasks";
    private const string ShowAllTasksEndpoint = "/showalltasks";
    private const string RemoveTaskEndpoint = "/removetask";
    private const string CompleteTaskEndpoint = "/completetask";
    /*
    private const int MinTasksCount = 1;
    private const int MinTaskLength = 1;

    private static int _maxTasksCount = 100;
    private static int _maxTaskLength = 100;

    private static bool _initialized = false;
    */
    private static ToDoUser? user = null;

    public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
    {
        botClient.SendMessage(update.Message.Chat, "Привет!\n" +
            $"Для взаимодействия доступны команды: {HelpEndpoint}, {InfoEndpoint}");

        try
        {
            DataEntryPrompt(botClient, update);

            var input = Console.ReadLine();
            string[] inputParams = [];

            if (!string.IsNullOrWhiteSpace(input))
            {
                var inputWithParams = input.Trim().Split(' ');
                input = inputWithParams[0];
                inputParams = [.. inputWithParams.Skip(1)];
            }

            switch (input)
            {
                case StartEndpoint:
                    Start(botClient, update, userService);
                    break;
                case HelpEndpoint:
                    Help(botClient, update);
                    break;
                case InfoEndpoint:
                    Info(botClient, update);
                    break;
                case AddTaskEndpoint:
                    AddTask(botClient, update, toDoService, inputParams);
                    break;
                case ShowTasksEndpoint:
                    ShowTasks(botClient, update, toDoService);
                    break;
                case ShowAllTasksEndpoint:
                    ShowAllTasks(botClient, update, toDoService);
                    break;
                case RemoveTaskEndpoint:
                    RemoveTask(botClient, update, toDoService, inputParams);
                    break;
                case CompleteTaskEndpoint:
                    CompleteTask(botClient, update, toDoService, inputParams);
                    break;
                case ExitEndpoint:
                    return;
                default:
                    botClient.SendMessage(update.Message.Chat, "Вы ввели некорректную команду. Попробуйте еще раз.");
                    break;
            }
        }
        catch (Exception e)
        {
            botClient.SendMessage(update.Message.Chat, $"Произошла непредвиденная ошибка:\n" +
                $"Type: {e.GetType}\n" +
                $"Message: {e.Message}\n" +
                $"Stacktrace: {e.StackTrace}\n" +
                $"InnerException: {e?.InnerException?.Message}");
        }
    }

    private static void ValidateString(string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            throw new ArgumentException("Значение должно быть не null, не пустой строкой и иметь символы кроме пробела.");
        }
    }
    /*
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
    */
    private static void DataEntryPrompt(ITelegramBotClient botClient, Update update)
    {
        string greating = user is null ? "Пожалуйста," : $"{user.TelegramUserName}, пожалуйста,";

        botClient.SendMessage(update.Message.Chat, $"{greating} введите команду: ");
    }

    private static void Start(ITelegramBotClient botClient, Update update, IUserService userService)
    {
        user = userService.GetUser(update.Message.From.Id);

        if (user is null)
            userService.RegisterUser(update.Message.From.Id, update.Message.From.Username!);
        /*
        if (!_initialized)
        {
            botClient.SendMessage(update.Message.Chat, "Введите максимально допустимое количество задач: ");

            _maxTasksCount = ParseAndValidateInt(Console.ReadLine()!, MinTasksCount, _maxTasksCount);

            botClient.SendMessage(update.Message.Chat, "Введите максимально допустимую длину задачи: ");

            _maxTaskLength = ParseAndValidateInt(Console.ReadLine()!, MinTaskLength, _maxTaskLength);

            _initialized = true;
        }*/
    }

    private static void Info(ITelegramBotClient botClient, Update update)
    {
        botClient.SendMessage(update.Message.Chat, $"ConsoleBot {AppVersion}\nДата создания: {_creationDate}");
    }

    private static void Help(ITelegramBotClient botClient, Update update)
    {
        string startEndpointText = string.Empty;
        string addTaskEndpointText = string.Empty;
        string showTasksEndpointText = string.Empty;
        string showAllTasksEndpointText = string.Empty;
        string removeEndpointText = string.Empty;
        string completeEndpointText = string.Empty;
        string exitEndpointText = string.Empty;

        if (user is not null)
        {
            startEndpointText =
                $"{StartEndpoint}\t\t - Точка входа. Введите свое имя для доступа к большему количеству команд.\n";

            addTaskEndpointText =
                $"{AddTaskEndpoint}\t - Добавить в список новую задачу.\n";

            showTasksEndpointText =
                $"{ShowTasksEndpoint}\t - Отобразить активные задачи из списка.\n";

            showAllTasksEndpointText =
                $"{ShowAllTasksEndpoint}\t - Отобразить все задачи из списка.\n";

            removeEndpointText =
                $"{RemoveTaskEndpoint}\t - Удалить задачу из списка по ее номеру.\n";

            completeEndpointText =
                $"{CompleteTaskEndpoint}\t - Завершить задачу (отметить выполненной) из списка по ее id.\n";

            exitEndpointText = $"{ExitEndpoint}\t\t - Выйти из программы.";
        }

        botClient.SendMessage(update.Message.Chat, "Для взаимодействия используйте следующие команды:\n" +
            startEndpointText +
            $"{HelpEndpoint}\t\t - Отобразить данную справочную информацию.\n" +
            $"{InfoEndpoint}\t\t - Показать версию и дату создания приложения.\n" +
            addTaskEndpointText +
            showTasksEndpointText +
            showAllTasksEndpointText +
            removeEndpointText +
            completeEndpointText +
            exitEndpointText);
    }

    private static void AddTask(ITelegramBotClient botClient, Update update, IToDoService toDoService, string[] inputParams)
    {
        var newTask = string.Join(" ", inputParams);

        ValidateString(newTask);

        toDoService.Add(user!, newTask);
        botClient.SendMessage(update.Message.Chat, $"Задача \"{newTask}\" добавлена.");
    }

    private static bool ShowTasks(ITelegramBotClient botClient, Update update, IToDoService toDoService)
    {
        var activeTasks = toDoService.GetActiveByUserId(user!.UserId);
        var hasActiveTasks = false;

        if (activeTasks.Count == 0)
        {
            botClient.SendMessage(update.Message.Chat, "Список задач пуст.");
            return hasActiveTasks;
        }

        hasActiveTasks = true;

        for (int i = 0; i < activeTasks.Count; i++)
        {
            botClient.SendMessage(update.Message.Chat, $"{i + 1}. {activeTasks[i]}");
        }

        return hasActiveTasks;
    }

    private static void ShowAllTasks(ITelegramBotClient botClient, Update update, IToDoService toDoService)
    {
        var tasks = toDoService.GetAllByUserId(user!.UserId);

        if (tasks.Count == 0)
        {
            botClient.SendMessage(update.Message.Chat, "Список задач пуст.");
            return;
        }

        for (int i = 0; i < tasks.Count; i++)
        {
            botClient.SendMessage(update.Message.Chat, $"{i + 1}. {tasks[i]}");
        }
    }

    private static void RemoveTask(
        ITelegramBotClient botClient,
        Update update,
        IToDoService toDoService,
        string[] inputParams
        )
    {
        if (inputParams.Length != 1)
        {
            botClient.SendMessage(update.Message.Chat, "Введите корректное значение id задачи.");
            return;
        }

        if (!ShowTasks(botClient, update, toDoService))
        {
            return;
        }

        var taskId = inputParams[0];

        ValidateString(taskId);

        if (Guid.TryParse(taskId, out Guid result))
        {
            toDoService.Delete(result);
            botClient.SendMessage(update.Message.Chat, $"Задача с id \"{taskId}\" удалена.");
        }
        else
        {
            botClient.SendMessage(update.Message.Chat, $"Задача с id \"{taskId}\" не найдена.");
        }
    }

    private static void CompleteTask(ITelegramBotClient botClient, Update update, IToDoService toDoService, string[] inputParams)
    {
        if (inputParams.Length != 1)
        {
            botClient.SendMessage(update.Message.Chat, "Введите корректное значение id задачи.");
            return;
        }

        var taskId = inputParams[0];

        ValidateString(taskId);

        if (Guid.TryParse(taskId, out Guid result))
        {
            toDoService.MarkCompleted(result);
            botClient.SendMessage(update.Message.Chat, $"Задача с id \"{taskId}\" завершена.");
        }
        else
        {
            botClient.SendMessage(update.Message.Chat, $"Задача с id \"{taskId}\" не найдена.");
        }
    }
}
