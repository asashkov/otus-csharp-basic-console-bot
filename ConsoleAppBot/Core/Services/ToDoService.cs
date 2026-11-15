using ConsoleBot.Core.DataAccess;
using ConsoleBot.Core.Entities;
using ConsoleBot.Core.Exceptions;

namespace ConsoleBot.Core.Services;

public class ToDoService(IToDoRepository repository, int maxTasksCount, int maxTaskLength) : IToDoService
{
    private readonly int _maxTasksCount = maxTasksCount;
    private readonly int _maxTaskLength = maxTaskLength;

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        return await repository.GetAllByUserId(userId, ct);
    }

    //Возвращает ToDoItem для UserId со статусом Active
    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
    {
        return await repository.GetActiveByUserId(userId, ct);
    }

    public async Task<ToDoItem> Add(ToDoUser user, string name, CancellationToken ct)
    {
        if (await repository.CountActive(user.UserId, ct) == _maxTasksCount)
            throw new TaskCountLimitException(_maxTasksCount);

        if (name!.Length > _maxTaskLength)
            throw new TaskLengthLimitException(name!.Length, _maxTaskLength);

        if (await repository.ExistsByName(user.UserId, name, ct))
            throw new DuplicateTaskException(name);

        var newItem = new ToDoItem(user, name);

        await repository.Add(newItem, ct);

        return newItem;
    }

    public async Task MarkCompleted(Guid id, CancellationToken ct)
    {
        var item = await repository.Get(id, ct);

        if (item is not null)
        {
            item.State = ToDoItemState.Completed;
            item.StateChangedAt = DateTime.UtcNow;

            await repository.Update(item, ct);
        }
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        await repository.Delete(id, ct);
    }

    public async Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken ct)
    {
        return await repository.Find(user.UserId, toDoItem => toDoItem.Name.StartsWith(namePrefix), ct);
    }
}
