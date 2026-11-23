using ConsoleBot.Core.DataAccess;
using ConsoleBot.Core.Entities;
using System.Text.Json;

namespace ConsoleBot.Infrastructure.DataAccess;

internal class FileToDoRepository : IToDoRepository
{
    private readonly string? _baseDir = null;

    private readonly string? _indexFilePath = null;

    private const string FileExtension = ".json";

    private const string IndexFile = "itemIndex" + FileExtension;

    private readonly JsonSerializerOptions _options = new JsonSerializerOptions { WriteIndented = true };

    public FileToDoRepository(string baseDir)
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

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        List<ToDoItem> toDoItems = [];

        try
        {
            var index = File.ReadAllText(_indexFilePath!);

            Dictionary<string, List<string>> indexData = string.IsNullOrWhiteSpace(index)
                ? []
                : JsonSerializer.Deserialize<Dictionary<string, List<string>>>(index)
                ?? [];

            if (!indexData.TryGetValue(userId.ToString(), out List<string>? values))
            {
                values = [];
            }

            if (values.Count > 0)
            {
                foreach (var value in values)
                {
                    using var taskReader = new StreamReader(Path.Combine(_baseDir!, userId.ToString(), value + FileExtension));
                    ToDoItem? toDoItem = await JsonSerializer.DeserializeAsync<ToDoItem>(taskReader.BaseStream, cancellationToken: ct);

                    toDoItems.Add(toDoItem!);
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

        return toDoItems;
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
    {
        var userTasks = await GetAllByUserId(userId, ct);
        return userTasks.Where(item => item.State.Equals(ToDoItemState.Active)).ToList();
    }

    public async Task<IReadOnlyList<ToDoItem>> GetCompletedByUserId(Guid userId, CancellationToken ct)
    {
        var userTasks = await GetAllByUserId(userId, ct);
        return userTasks.Where(item => item.State.Equals(ToDoItemState.Completed)).ToList();
    }

    public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
    {
        ToDoItem? toDoItem = null;

        try
        {
            var index = File.ReadAllText(_indexFilePath!);

            Dictionary<string, List<string>> indexData = string.IsNullOrWhiteSpace(index)
                ? []
                : JsonSerializer.Deserialize<Dictionary<string, List<string>>>(index)
                ?? [];

            if (indexData.Count > 0)
            {
                foreach (var key in indexData.Keys)
                {
                    if (!indexData.TryGetValue(key, out List<string>? values))
                    {
                        values = [];
                    }

                    if (values.Count > 0)
                    {
                        foreach (var value in values)
                        {
                            if (value.Equals(id.ToString()))
                            {
                                using var taskReader = new StreamReader(Path.Combine(_baseDir!, key, value + FileExtension));
                                toDoItem = await JsonSerializer.DeserializeAsync<ToDoItem>(taskReader.BaseStream, cancellationToken: ct);
                            }
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

        return toDoItem;
    }

    public async Task Add(ToDoItem item, CancellationToken ct)
    {
        var tasksFolder = Path.Combine(_baseDir!, item.User.UserId.ToString());

        if (!Directory.Exists(tasksFolder))
            Directory.CreateDirectory(tasksFolder);

        await using var taskWriter = new StreamWriter(Path.Combine(tasksFolder, item.Id.ToString() + FileExtension));
        await JsonSerializer.SerializeAsync(taskWriter.BaseStream, item, _options, ct);
        await UpdateIndexFile(item.User.UserId, item.Id, ct);
    }

    public async Task Update(ToDoItem item, CancellationToken ct)
    {
        var toDoItem = Get(item.Id, ct);

        if (toDoItem is not null)
            await Add(item, ct); // Is this correct?
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        await Task.Run(() =>
        {
            var filePath = id.ToString() + FileExtension;

            if (!File.Exists(filePath))
                return;

            File.Delete(filePath);
        }, ct);

        var toDoItem = await Get(id, ct);
        var userId = toDoItem!.User.UserId.ToString();

        var index = File.ReadAllText(_indexFilePath!);

        Dictionary<string, List<string>> indexData = string.IsNullOrWhiteSpace(index)
            ? []
            : JsonSerializer.Deserialize<Dictionary<string, List<string>>>(index)
            ?? [];

        if (!indexData.TryGetValue(userId, out List<string>? values))
        {
            values = [];
        }
        else
        {
            values.Remove(id.ToString());
        }

        indexData[userId.ToString()] = values;

        await using var indexWriter = new StreamWriter(_indexFilePath!);
        await JsonSerializer.SerializeAsync(indexWriter.BaseStream, indexData, _options, ct);
    }

    public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
    {
        var userTasks = await Find(userId, toDoItem => toDoItem.Name.Equals(name), ct);

        return userTasks.Count != 0;
    }

    public async Task<int> CountActive(Guid userId, CancellationToken ct)
    {
        return (await GetActiveByUserId(userId, ct)).Count;
    }

    public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
    {
        return (await GetAllByUserId(userId, ct)).Where(predicate).ToList();
    }

    private async Task UpdateIndexFile(Guid userId, Guid itemId, CancellationToken ct)
    {
        var index = File.ReadAllText(_indexFilePath!);

        Dictionary<string, List<string>> indexData = string.IsNullOrWhiteSpace(index)
            ? []
            : JsonSerializer.Deserialize<Dictionary<string, List<string>>>(index)
            ?? [];

        if (!indexData.TryGetValue(userId.ToString(), out List<string>? values))
        {
            values = [];
        }

        values.Add(itemId.ToString());
        indexData[userId.ToString()] = values;

        await using var indexWriter = new StreamWriter(_indexFilePath!);
        await JsonSerializer.SerializeAsync(indexWriter.BaseStream, indexData, _options, ct);
    }
}
