using DrMajokoProjects.Web.Models.ViewModels;

namespace DrMajokoProjects.Web.Services
{
    public interface IPerformanceManager
    {
        PerformanceMetrics CalculateOverallPerformance(List<ContractorTaskViewModel> tasks);
        List<PhasePerformance> CalculatePhasePerformance(List<ContractorTaskViewModel> tasks);
    }

    public class PerformanceManager : IPerformanceManager
    {
        public PerformanceMetrics CalculateOverallPerformance(List<ContractorTaskViewModel> tasks)
        {
            if (!tasks.Any())
            {
                return new PerformanceMetrics();
            }

            var completedTasks = tasks.Where(t => t.Status == "Done").ToList();
            var inProgressTasks = tasks.Where(t => t.Status == "In Progress").ToList();
            var delayedTasks = completedTasks.Where(t => t.DaysLate > 0).ToList();
            var onTimeTasks = completedTasks.Where(t => t.DaysLate == 0).ToList();

            var totalTasks = tasks.Count;
            var completedCount = completedTasks.Count;

            return new PerformanceMetrics
            {
                OnTrackPercentage = totalTasks > 0 ? (double)onTimeTasks.Count / totalTasks * 100 : 0,
                DelayedPercentage = totalTasks > 0 ? (double)delayedTasks.Count / totalTasks * 100 : 0,
                InProgressPercentage = totalTasks > 0 ? (double)inProgressTasks.Count / totalTasks * 100 : 0,
                AverageDelayDays = delayedTasks.Any() ? delayedTasks.Average(t => t.DaysLate) : 0,
                CompletionRate = totalTasks > 0 ? (double)completedCount / totalTasks * 100 : 0,
                TotalTasksCompleted = completedCount,
                TotalTasksOnTime = onTimeTasks.Count
            };
        }

        public List<PhasePerformance> CalculatePhasePerformance(List<ContractorTaskViewModel> tasks)
        {
            return tasks.GroupBy(t => new { t.PhaseId, t.PhaseName })
                .Select(g =>
                {
                    var phaseTasks = g.ToList();
                    var completed = phaseTasks.Where(t => t.Status == "Done").ToList();
                    var onTime = completed.Where(t => t.DaysLate == 0).ToList();
                    var delayed = completed.Where(t => t.DaysLate > 0).ToList();

                    return new PhasePerformance
                    {
                        PhaseId = g.Key.PhaseId,
                        PhaseName = g.Key.PhaseName,
                        TotalTasks = phaseTasks.Count,
                        CompletedTasks = completed.Count,
                        OnTimeTasks = onTime.Count,
                        DelayedTasks = delayed.Count,
                        AverageDelayDays = delayed.Any() ? delayed.Average(t => t.DaysLate) : 0,
                        CompletionPercentage = phaseTasks.Count > 0 ? (double)completed.Count / phaseTasks.Count * 100 : 0
                    };
                })
                .OrderBy(p => p.PhaseName)
                .ToList();
        }
    }
}