using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = IdentitySeeder.AdminRole)]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        if (!OrderStatuses.All.Contains(status))
        {
            return BadRequest("Trạng thái đơn hàng không hợp lệ.");
        }

        var order = await _context.Orders.FindAsync(id);
        if (order is null)
        {
            return NotFound();
        }

        order.Status = status;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
