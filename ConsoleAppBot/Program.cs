namespace ConsoleBot;

public class Program
{
    private const string StartEndpoint = "/start";
    private const string HelpEndpoint = "/help";
    private const string InfoEndpoint = "/info";
    private const string EchoEndpoint = "/echo";
    private const string ExitEndpoint = "/exit";

    static void Main()
    {
        DateOnly creationDate = DateOnly.Parse("05.09.2026");

        Console.WriteLine($"Hello!\n" +
            $"Available commands to interact with the console bot: " +
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
                        Console.WriteLine($"You are logged in as {userName}. Use one of the other commands.");
                    }
                    else
                    {
                        Console.Write("Enter your name: ");

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
                    Console.WriteLine("ConsoleBot 0.0.1\n" + $"Cteation Date: {creationDate}");
                    break;
                case EchoEndpoint:
                    PrintEchoEndpointText(isNameTaken, inputParams);
                    break;
                case ExitEndpoint:
                    return;
                default:
                    Console.WriteLine("You typed an incorrect command, try again.");
                    break;
            }
        }
    }

    private static void PrintMenu(bool isNameTaken, string userName)
    {
        string greating = !isNameTaken ? "Please" : $"{userName}, please";

        Console.WriteLine();
        Console.Write($"{greating} enter the command: ");
    }

    private static void PrintEchoEndpointText(bool isNameTaken, string[] inputParams)
    {
        if (!isNameTaken)
        {
            Console.WriteLine($"You are not logged in. Type your name using {StartEndpoint} command.");
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
        string echoEndpointText = !isNameTaken ? "" : 
            $"{EchoEndpoint}\t - You make to print on screen some text if you type it after command using a space character for separation.\n";

        Console.WriteLine("To interact with the console bot you need to use following commands:\n" +
            $"{StartEndpoint}\t - Program entry point. Type your name to get more available commands.\n" +
            $"{HelpEndpoint}\t - Display this info.\n" +
            $"{InfoEndpoint}\t - Display program version and creation date.\n" +
            echoEndpointText +
            $"{ExitEndpoint}\t - Exit from program.");
    }
}
