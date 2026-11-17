using FinTracker.Models;
using FinTracker.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTracker.Controllers;

public class HomeController : Controller
{
    private readonly IConfiguration _configuration;
    public HomeController(IConfiguration configuration) => _configuration = configuration;

    public IActionResult Index() => User.Identity?.IsAuthenticated == true
        ? RedirectToAction(nameof(Dashboard))
        : View("Welcome");

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var repo = new TransactionRepository(_configuration.GetConnectionString("DefaultConnection")!);
        var transactions = await repo.GetAllAsync(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var model = new DashboardViewModel
        {
            TotalIncome = transactions.Where(x => x.Type == "Income").Sum(x => x.Amount),
            TotalExpenses = transactions.Where(x => x.Type == "Expense").Sum(x => x.Amount),
            RecentTransactions = transactions.Take(6).ToList()
        };
        return View(model);
    }
}
