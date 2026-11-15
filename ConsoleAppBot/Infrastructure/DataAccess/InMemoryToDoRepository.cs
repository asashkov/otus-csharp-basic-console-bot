using ConsoleBot.Core.DataAccess;
using ConsoleBot.Core.Entities;

namespace ConsoleBot.Infrastructure.DataAccess;

internal class InMemoryToDoRepository : IToDoRepository
{
    private readonly List<ToDoItem> _itemList = [];
    
    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        return _itemList.Where(item => item.User.UserId.Equals(userId)).ToList();
    }

    //Возвращает ToDoItem для UserId со статусом Active
    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
    {
        return _itemList.Where(item => item.User.UserId.Equals(userId)&& item.State.Equals(ToDoItemState.Active)).ToList();
    }

    //Возвращает ToDoItem для UserId со статусом Completed
    public async Task<IReadOnlyList<ToDoItem>> GetCompletedByUserId(Guid userId, CancellationToken ct)
    {
        return _itemList.Where(item => item.User.UserId.Equals(userId) && item.State.Equals(ToDoItemState.Completed)).ToList();
    }

    public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
    {
        return _itemList.SingleOrDefault(item => item.Id.Equals(id));
    }

    public async Task Add(ToDoItem item, CancellationToken ct)
    {
        _itemList.Add(item);
    }

    public async Task Update(ToDoItem item, CancellationToken ct)
    {
        var itemIndex = _itemList.FindIndex(listItem => listItem.Id.Equals(item.Id));

        if (itemIndex != -1)
            _itemList[itemIndex] = item;
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        _itemList.RemoveAll(item => item.Id.Equals(id));
    }

    //Проверяет есть ли задача с таким именем у пользователя
    public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
    {
        return _itemList.Exists(item => item.User.UserId.Equals(userId) && item.Name.Equals(name));
    }

    //Возвращает количество активных задач у пользователя
    public async Task<int> CountActive(Guid userId, CancellationToken ct)
    {
        return (await GetActiveByUserId(userId, ct)).Count;
    }

    public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
    {
        return (await GetAllByUserId(userId, ct)).Where(predicate).ToList();
    }
}
