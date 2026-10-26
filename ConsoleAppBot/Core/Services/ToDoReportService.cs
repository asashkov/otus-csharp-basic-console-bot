using ConsoleBot.Core.DataAccess;

namespace ConsoleBot.Core.Services;

internal class ToDoReportService(IToDoRepository toDoRepository) : IToDoReportService
{
    public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
    {
        var totalItemsCount = toDoRepository.GetAllByUserId(userId).Count;
        var completedItemsCount = toDoRepository.GetCompletedByUserId(userId).Count;
        var activeItemsCount = toDoRepository.GetActiveByUserId(userId).Count;
        var generatedDate = DateTime.UtcNow;

        return (totalItemsCount, completedItemsCount, activeItemsCount, generatedDate);
    }
}
