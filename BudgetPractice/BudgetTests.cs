using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BudgetPractice;

public class BudgetTests
{
    private IBudgetRepo _budgetRepo;
    private BudgetService _budgetService;

    [SetUp]
    public void SetUp()
    {
        _budgetRepo = Substitute.For<IBudgetRepo>();
        _budgetService = new BudgetService(_budgetRepo);
    }

    [Test]
    public void query_one_day_in_single_month_with_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 1));

        BudgetShouldBe(budget, 1000);
    }

    [Test]
    public void query_two_day_in_single_month_with_budget_in_month_start()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));

        BudgetShouldBe(budget, 2000);
    }

    [Test]
    public void query_two_day_in_single_month_in_month_end()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 30), new DateTime(2023, 3, 31));

        BudgetShouldBe(budget, 2000);
    }

    [Test]
    public void query_two_day_in_single_month_with_zero_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 0)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));

        BudgetShouldBe(budget, 0);
    }

    [Test]
    public void query_full_month()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 31));

        BudgetShouldBe(budget, 31000);
    }

    [Test]
    public void query_two_full_month()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000),
            CreateBudget("202304", 3000)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 4, 30));

        BudgetShouldBe(budget, 34000);
    }


    // [Test]
    // public void query_two_day_in_cross_month_with_budget()
    // {
    //     GivenGetAllReturn(new List<Budget>
    //     {
    //         CreateBudget("202303", 31000),
    //         CreateBudget("202304", 15000)
    //     });
    //
    //     var budget = QueryBudget(new DateTime(2023, 3, 31), new DateTime(2023, 4, 1));
    //
    //     BudgetShouldBe(budget, 1500);
    // }

    private static void BudgetShouldBe(decimal budget, int expected)
    {
        budget.Should().Be(expected);
    }

    private decimal QueryBudget(DateTime startTime, DateTime endTime)
    {
        return _budgetService.Query(startTime, endTime);
    }


    private static Budget CreateBudget(string yearMonth, int amount)
    {
        return new Budget
        {
            YearMonth = yearMonth,
            Amount = amount
        };
    }

    private void GivenGetAllReturn(List<Budget> budgets)
    {
        _budgetRepo.GetAll().Returns(t => budgets);
    }
}

public class BudgetRepo : IBudgetRepo
{
    public List<Budget> GetAll()
    {
        return default;
    }
}

public interface IBudgetRepo
{
    List<Budget> GetAll();
}

public class Budget
{
    public string YearMonth { get; set; }
    public int Amount { get; set; }
}

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
        var startBudget = budgets.First(t => t.YearMonth == startTime.ToString("yyyyMM"));
        var endBudget = budgets.First(t => t.YearMonth == endTime.ToString("yyyyMM"));

        if (startTime.ToString("yyyyMM") == endTime.ToString("yyyyMM"))
        {
            return GetBudgetInMonth(startTime, endTime, startBudget);
        }

        return startBudget.Amount + endBudget.Amount;

        decimal GetBudgetInMonth(DateTime startTime1, DateTime endTime1, Budget budget)
        {
            if (IsFullMonth(startTime1, endTime1))
            {
                return budget.Amount;
            }

            var days = (endTime1 - startTime1).Days;
            days++;

            var _ = DateTime.TryParse(budget.YearMonth, out var dateTime);
            var daysInMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

            return budget.Amount / daysInMonth * days;
        }
    }

    private static bool IsFullMonth(DateTime startTime, DateTime endTime)
    {
        return startTime == new DateTime(startTime.Year, startTime.Month, 1)
               && endTime == new DateTime(endTime.Year, endTime.Month, 1).AddMonths(1).AddDays(-1);
    }
}