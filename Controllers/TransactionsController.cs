using FinTracker.Models;
using FinTracker.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTracker.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    private readonly TransactionRepository _repo;
    private string CurrentUserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

    public TransactionsController(IConfiguration configuration)
    {
        _repo = new TransactionRepository(configuration.GetConnectionString("DefaultConnection")!);
    }

    public async Task<IActionResult> Index() => View(await _repo.GetAllAsync(CurrentUserId));

    [HttpGet]
    public IActionResult Create() => View(new FinanceTransaction { TransactionDate = DateTime.Today });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FinanceTransaction model)
    {
        ValidateType(model);
        if (!ModelState.IsValid) return View(model);
        await _repo.AddAsync(model, CurrentUserId);
        TempData["Success"] = "Transaction added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _repo.GetByIdAsync(id, CurrentUserId);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FinanceTransaction model)
    {
        if (id != model.Id) return BadRequest();
        ValidateType(model);
        if (!ModelState.IsValid) return View(model);
        await _repo.UpdateAsync(model, CurrentUserId);
        TempData["Success"] = "Transaction updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _repo.DeleteAsync(id, CurrentUserId);
        TempData["Success"] = "Transaction deleted.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidateType(FinanceTransaction model)
    {
        if (model.Type != "Income" && model.Type != "Expense")
            ModelState.AddModelError(nameof(model.Type), "Choose Income or Expense.");
    }
}
