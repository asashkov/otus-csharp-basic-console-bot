using ConsoleBot.Core.DataAccess;
using ConsoleBot.Core.Entities;
using ConsoleBot.Core.Exceptions;

namespace ConsoleBot.Core.Services;

public class ToDoService(IToDoRepository repository, int maxTasksCount, int maxTaskLength) : IToDoService
{
    private readonly int _maxTasksCount = maxTasksCount;
    private readonly int _maxTaskLength = maxTaskLength;

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return repository.GetAllByUserId(userId);
    }

    //Возвращает ToDoItem для UserId со статусом Active
    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return repository.GetActiveByUserId(userId);
    }

    public ToDoItem Add(ToDoUser user, string name)
    {
        if (repository.CountActive(user.UserId) == _maxTasksCount)
            throw new TaskCountLimitException(_maxTasksCount);

        if (name!.Length > _maxTaskLength)
            throw new TaskLengthLimitException(name!.Length, _maxTaskLength);

        if (repository.ExistsByName(user.UserId, name))
            throw new DuplicateTaskException(name);

        var newItem = new ToDoItem(user, name);

        repository.Add(newItem);

        return newItem;
    }

    public void MarkCompleted(Guid id)
    {
        var item = repository.Get(id);

        if (item is not null)
        {
            item.State = ToDoItemState.Completed;
            item.StateChangedAt = DateTime.UtcNow;

            repository.Update(item);
        }
    }

    public void Delete(Guid id)
    {
        repository.Delete(id);
    }

    public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
    {
        return repository.Find(user.UserId, toDoItem => toDoItem.Name.StartsWith(namePrefix));
    }
}
