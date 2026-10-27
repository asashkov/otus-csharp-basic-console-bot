namespace ConsoleBot.Core.DataAccess;

public interface IToDoReportService
{
    (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId);
}
