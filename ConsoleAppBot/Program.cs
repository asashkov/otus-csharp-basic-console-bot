using System.Diagnostics.Metrics;

namespace ConsoleBot;

public class Program
{
    private static readonly DateOnly _creationDate = new(2026, 9, 5);
    private const string AppVersion = "0.2.0";

    private const string StartEndpoint = "/start";
    private const string HelpEndpoint = "/help";
    private const string InfoEndpoint = "/info";
    private const string EchoEndpoint = "/echo";
    
    private const string AddTaskEndpoint = "/addtask";
    private const string ShowTasksEndpoint = "/showtasks";
    private const string ShowAllTasksEndpoint = "/showalltasks";
    private const string RemoveTaskEndpoint = "/removetask";
    private const string CompleteTaskEndpoint = "/completetask";

    private const string ExitEndpoint = "/exit";

    private const int MinTasksCount = 1;
    private const int MinTaskLength = 1;
    
    private static int _maxTasksCount = 100;
    private static int _maxTaskLength = 100;

    private static List<ToDoItem> _tasks = [];

    private static bool _initialized = false;

    static void Main()
    {
        Console.WriteLine($"Привет!\n" +
            $"Для взаимодействия доступны следующие команды: " +
            $"{StartEndpoint}, {HelpEndpoint}, {InfoEndpoint}, {ExitEndpoint}");

        ToDoUser? user = null;

        while (true)
        {
            try
            {
                if (!_initialized)
                {
                    Console.WriteLine();
                    Console.Write("Введите максимально допустимое количество задач: ");

                    _maxTasksCount = ParseAndValidateInt(Console.ReadLine()!, MinTasksCount, _maxTasksCount);

                    Console.Write("Введите максимально допустимую длину задачи: ");

                    _maxTaskLength = ParseAndValidateInt(Console.ReadLine()!, MinTaskLength, _maxTaskLength);

                    _tasks = new List<ToDoItem>(_maxTasksCount);
                    _initialized = true;
                }

                DataEntryPrompt(user!);

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
                        Start(ref user!);
                        break;
                    case HelpEndpoint:
                        Help(user!);
                        break;
                    case InfoEndpoint:
                        Info();
                        break;
                    case EchoEndpoint:
                        Echo(user!, inputParams);
                        break;
                    case AddTaskEndpoint:
                        AddTask(user);
                        break;
                    case ShowTasksEndpoint:
                        ShowTasks();
                        break;
                    case ShowAllTasksEndpoint:
                        ShowAllTasks();
                        break;
                    case RemoveTaskEndpoint:
                        RemoveTask();
                        break;
                    case CompleteTaskEndpoint:
                        CompleteTask(inputParams);
                        break;
                    case ExitEndpoint:
                        return;
                    default:
                        Console.WriteLine("Вы ввели некорректную команду. Попробуйте еще раз.");
                        break;
                }
            }
            catch (TaskCountLimitException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (TaskLengthLimitException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (DuplicateTaskException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
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

    private static void ValidateString(string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            throw new ArgumentException("Значение должно быть не null, не пустой строкой и иметь символы кроме пробела.");
        }
    }

    private static void DataEntryPrompt(ToDoUser user)
    {
        string greating = user is null ? "Пожалуйста," : $"{user.TelegramUserName}, пожалуйста,";
        
        Console.WriteLine();
        Console.Write($"{greating} введите команду: ");
    }

    private static void Start(ref ToDoUser user)
    {
        if (user is not null)
        {
            Console.WriteLine($"Вы уже ввели имя {user.TelegramUserName}. Используйте другую команду.");
        }
        else
        {
            Console.Write("Введите Ваше имя: ");

            var name = Console.ReadLine()!;

            ValidateString(name);

            user = new ToDoUser(name.Trim());
        }
    }

    private static void Info()
    {
        Console.WriteLine($"ConsoleBot {AppVersion}\nДата создания: {_creationDate}");
    }

    private static void Echo(ToDoUser user, string[] inputParams)
    {
        if (user is null)
        {
            Console.WriteLine($"Вы не ввели свое имя, используйте для этого команду {StartEndpoint}.");
            return;
        }

        if (inputParams.Length > 0)
        {
            foreach (var param in inputParams)
            {
                Console.Write(param + " ");
            }
        }

        Console.WriteLine();
    }

    private static void Help(ToDoUser user)
    {
        string echoEndpointText = string.Empty;
        string addTaskEndpointText = string.Empty;
        string showTasksEndpointText = string.Empty;
        string showAllTasksEndpointText = string.Empty;
        string removeEndpointText = string.Empty;
        string completeEndpointText = string.Empty;

        if (user is not null)
        {
            echoEndpointText = 
                $"{EchoEndpoint}\t\t - Вывести на экран текст, введенный после команды через пробел.\n";

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

        Console.WriteLine("Для взаимодействия с ботом используйте следующие команды:\n" +
            $"{StartEndpoint}\t\t - Точка входа. Введите свое имя для доступа к большему количеству команд.\n" +
            $"{HelpEndpoint}\t\t - Отобразить данную справочную информацию.\n" +
            $"{InfoEndpoint}\t\t - Показать версию и дату создания приложения.\n" +
            echoEndpointText +
            addTaskEndpointText +
            showTasksEndpointText +
            showAllTasksEndpointText +
            removeEndpointText +
            completeEndpointText +
            $"{ExitEndpoint}\t\t - Выйти из программы.");
    }

    private static void AddTask(ToDoUser user)
    {
        Console.Write("Введите описание задачи: ");

        var newTask = Console.ReadLine();

        ValidateString(newTask);
        
        if (_tasks.Count == _maxTasksCount)
            throw new TaskCountLimitException(_maxTasksCount);

        if (newTask!.Length > _maxTaskLength)
            throw new TaskLengthLimitException(newTask.Length, _maxTaskLength);

        foreach (var task in _tasks)
        {
            if (task.Name.Equals(newTask))
                throw new DuplicateTaskException(newTask);
        }

        var newToDoItem = new ToDoItem(user, newTask);
        _tasks.Add(newToDoItem);

        Console.WriteLine($"Задача \"{newToDoItem.Name}\" добавлена.");
    }

    private static bool ShowTasks()
    {
        if (_tasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст.");
            return false;
        }

        var counter = 0;

        for (int i = 0; i < _tasks.Count; i++)
        {
            if (_tasks[i].State.Equals(ToDoItemState.Active))
            {
                Console.WriteLine($"{++counter}. {_tasks[i]}");
            }
        }

        return true;
    }

    private static void ShowAllTasks()
    {
        if (_tasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст.");
            return;
        }

        for (int i = 0; i < _tasks.Count; i++)
        {
            Console.WriteLine($"{i + 1}. ({_tasks[i].State}) {_tasks[i]}");
        }
    }

    private static void RemoveTask()
    {
        if (!ShowTasks())
        {
            return;
        }

        Console.Write("Введите номер задачи для удаления: ");

        var taskNumber = ParseAndValidateInt(Console.ReadLine(), MinTasksCount, _maxTasksCount);
        var task = _tasks[taskNumber - 1];

        _tasks.Remove(task);
        Console.WriteLine($"Задача \"{task}\" удалена.");
    }

    private static void CompleteTask(string[] inputParams)
    {
        if (inputParams.Length != 1)
        {
            Console.WriteLine("Введите некорректное значение id задачи.");
            return;
        }

        var taskId = inputParams[0];
        
        ValidateString(taskId);

        foreach (var task in _tasks)
        {
            if (task.Id.Equals(taskId))
            {
                task.State = ToDoItemState.Completed;
                task.StateChangedAt = DateTime.UtcNow;

                Console.WriteLine($"Задача \"{task}\" завершена.");
                return;
            }
        }

        Console.WriteLine($"Задача с id \"{taskId}\" не найдена.");
    }
}

internal class TaskCountLimitException(int taskCountLimit) : Exception()
{
    int TaskCountLimitValue { get; } = taskCountLimit;

    public override string Message => $"Превышено максимальное количество задач равное {TaskCountLimitValue}.";
}

internal class TaskLengthLimitException(int taskLength, int taskLengthLimit) : Exception()
{
    int TaskLengthValue { get; } = taskLength;
    int TaskLengthLimitValue { get; } = taskLengthLimit;
    
    public override string Message => $"Длина задачи {TaskLengthValue} превышает максимально допустимое значение {TaskLengthLimitValue}.";
}

internal class DuplicateTaskException(string task) : Exception()
{
    string TaskValue { get; } = task;

    public override string Message => $"Задача '{TaskValue}' уже существует.";
}