namespace ConsoleBot;

public class Program
{
    private static DateOnly _creationDate = new DateOnly(2026, 9, 5);
    private const string AppVersion = "0.1.0";

    private const string StartEndpoint = "/start";
    private const string HelpEndpoint = "/help";
    private const string InfoEndpoint = "/info";
    private const string EchoEndpoint = "/echo";
    private const string AddTaskEndpoint = "/addtask";
    private const string ShowTasksEndpoint = "/showtasks";
    private const string RemoveTaskEndpoint = "/removetask";
    private const string ExitEndpoint = "/exit";

    private const int MinTasksCount = 1;
    private const int MinTaskLength = 1;
    
    private static int _maxTasksCount = 100;
    private static int _maxTaskLength = 100;

    private static List<string> _tasks = new List<string>(_maxTasksCount);

    private static bool _initialized = false;

    static void Main()
    {
        Console.WriteLine($"Привет!\n" +
            $"Для взаимодействия доступны следующие команды: " +
            $"{StartEndpoint}, {HelpEndpoint}, {InfoEndpoint}, {ExitEndpoint}");

        string? userName = null;
        bool isNameTaken = false;

        
        while (true)
        {
            try
            {
                if (!_initialized)
                {
                    Console.WriteLine();
                    Console.Write("Введите максимально допустимое количество задач: ");

                    var tasksCount = ParseAndValidateInt(Console.ReadLine()!, MinTasksCount, _maxTasksCount);
                    _maxTasksCount = tasksCount;

                    Console.Write("Введите максимально допустимую длину задачи: ");

                    var taskLength = ParseAndValidateInt(Console.ReadLine()!, MinTaskLength, _maxTaskLength);
                    _maxTaskLength = taskLength;

                    _tasks = new List<string>(tasksCount);
                    _initialized = true;
                }

                DataEntryPrompt(isNameTaken, userName!);

                var input = Console.ReadLine();
                string[] inputParams = [];

                if (!string.IsNullOrWhiteSpace(input))
                {
                    var inputWithParams = input.Trim().Split(' ');
                    input = inputWithParams[0];
                    inputParams = inputWithParams.Skip(1).ToArray();
                }

                switch (input)
                {
                    case StartEndpoint:
                        Start(ref isNameTaken, ref userName!);
                        break;
                    case HelpEndpoint:
                        Help(isNameTaken);
                        break;
                    case InfoEndpoint:
                        Info();
                        break;
                    case EchoEndpoint:
                        Echo(isNameTaken, inputParams);
                        break;
                    case AddTaskEndpoint:
                        AddTask();
                        break;
                    case ShowTasksEndpoint:
                        ShowTasks();
                        break;
                    case RemoveTaskEndpoint:
                        RemoveTask();
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
                    $"InnerException: {e.InnerException}");
            }
        }
    }

    private static int ParseAndValidateInt(string? str, int min, int max)
    {
        var parsedValue = int.Parse(str!);

        if (parsedValue >= min && parsedValue <= max)
        {
            return parsedValue;
        }
        else
        {
            throw new ArgumentException($"Допустимое значение от {min} до {max}.");
        }
    }

    private static void ValidateString(string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            throw new ArgumentException("Значение должно быть не null, не пустой строкой и иметь символы кроме пробела.");
        }
    }

    private static void DataEntryPrompt(bool isNameTaken, string userName)
    {
        string greating = !isNameTaken ? "Пожалуйста," : $"{userName}, пожалуйста,";

        Console.WriteLine();
        Console.Write($"{greating} введите команду: ");
    }

    private static void Start(ref bool isNameTaken, ref string userName)
    {
        if (isNameTaken)
        {
            Console.WriteLine($"Вы уже ввели имя {userName}. Используйте другую команду.");
        }
        else
        {
            Console.Write("Введите Ваше имя: ");

            var name = Console.ReadLine();

            ValidateString(name);

            userName = name!.Trim();
            isNameTaken = true;
        }
    }

    private static void Info()
    {
        Console.WriteLine($"ConsoleBot {AppVersion}\nДата создания: {_creationDate}");
    }

    private static void Echo(bool isNameTaken, string[] inputParams)
    {
        if (!isNameTaken)
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

    private static void Help(bool isNameTaken)
    {
        string echoEndpointText = string.Empty;
        string addTaskEndpointText = string.Empty;
        string showTasksEndpointText = string.Empty;
        string removeEndpointText = string.Empty;

        if (isNameTaken)
        {
            echoEndpointText = 
                $"{EchoEndpoint}\t\t - Вывести на экран текст, введенный после команды через пробел.\n";

            addTaskEndpointText =
                $"{AddTaskEndpoint}\t - Добавить в список новую задачу.\n";

            showTasksEndpointText =
                $"{ShowTasksEndpoint}\t - Отобразить все задачи в списке.\n";

            removeEndpointText =
                $"{RemoveTaskEndpoint}\t - Удалить задачу из списка по ее номеру.\n";
        }

        Console.WriteLine("Для взаимодействия с ботом используйте следующие команды:\n" +
            $"{StartEndpoint}\t\t - Точка входа. Введите свое имя для доступа к большему количеству команд.\n" +
            $"{HelpEndpoint}\t\t - Отобразить данную справочную информацию.\n" +
            $"{InfoEndpoint}\t\t - Показать версию и дату создания приложения.\n" +
            echoEndpointText +
            addTaskEndpointText +
            showTasksEndpointText +
            removeEndpointText +
            $"{ExitEndpoint}\t\t - Выйти из программы.");
    }

    private static void AddTask()
    {
        Console.Write("Введите описание задачи: ");

        var task = Console.ReadLine();

        ValidateString(task);
        
        if (_tasks.Count == _maxTasksCount)
            throw new TaskCountLimitException(_maxTasksCount);

        if (task!.Length > _maxTaskLength)
            throw new TaskLengthLimitException(task.Length, _maxTaskLength);

        if (_tasks.Contains(task))
            throw new DuplicateTaskException(task);

        _tasks.Add(task);

        Console.WriteLine($"Задача \"{task}\" добавлена.");
    }

    private static bool ShowTasks()
    {
        if (_tasks.Count == 0)
        {
            Console.WriteLine("Список задач пуст.");
            return false;
        }

        for (int i = 0; i < _tasks.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {_tasks[i]}");
        }

        return true;
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