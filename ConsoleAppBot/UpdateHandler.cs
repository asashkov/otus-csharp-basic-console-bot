using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace ConsoleBot;

internal class UpdateHandler(IUserService userService, IToDoService toDoService) : IUpdateHandler
{
    private static readonly DateOnly _creationDate = new(2026, 9, 5);
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

    public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
    {
        try
        {
            var input = update.Message.Text;
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
                    Start(update, userService);
                    break;
                case HelpEndpoint:
                    Help(botClient, update, userService);
                    break;
                case InfoEndpoint:
                    Info(botClient, update);
                    break;
                case AddTaskEndpoint:
                    AddTask(botClient, update, toDoService, userService, inputParams);
                    break;
                case ShowTasksEndpoint:
                    ShowTasks(botClient, update, toDoService, userService);
                    break;
                case ShowAllTasksEndpoint:
                    ShowAllTasks(botClient, update, toDoService, userService);
                    break;
                case RemoveTaskEndpoint:
                    RemoveTask(botClient, update, toDoService, userService, inputParams);
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
        catch (TaskCountLimitException e)
        {
            botClient.SendMessage(update.Message.Chat, e.Message);
        }
        catch (TaskLengthLimitException e)
        {
            botClient.SendMessage(update.Message.Chat, e.Message);
        }
        catch (DuplicateTaskException e)
        {
            botClient.SendMessage(update.Message.Chat, e.Message);
        }
        catch (ArgumentException e)
        {
            botClient.SendMessage(update.Message.Chat, e.Message);
        }
    }

    private static void ValidateString(string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            throw new ArgumentException("Значение должно быть не null, не пустой строкой и иметь символы кроме пробела.");
        }
    }

    private static void Start(Update update, IUserService userService)
    {
        if (userService.GetUser(update.Message.From.Id) is null)
            userService.RegisterUser(update.Message.From.Id, update.Message.From.Username!);
    }

    private static void Info(ITelegramBotClient botClient, Update update)
    {
        botClient.SendMessage(update.Message.Chat, $"ConsoleBot {AppVersion}\nДата создания: {_creationDate}");
    }

    private static void Help(ITelegramBotClient botClient, Update update, IUserService userService)
    {
        string startEndpointText = $"{StartEndpoint}\t\t - Точка входа. Зарегистрированный пользователь имеет доступ к большему количеству команд.\n";
        string addTaskEndpointText = string.Empty;
        string showTasksEndpointText = string.Empty;
        string showAllTasksEndpointText = string.Empty;
        string removeEndpointText = string.Empty;
        string completeEndpointText = string.Empty;
        string exitEndpointText = $"{ExitEndpoint}\t\t - Выйти из программы.";

        var user = userService.GetUser(update.Message.From.Id);

        if (user is not null)
        {
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

    private static void AddTask(ITelegramBotClient botClient, Update update, IToDoService toDoService, IUserService userService, string[] inputParams)
    {
        var newTask = string.Join(" ", inputParams);

        ValidateString(newTask);

        var user = userService.GetUser(update.Message.From.Id);

        toDoService.Add(user!, newTask);
        botClient.SendMessage(update.Message.Chat, $"Задача \"{newTask}\" добавлена.");
    }

    private static bool ShowTasks(ITelegramBotClient botClient, Update update, IToDoService toDoService, IUserService userService)
    {
        var user = userService.GetUser(update.Message.From.Id);
        var activeTasks = toDoService.GetActiveByUserId(user!.UserId);

        if (activeTasks.Count == 0)
        {
            botClient.SendMessage(update.Message.Chat, "Список задач пуст.");
            return false;
        }

        for (int i = 0; i < activeTasks.Count; i++)
        {
            botClient.SendMessage(update.Message.Chat, $"{i + 1}. {activeTasks[i]}");
        }

        return true;
    }

    private static void ShowAllTasks(ITelegramBotClient botClient, Update update, IToDoService toDoService, IUserService userService)
    {
        var user = userService.GetUser(update.Message.From.Id);
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

    private static void RemoveTask(ITelegramBotClient botClient, Update update, IToDoService toDoService, IUserService userService, string[] inputParams)
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
            toDoService.Delete(result);
            botClient.SendMessage(update.Message.Chat, $"Задача с id \"{taskId}\" удалена.");
        }
        else
        {
            botClient.SendMessage(update.Message.Chat, $"Вы ввели некорректный id задачи: {taskId}");
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
            botClient.SendMessage(update.Message.Chat, $"Задача с id \"{taskId}\" выполнена.");
        }
        else
        {
            botClient.SendMessage(update.Message.Chat, $"Задача с id \"{taskId}\" не найдена.");
        }
    }
}
