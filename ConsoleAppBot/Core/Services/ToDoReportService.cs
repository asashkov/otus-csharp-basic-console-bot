using ConsoleBot.Core.DataAccess;

namespace ConsoleBot.Core.Services;

internal class ToDoReportService(IToDoRepository toDoRepository) : IToDoReportService
{
    public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStats(Guid userId, CancellationToken ct)
    {
        var totalItemsCount = (await toDoRepository.GetAllByUserId(userId, ct)).Count;
        var completedItemsCount = (await toDoRepository.GetCompletedByUserId(userId, ct)).Count;
        var activeItemsCount = (await toDoRepository.GetActiveByUserId(userId, ct)).Count;
        var generatedDate = DateTime.UtcNow;

        return (totalItemsCount, completedItemsCount, activeItemsCount, generatedDate);
    }
}
