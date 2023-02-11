using System;
using System.Linq;

namespace BudgetServiceTests;

public class BudgetService
{
    private readonly IBudgetRepo _budgetRepo;

    public BudgetService(IBudgetRepo budgetRepo)
    {
        _budgetRepo = budgetRepo;
    }

    public decimal Query(DateTime startTime, DateTime endTime)
    {
        var budgets = _budgetRepo.GetAll();

        var currentMonthFirstDate = new DateTime(startTime.Year, startTime.Month, 1);

        var res = 0M;
        while (currentMonthFirstDate <= endTime)
        {
            var budget = budgets.FirstOrDefault(p => p.YearMonth == currentMonthFirstDate.ToString("yyyyMM"));
            if (budget is not null)
            {
                var currentMonthLastDate = new DateTime(currentMonthFirstDate.Year, currentMonthFirstDate.Month, 1).AddMonths(1).AddDays(-1);
                var monthStart = currentMonthFirstDate < startTime ? startTime : currentMonthFirstDate;
                var monthEnd = currentMonthLastDate > endTime ? endTime : currentMonthLastDate;
                res += GetBudget(monthStart, monthEnd, budget);
            }

            currentMonthFirstDate = currentMonthFirstDate.AddMonths(1);
        }

        return res;
    }

    private decimal GetBudget(DateTime startTime, DateTime endTime, Budget budget)
    {
        if (IsFullMonth(startTime, endTime))
        {
            return budget.Amount;
        }

        var days = (endTime - startTime).Days;
        days++;

        var _ = DateOnly.TryParseExact(budget.YearMonth + "01", "yyyyMMdd", out var date);
        var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);

        return (budget.Amount / daysInMonth) * days;
    }

    private static bool IsFullMonth(DateTime startTime, DateTime endTime)
    {
        return startTime == new DateTime(startTime.Year, startTime.Month, 1)
               && endTime == new DateTime(endTime.Year, endTime.Month, 1).AddMonths(1).AddDays(-1);
    }
}