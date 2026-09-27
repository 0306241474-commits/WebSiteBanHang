using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;

namespace WebBanHang.Controllers;

public class HomeController : Controller
{
    [HttpGet("/")]
    [HttpGet("/index.html")]
    [HttpGet("/Home/Index")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("/shop.html")]
    [HttpGet("/Home/Shop")]
    public IActionResult Shop()
    {
        return View();
    }

    [HttpGet("/product-details.html")]
    [HttpGet("/Home/Details")]
    public IActionResult Details()
    {
        return View();
    }

    [HttpGet("/cart.html")]
    [HttpGet("/Home/Cart")]
    public IActionResult Cart()
    {
        return View();
    }

    [HttpGet("/checkout.html")]
    [HttpGet("/checkout-1.html")]
    [HttpGet("/Home/Checkout")]
    public IActionResult Checkout()
    {
        return View();
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
