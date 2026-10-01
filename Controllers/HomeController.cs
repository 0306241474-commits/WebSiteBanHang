using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;
using WebBanHang.Services;

namespace WebBanHang.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly CartService _cartService;

    public HomeController(ApplicationDbContext context, CartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    [HttpGet("/")]
    [HttpGet("/index.html")]
    [HttpGet("/Home/Index")]
    public async Task<IActionResult> Index()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(product => product.IsActive)
            .Include(product => product.Category)
            .OrderByDescending(product => product.Id)
            .ToListAsync();
        return View(products);
    }

    [HttpGet("/shop.html")]
    [HttpGet("/Home/Shop")]
    public async Task<IActionResult> Shop(int? categoryId)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ToListAsync();

        var productsQuery = _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Where(product => product.IsActive);

        if (categoryId.HasValue)
        {
            productsQuery = productsQuery.Where(product => product.CategoryId == categoryId.Value);
        }

        return View(new ShopViewModel
        {
            Categories = categories,
            Products = await productsQuery.OrderByDescending(product => product.Id).ToListAsync(),
            SelectedCategoryId = categoryId
        });
    }

    [HttpGet("/product-details.html")]
    [HttpGet("/Home/Details")]
    public async Task<IActionResult> Details(int? id)
    {
        if (!id.HasValue)
        {
            return RedirectToAction(nameof(Shop));
        }

        var product = await _context.Products
            .AsNoTracking()
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id.Value && item.IsActive);

        if (product is null)
        {
            return NotFound();
        }

        var relatedProducts = await _context.Products
            .AsNoTracking()
            .Where(item =>
                item.IsActive &&
                item.CategoryId == product.CategoryId &&
                item.Id != product.Id)
            .OrderByDescending(item => item.Id)
            .Take(8)
            .ToListAsync();

        return View(new ProductDetailsViewModel
        {
            Product = product,
            RelatedProducts = relatedProducts
        });
    }

    [HttpGet("/cart.html")]
    [HttpGet("/Home/Cart")]
    public async Task<IActionResult> Cart()
    {
        return View(await _cartService.GetCartAsync(HttpContext.Session));
    }

    [HttpGet("/checkout.html")]
    [HttpGet("/checkout-1.html")]
    [HttpGet("/Home/Checkout")]
    public async Task<IActionResult> Checkout()
    {
        var cart = await _cartService.GetCartAsync(HttpContext.Session);
        if (cart.Items.Count == 0)
        {
            return RedirectToAction(nameof(Cart));
        }

        return View(new CheckoutViewModel { Cart = cart });
    }

    [HttpPost("/Checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var cart = await _cartService.GetCartAsync(HttpContext.Session);
        if (cart.Items.Count == 0)
        {
            return RedirectToAction(nameof(Cart));
        }

        model.Cart = cart;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var order = new Order
        {
            CustomerName = model.CustomerName.Trim(),
            Phone = model.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            ShippingAddress = model.ShippingAddress.Trim(),
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatuses.Pending,
            Items = cart.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Name,
                ImageUrl = item.ImageUrl,
                UnitPrice = item.Price,
                Quantity = item.Quantity
            }).ToList()
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        _cartService.Clear(HttpContext.Session);

        return RedirectToAction(nameof(OrderPlaced), new { id = order.Id });
    }

    [HttpGet("/Home/OrderPlaced/{id:int}")]
    public async Task<IActionResult> OrderPlaced(int id)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == id);
        return order is null ? NotFound() : View(order);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
