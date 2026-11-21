using ConsoleBot.Core.Entities;
using ConsoleBot.Core.Exceptions;
using ConsoleBot.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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

    private static readonly string[] _availableCommands =
    {
        StartEndpoint,
        ShowAllTasksEndpoint,
        ShowTasksEndpoint,
        ReportEndpoint
    };

    private static readonly string[] _commandsDescriptions =
    {
        "Запустить бота",
        "Показать все задачи",
        "Показать активные задачи",
        "Сгенерировать отчет"
    };

    private static ToDoUser? _user = null;

    private static TaskCompletionSource? tcs;

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        tcs = new();

        await botClient.SetMyCommands(GetMenuCommands(), cancellationToken: ct);

        ReplyKeyboardMarkup keyboard = new([GetKeyboardButtons()]) { };

        try
        {
            var input = update.Message!.Text;
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
                    await Start(botClient, update, userService, keyboard, ct);
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
                    await ShowTasks(botClient, update, toDoService, userService, keyboard, ct);
                    break;
                case ShowAllTasksEndpoint:
                    await ShowAllTasks(botClient, update, toDoService, userService, keyboard, ct);
                    break;
                case RemoveTaskEndpoint:
                    await RemoveTask(botClient, update, toDoService, inputParams, ct);
                    break;
                case CompleteTaskEndpoint:
                    await CompleteTask(botClient, update, toDoService, inputParams, ct);
                    break;
                case ReportEndpoint:
                    await Report(botClient, update, userService, toDoReportService, keyboard, ct);
                    break;
                case FindEndpoint:
                    await Find(botClient, update, toDoService, userService, inputParams, ct);
                    break;
                case ExitEndpoint:
                    await botClient.DeleteMyCommands(cancellationToken: ct);
                    await botClient.SendMessage(update.Message?.Chat, "Завершение работы бота...", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: ct);
                    await tcs.Task;
                    tcs = null;

                    return;
                default:
                    await botClient.SendMessage(update.Message?.Chat, "Вы ввели некорректную команду. Для просмотра доступных команд введите /help", cancellationToken: ct);
                    break;
            }
        }
        catch (TaskCountLimitException e)
        {
            await HandleErrorAsync(botClient, e, HandleErrorSource.HandleUpdateError, ct);
        }
        catch (TaskLengthLimitException e)
        {
            await HandleErrorAsync(botClient, e, HandleErrorSource.HandleUpdateError, ct);
        }
        catch (DuplicateTaskException e)
        {
            await HandleErrorAsync(botClient, e, HandleErrorSource.HandleUpdateError, ct);
        }
        catch (ArgumentException e)
        {
            await HandleErrorAsync(botClient, e, HandleErrorSource.HandleUpdateError, ct);
        }
    }
    
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        Console.WriteLine($"HandleError: {exception.Message}");
        await Task.CompletedTask;
    }
    
    private static void ValidateString(string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            throw new ArgumentException("Значение должно быть не null, не пустой строкой и иметь символы кроме пробела.");
        }
    }

    private static async Task Start(ITelegramBotClient botClient, Update update, IUserService userService, ReplyMarkup replyMarkup, CancellationToken ct)
    {
        _user = await userService.GetUser(update.Message!.From!.Id, ct) ??
            await userService.RegisterUser(update.Message!.From!.Id, update.Message.From.Username!, ct);

        await botClient.SendMessage(
            update.Message?.Chat,
            $"{_user.TelegramUserName}, введите команду для взаимодействия: ",
            replyMarkup: replyMarkup,
            cancellationToken: ct
        );
    }

    private static async Task Info(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        await botClient.SendMessage(update.Message?.Chat, $"ConsoleBot {AppVersion}\nДата создания: {_creationDate}", cancellationToken: ct);
    }

    private static async Task Help(ITelegramBotClient botClient, Update update, IUserService userService, CancellationToken ct)
    {
        string startEndpointText = $"{StartEndpoint}\t\t - Начало работы с ботом.\n";
        string addTaskEndpointText = string.Empty;
        string showTasksEndpointText = string.Empty;
        string showAllTasksEndpointText = string.Empty;
        string removeEndpointText = string.Empty;
        string completeEndpointText = string.Empty;
        string findEndpointText = string.Empty;
        string reportEndpointText = string.Empty;
        string exitEndpointText = $"{ExitEndpoint}\t\t - Выйти из программы (Ctrl+C).";

        var user = await userService.GetUser(update.Message!.From!.Id, ct);

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

        await botClient.SendMessage(update.Message?.Chat, "Для взаимодействия используйте следующие команды:\n" +
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
            exitEndpointText, cancellationToken: ct);
    }

    private static async Task AddTask(
        ITelegramBotClient botClient, Update update, IToDoService toDoService,
        IUserService userService, string[] inputParams, CancellationToken ct
        )
    {
        var newTask = string.Join(" ", inputParams);

        ValidateString(newTask);

        var user = await userService.GetUser(update.Message!.From!.Id, ct);

        await toDoService.Add(user!, newTask, ct);
        await botClient.SendMessage(update.Message?.Chat, $"Задача \"{newTask}\" добавлена.", cancellationToken: ct);
    }

    private static async Task ShowTasks(ITelegramBotClient botClient, Update update, IToDoService toDoService, IUserService userService, ReplyMarkup replyMarkup, CancellationToken ct)
    {
        var user = await userService.GetUser(update.Message!.From!.Id, ct);
        var activeTasks = await toDoService.GetActiveByUserId(user!.UserId, ct);

        if (activeTasks.Count == 0)
            await botClient.SendMessage(update.Message?.Chat, "Список задач пуст.", replyMarkup: replyMarkup, cancellationToken: ct);

        for (int i = 0; i < activeTasks.Count; i++)
        {
            await botClient.SendMessage(update.Message?.Chat, $"{i + 1}. {activeTasks[i]}", replyMarkup: replyMarkup, cancellationToken: ct);
        }
    }

    private static async Task ShowAllTasks(ITelegramBotClient botClient, Update update, IToDoService toDoService, IUserService userService, ReplyMarkup replyMarkup, CancellationToken ct)
    {
        var user = await userService.GetUser(update.Message!.From!.Id, ct);
        var tasks = await toDoService.GetAllByUserId(user!.UserId, ct);

        if (tasks.Count == 0)
        {
            await botClient.SendMessage(update.Message?.Chat, "Список задач пуст.", replyMarkup: replyMarkup, cancellationToken: ct);
            return;
        }

        for (int i = 0; i < tasks.Count; i++)
        {
            await botClient.SendMessage(update.Message?.Chat, $"{i + 1}. {tasks[i]}", replyMarkup: replyMarkup, cancellationToken: ct);
        }
    }

    private static async Task RemoveTask(ITelegramBotClient botClient, Update update, IToDoService toDoService, string[] inputParams, CancellationToken ct)
    {
        if (inputParams.Length != 1)
        {
            await botClient.SendMessage(update.Message?.Chat, "Введите корректное значение id задачи.", cancellationToken: ct);
            return;
        }

        var taskId = inputParams[0];

        ValidateString(taskId);

        if (Guid.TryParse(taskId, out Guid result))
        {
            await toDoService.Delete(result, ct);
            await botClient.SendMessage(update.Message?.Chat, $"Задача с id \"{taskId}\" удалена.", cancellationToken: ct);
        }
        else
        {
            await botClient.SendMessage(update.Message?.Chat, $"Вы ввели некорректный id задачи: {taskId}", cancellationToken: ct);
        }
    }

    private static async Task CompleteTask(ITelegramBotClient botClient, Update update, IToDoService toDoService, string[] inputParams, CancellationToken ct)
    {
        if (inputParams.Length != 1)
        {
            await botClient.SendMessage(update.Message?.Chat, "Введите корректное значение id задачи.", cancellationToken: ct);
            return;
        }

        var taskId = inputParams[0];

        ValidateString(taskId);

        if (Guid.TryParse(taskId, out Guid result))
        {
            await toDoService.MarkCompleted(result, ct);
            await botClient.SendMessage(update.Message?.Chat, $"Задача с id \"{taskId}\" выполнена.", cancellationToken: ct);
        }
        else
        {
            await botClient.SendMessage(update.Message?.Chat, $"Задача с id \"{taskId}\" не найдена.", cancellationToken: ct);
        }
    }

    private static async Task Report(ITelegramBotClient botClient, Update update, IUserService userService, IToDoReportService toDoReportService, ReplyMarkup replyMarkup, CancellationToken ct)
    {
        var user = await userService.GetUser(update.Message!.From!.Id, ct);

        if (user is not null)
        {
            var (total, completed, active, generatedAt) = await toDoReportService.GetUserStats(user.UserId, ct);

            await botClient.SendMessage(update.Message?.Chat, $"Статистика по задачам на {generatedAt}. " +
                $"Всего: {total}; Завершенных: {completed}; Активных: {active}.", replyMarkup: replyMarkup, cancellationToken: ct);
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
            await botClient.SendMessage(update.Message?.Chat,
                $"Вы превысили допустимое количество параметров для данной команды: {expectedParamsCount}.",
                cancellationToken: ct);
            return;
        }

        var user = await userService.GetUser(update.Message!.From!.Id, ct);

        if (user is not null)
        {
            var tasks = await toDoService.Find(user, inputParams[0], ct);

            if (tasks.Count == 0)
            {
                await botClient.SendMessage(update.Message?.Chat, "Список задач пуст.", cancellationToken: ct);
                return;
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                await botClient.SendMessage(update.Message?.Chat, $"{i + 1}. {tasks[i]}", cancellationToken: ct);
            }
        }
    }

    private static KeyboardButton[] GetKeyboardButtons()
    {
        var buttons = new List<KeyboardButton>();

        if (_user is null)
        {
            buttons.Add(new KeyboardButton(_availableCommands[0]));
        }
        else
        {
            for (int i = 1; i < _availableCommands.Length; i++)
            {
                buttons.Add(new KeyboardButton(_availableCommands[i]));
            }
        }
        return buttons.ToArray();
    }

    private static BotCommand[] GetMenuCommands()
    {
        var buttons = new List<BotCommand>();
        
        if (_user is null)
        {
            buttons.Add(new BotCommand { Command = _availableCommands[0].Substring(1), Description = _commandsDescriptions[0] });
        }
        else
        {
            for (int i = 1; i < _availableCommands.Length; i++)
            {
                buttons.Add(new BotCommand { Command = _availableCommands[i].Substring(1), Description = _commandsDescriptions[i] });
            }
        }
        
        return buttons.ToArray();
    }
}
