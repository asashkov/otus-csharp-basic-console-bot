using ConsoleBot.Core.DataAccess;
using ConsoleBot.Core.Entities;
using Otus.ToDoList.ConsoleBot.Types;
using System.Text.Json;

namespace ConsoleBot.Infrastructure.DataAccess;

internal class FileUserRepository : IUserRepository
{
    private readonly List<ToDoUser> userList = [];

    private readonly string? _baseDir = null;

    private readonly string? _indexFilePath = null;

    private const string FileExtension = ".json";

    private const string IndexFile = "userIndex" + FileExtension;

    private readonly JsonSerializerOptions _options = new JsonSerializerOptions { WriteIndented = true };

    public FileUserRepository(string baseDir)
    {
        if (!Directory.Exists(_baseDir))
        {
            Directory.CreateDirectory(baseDir);
            _baseDir = baseDir;
        }

        _indexFilePath = Path.Combine(_baseDir, IndexFile);

        if (!File.Exists(_indexFilePath))
            File.Create(_indexFilePath);
    }

    public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
    {
        ToDoUser? toDoUser = null;

        try
        {
            var index = File.ReadAllText(_indexFilePath!);

            Dictionary<string, string> indexData = string.IsNullOrWhiteSpace(index)
                ? []
                : JsonSerializer.Deserialize<Dictionary<string, string>>(index)
                ?? [];

            if (indexData.Count > 0)
            {
                foreach (var key in indexData.Keys)
                {
                    if (key.Equals(userId.ToString()))
                    {
                        using var userReader = new StreamReader(Path.Combine(_baseDir!, key + FileExtension));
                        toDoUser = await JsonSerializer.DeserializeAsync<ToDoUser>(userReader.BaseStream, cancellationToken: ct);
                    }
                }
            }
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (JsonException e)
        {
            Console.WriteLine(e.Message);
        }

        return toDoUser;
    }

    public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
    {
        ToDoUser? toDoUser = null;

        try
        {
            var index = File.ReadAllText(_indexFilePath!);

            Dictionary<string, string> indexData = string.IsNullOrWhiteSpace(index)
                ? []
                : JsonSerializer.Deserialize<Dictionary<string, string>>(index)
                ?? [];

            if (indexData.Count > 0)
            {
                foreach (var key in indexData.Keys)
                {
                    if (!indexData.TryGetValue(key, out string? value))
                    {
                        if (value!.Equals(telegramUserId.ToString()))
                        {
                            using var userReader = new StreamReader(Path.Combine(_baseDir!, key + FileExtension));
                            toDoUser = await JsonSerializer.DeserializeAsync<ToDoUser>(userReader.BaseStream, cancellationToken: ct);
                        }
                    }
                }
            }
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (JsonException e)
        {
            Console.WriteLine(e.Message);
        }

        return toDoUser;
    }

    public async Task Add(ToDoUser user, CancellationToken ct)
    {
        await using var taskWriter = new StreamWriter(Path.Combine(_baseDir!, user.UserId.ToString() + FileExtension));
        await JsonSerializer.SerializeAsync(taskWriter.BaseStream, user, _options, ct);
        await UpdateIndexFile(user, ct);
    }

    private async Task UpdateIndexFile(ToDoUser user, CancellationToken ct)
    {
        var index = File.ReadAllText(_indexFilePath!);

        Dictionary<string, List<string>> indexData = string.IsNullOrWhiteSpace(index)
            ? []
            : JsonSerializer.Deserialize<Dictionary<string, List<string>>>(index)
            ?? [];

        if (!indexData.TryGetValue(user.UserId.ToString(), out List<string>? values))
        {
            values = [];
        }

        values.Add(user.TelegramUserId.ToString());
        indexData[user.UserId.ToString()] = values;

        await using var indexWriter = new StreamWriter(_indexFilePath!);
        await JsonSerializer.SerializeAsync(indexWriter.BaseStream, indexData, _options, ct);
    }
}
