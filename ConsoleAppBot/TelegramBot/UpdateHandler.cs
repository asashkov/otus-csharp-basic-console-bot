using ConsoleBot.Core.Exceptions;
using ConsoleBot.Core.Services;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace ConsoleBot.TelegramBot;

internal class UpdateHandler(IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService) : IUpdateHandler
{
    private static readonly DateOnly _creationDate = new(2026, 9, 5);
    private const string AppVersion = "0.5.0";

    private const string StartEndpoint = "/start";
    private const string HelpEndpoint = "/help";
    private const string InfoEndpoint = "/info";
    private const string ExitEndpoint = "/exit";

    private const string AddTaskEndpoint = "/addtask";
    private const string ShowTasksEndpoint = "/showtasks";
    private const string ShowAllTasksEndpoint = "/showalltasks";
    private const string RemoveTaskEndpoint = "/removetask";
    private const string CompleteTaskEndpoint = "/completetask";
    private const string FindEndpoint = "/find";
    private const string ReportEndpoint = "/report";

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
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
                    await Start(botClient, update, userService, ct);
                    break;
                case HelpEndpoint:
                    await Help(botClient, update, userService, ct);
                    break;
                case InfoEndpoint:
                    await Info(botClient, update, ct);
                    break;
                case AddTaskEndpoint:
                    await AddTask(botClient, update, toDoService, userService, inputParams, ct);
                    break;
                case ShowTasksEndpoint:
                    await ShowTasks(botClient, update, toDoService, userService, ct);
                    break;
                case ShowAllTasksEndpoint:
                    await ShowAllTasks(botClient, update, toDoService, userService, ct);
                    break;
                case RemoveTaskEndpoint:
                    await RemoveTask(botClient, update, toDoService, inputParams, ct);
                    break;
                case CompleteTaskEndpoint:
                    await CompleteTask(botClient, update, toDoService, inputParams, ct);
                    break;
                case ReportEndpoint:
                    await Report(botClient, update, userService, toDoReportService, ct);
                    break;
                case FindEndpoint:
                    await Find(botClient, update, toDoService, userService, inputParams, ct);
                    break;
                case ExitEndpoint:
                    await botClient.SendMessage(update.Message.Chat, "Для завершения работы с ботом нажмите комбинацию клавиш Ctrl+C.", ct);
                    return;
                default:
                    await botClient.SendMessage(update.Message.Chat, "Вы ввели некорректную команду. Попробуйте еще раз.", ct);
                    break;
            }
        }
        catch (TaskCountLimitException e)
        {
            await HandleErrorAsync(botClient, e, ct);
        }
        catch (TaskLengthLimitException e)
        {
            await HandleErrorAsync(botClient, e, ct);
        }
        catch (DuplicateTaskException e)
        {
            await HandleErrorAsync(botClient, e, ct);
        }
        catch (ArgumentException e)
        {
            await HandleErrorAsync(botClient, e, ct);
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
    {
        Console.WriteLine($"HandleError: {exception.Message}");
        return Task.CompletedTask;
    }

    private static void ValidateString(string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            throw new ArgumentException("Значение должно быть не null, не пустой строкой и иметь символы кроме пробела.");
        }
    }

    private static async Task Start(ITelegramBotClient botClient, Update update, IUserService userService, CancellationToken ct)
    {
        var user = await userService.GetUser(update.Message.From.Id, ct) ??
            await userService.RegisterUser(update.Message.From.Id, update.Message.From.Username!, ct);

        await botClient.SendMessage(update.Message.Chat, $"{user.TelegramUserName}, введите команду для взаимодействия:", ct);
    }

    private static async Task Info(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        await botClient.SendMessage(update.Message.Chat, $"ConsoleBot {AppVersion}\nДата создания: {_creationDate}", ct);
    }

    private static async Task Help(ITelegramBotClient botClient, Update update, IUserService userService, CancellationToken ct)
    {
        string startEndpointText = $"{StartEndpoint}\t\t - Точка входа. Зарегистрированный пользователь имеет доступ к большему количеству команд.\n";
        string addTaskEndpointText = string.Empty;
        string showTasksEndpointText = string.Empty;
        string showAllTasksEndpointText = string.Empty;
        string removeEndpointText = string.Empty;
        string completeEndpointText = string.Empty;
        string findEndpointText = string.Empty;
        string reportEndpointText = string.Empty;
        string exitEndpointText = $"{ExitEndpoint}\t\t - Выйти из программы (Ctrl+C).";

        var user = await userService.GetUser(update.Message.From.Id, ct);

        if (user is not null)
        {
            addTaskEndpointText =
                $"{AddTaskEndpoint}\t - Добавить в список новую задачу.\n";

            showTasksEndpointText =
                $"{ShowTasksEndpoint}\t - Отобразить активные задачи из списка.\n";

            showAllTasksEndpointText =
                $"{ShowAllTasksEndpoint}\t - Отобразить все задачи из списка.\n";

            removeEndpointText =
                $"{RemoveTaskEndpoint}\t - Удалить задачу из списка по ее id.\n";

            completeEndpointText =
                $"{CompleteTaskEndpoint}\t - Завершить задачу (отметить выполненной) из списка по ее id.\n";

            findEndpointText =
                $"{FindEndpoint}\t - Найти задачу по первым символам названия.\n";

            reportEndpointText =
                $"{ReportEndpoint}\t - Сводная информация по всем задачам.\n";
        }

        await botClient.SendMessage(update.Message.Chat, "Для взаимодействия используйте следующие команды:\n" +
            startEndpointText +
            $"{HelpEndpoint}\t\t - Отобразить данную справочную информацию.\n" +
            $"{InfoEndpoint}\t\t - Показать версию и дату создания приложения.\n" +
            addTaskEndpointText +
            showTasksEndpointText +
            showAllTasksEndpointText +
            removeEndpointText +
            completeEndpointText +
            findEndpointText +
            reportEndpointText +
            exitEndpointText, ct);
    }

    private static async Task AddTask(
        ITelegramBotClient botClient, Update update, IToDoService toDoService,
        IUserService userService, string[] inputParams, CancellationToken ct
        )
    {
        var newTask = string.Join(" ", inputParams);

        ValidateString(newTask);

        var user = await userService.GetUser(update.Message.From.Id, ct);

        await toDoService.Add(user!, newTask, ct);
        await botClient.SendMessage(update.Message.Chat, $"Задача \"{newTask}\" добавлена.", ct);
    }

    private static async Task ShowTasks(ITelegramBotClient botClient, Update update, IToDoService toDoService, IUserService userService, CancellationToken ct)
    {
        var user = await userService.GetUser(update.Message.From.Id, ct);
        var activeTasks = await toDoService.GetActiveByUserId(user!.UserId, ct);

        if (activeTasks.Count == 0)
            await botClient.SendMessage(update.Message.Chat, "Список задач пуст.", ct);

        for (int i = 0; i < activeTasks.Count; i++)
        {
            await botClient.SendMessage(update.Message.Chat, $"{i + 1}. {activeTasks[i]}", ct);
        }
    }

    private static async Task ShowAllTasks(ITelegramBotClient botClient, Update update, IToDoService toDoService, IUserService userService, CancellationToken ct)
    {
        var user = await userService.GetUser(update.Message.From.Id, ct);
        var tasks = await toDoService.GetAllByUserId(user!.UserId, ct);

        if (tasks.Count == 0)
        {
            await botClient.SendMessage(update.Message.Chat, "Список задач пуст.", ct);
            return;
        }

        for (int i = 0; i < tasks.Count; i++)
        {
            await botClient.SendMessage(update.Message.Chat, $"{i + 1}. {tasks[i]}", ct);
        }
    }

    private static async Task RemoveTask(ITelegramBotClient botClient, Update update, IToDoService toDoService, string[] inputParams, CancellationToken ct)
    {
        if (inputParams.Length != 1)
        {
            await botClient.SendMessage(update.Message.Chat, "Введите корректное значение id задачи.", ct);
            return;
        }

        var taskId = inputParams[0];

        ValidateString(taskId);

        if (Guid.TryParse(taskId, out Guid result))
        {
            await toDoService.Delete(result, ct);
            await botClient.SendMessage(update.Message.Chat, $"Задача с id \"{taskId}\" удалена.", ct);
        }
        else
        {
            await botClient.SendMessage(update.Message.Chat, $"Вы ввели некорректный id задачи: {taskId}", ct);
        }
    }

    private static async Task CompleteTask(ITelegramBotClient botClient, Update update, IToDoService toDoService, string[] inputParams, CancellationToken ct)
    {
        if (inputParams.Length != 1)
        {
            await botClient.SendMessage(update.Message.Chat, "Введите корректное значение id задачи.", ct);
            return;
        }

        var taskId = inputParams[0];

        ValidateString(taskId);

        if (Guid.TryParse(taskId, out Guid result))
        {
            await toDoService.MarkCompleted(result, ct);
            await botClient.SendMessage(update.Message.Chat, $"Задача с id \"{taskId}\" выполнена.", ct);
        }
        else
        {
            await botClient.SendMessage(update.Message.Chat, $"Задача с id \"{taskId}\" не найдена.", ct);
        }
    }

    private static async Task Report(ITelegramBotClient botClient, Update update, IUserService userService, IToDoReportService toDoReportService, CancellationToken ct)
    {
        var user = await userService.GetUser(update.Message.From.Id, ct);

        if (user is not null)
        {
            var (total, completed, active, generatedAt) = await toDoReportService.GetUserStats(user.UserId, ct);

            await botClient.SendMessage(update.Message.Chat, $"Статистика по задачам на {generatedAt}. " +
                $"Всего: {total}; Завершенных: {completed}; Активных: {active}.", ct);
        }
    }

    private static async Task Find(
        ITelegramBotClient botClient, Update update, IToDoService toDoService,
        IUserService userService, string[] inputParams, CancellationToken ct
        )
    {
        var expectedParamsCount = 1;

        if (inputParams.Length != expectedParamsCount)
        {
            await botClient.SendMessage(update.Message.Chat, $"Вы превысили допустимое количество параметров для данной команды: {expectedParamsCount}.", ct);
            return;
        }

        var user = await userService.GetUser(update.Message.From.Id, ct);

        if (user is not null)
        {
            var tasks = await toDoService.Find(user, inputParams[0], ct);

            if (tasks.Count == 0)
            {
                await botClient.SendMessage(update.Message.Chat, "Список задач пуст.", ct);
                return;
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                await botClient.SendMessage(update.Message.Chat, $"{i + 1}. {tasks[i]}", ct);
            }
        }
    }
}
