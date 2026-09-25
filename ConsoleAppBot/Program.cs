namespace ConsoleBot;

public class Program
{
    private const string StartEndpoint = "/start";
    private const string HelpEndpoint = "/help";
    private const string InfoEndpoint = "/info";
    private const string EchoEndpoint = "/echo";
    private const string AddTaskEndpoint = "/addtask";
    private const string ShowTasksEndpoint = "/showtasks";
    private const string RemoveTaskEndpoint = "/removetask";
    private const string ExitEndpoint = "/exit";

    private static List<string> _tasks = new List<string>();

    static void Main()
    {
        DateOnly creationDate = DateOnly.Parse("05.09.2026");

        Console.WriteLine($"Привет!\n" +
            $"Для взаимодействия доступны следующие команды: " +
            $"{StartEndpoint}, {HelpEndpoint}, {InfoEndpoint}, {ExitEndpoint}");

        string? userName = null;
        bool isNameTaken = false;

        while (true)
        {
            PrintMenu(isNameTaken, userName);

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
                    if (isNameTaken)
                    {
                        Console.WriteLine($"Вы уже ввели имя {userName}. Используйте другую команду.");
                    }
                    else
                    {
                        Console.Write("Введите Ваше имя: ");

                        var name = Console.ReadLine();

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            userName = name.Trim();
                            isNameTaken = true;
                        }
                    }

                    break;
                case HelpEndpoint:
                    PrintHelpEndpointText(isNameTaken);
                    break;
                case InfoEndpoint:
                    Console.WriteLine("ConsoleBot 0.0.3\n" + $"Дата создания: {creationDate}");
                    break;
                case EchoEndpoint:
                    PrintEchoEndpointText(isNameTaken, inputParams);
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
    }

    private static void PrintMenu(bool isNameTaken, string userName)
    {
        string greating = !isNameTaken ? "Пожалуйста" : $"{userName}, пожалуйста";

        Console.WriteLine();
        Console.Write($"{greating} введите команду: ");
    }

    private static void PrintEchoEndpointText(bool isNameTaken, string[] inputParams)
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

    private static void PrintHelpEndpointText(bool isNameTaken)
    {
        string echoEndpointText = string.Empty;
        string addTaskEndpointText = string.Empty;
        string showTasksEndpointText = string.Empty;
        string removeEndpointText = string.Empty;

        if (isNameTaken)
        {
            echoEndpointText = 
                $"{EchoEndpoint}\t\t - Вы можете отобразить на экране текст, введя его после команды через пробел.\n";

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
            $"{InfoEndpoint}\t\t - Отобразить версию и дату создания приложения.\n" +
            echoEndpointText +
            addTaskEndpointText +
            showTasksEndpointText +
            removeEndpointText +
            $"{ExitEndpoint}\t\t - Выход из программы.");
    }

    private static void AddTask()
    {
        Console.Write("Введите описание задачи: ");

        var task = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(task))
        {
            Console.WriteLine($"Вы ввели некорректное описание задачи.");
            return;
        }

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

        if (!int.TryParse(Console.ReadLine(), out int taskNumber)
            || (taskNumber < 1 || taskNumber > _tasks.Count))
        {
            Console.WriteLine($"Неверный номер задачи. Пожалуйста, введите корректное значение (1 - {_tasks.Count})");
            return;
        }
        else
        {
            var task = _tasks[taskNumber - 1];

            _tasks.Remove(task);
            Console.WriteLine($"Задача \"{task}\" удалена.");
        }
    }
}
