using Microsoft.AspNetCore.Mvc;
using WebBanHang.Services;

namespace WebBanHang.Controllers;

[Route("Cart")]
public class CartController : Controller
{
    private readonly CartService _cartService;

    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost("Add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        if (!await _cartService.AddAsync(HttpContext.Session, productId, quantity))
        {
            return NotFound();
        }

        return RedirectToAction("Cart", "Home");
    }

    [HttpPost("Update")]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int productId, int quantity)
    {
        _cartService.Update(HttpContext.Session, productId, quantity);
        return RedirectToAction("Cart", "Home");
    }

    [HttpPost("Remove")]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        _cartService.Remove(HttpContext.Session, productId);
        return RedirectToAction("Cart", "Home");
    }

    [HttpPost("Clear")]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _cartService.Clear(HttpContext.Session);
        return RedirectToAction("Cart", "Home");
    }
}
