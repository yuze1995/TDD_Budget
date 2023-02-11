using FluentAssertions;
using NSubstitute;

namespace BudgetServiceTests;

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
        
        _budgetService = new BudgetService(_budgetRepo);
        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 1));

        BudgetShouldBe(budget, 1000);
    }

    [Test]
    public void query_two_days_in_single_month_with_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000)
        });
        
        _budgetService = new BudgetService(_budgetRepo);
        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));

        BudgetShouldBe(budget, 2000);
    }

    [Test]
    public void query_two_days_in_single_month_with_budget_at_month_start()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));

        BudgetShouldBe(budget, 2000);
    }

    [Test]
    public void query_two_days_in_single_month_with_budget_at_month_end()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 30), new DateTime(2023, 3, 31));

        BudgetShouldBe(budget, 2000);
    }

    [Test]
    public void query_two_days_in_single_month_with_zero_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 0)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));

        BudgetShouldBe(budget, 0);
    }

    [Test]
    public void query_two_days_in_single_month_with_no_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 2));

        BudgetShouldBe(budget, 0);
    }

    [Test]
    public void query_one_day_in_single_month_with_no_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 1));

        BudgetShouldBe(budget, 0);
    }
    
    [Test]
    public void query_one_day_in_single_month_with_zero_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 3, 1));

        BudgetShouldBe(budget, 0);
    }

    [Test]
    public void query_full_month_in_one_month()
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

    [Test]
    public void query_two_days_cross_two_months()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000),
            CreateBudget("202304", 3000)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 31), new DateTime(2023, 4, 1));

        BudgetShouldBe(budget, 1100);
    }

    [Test]
    public void query_three_full_months_but_middle_month_no_budget()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000),
            CreateBudget("202305", 310)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 1), new DateTime(2023, 5, 31));

        BudgetShouldBe(budget, 31310);
    }
    
    [Test]
    public void query_four_months_when_first_month_at_last_date_and_second_month_with_full_month_and_third_month_zero_budget_and_forth_month_at_first_date()
    {
        GivenGetAllReturn(new List<Budget>
        {
            CreateBudget("202303", 31000),
            CreateBudget("202304", 3000),
            CreateBudget("202305", 0),
            CreateBudget("202306", 30)
        });

        var budget = QueryBudget(new DateTime(2023, 3, 31), new DateTime(2023, 6, 1));

        BudgetShouldBe(budget, 4001);
    }

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