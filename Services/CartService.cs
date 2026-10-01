using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;

namespace WebBanHang.Services;

public class CartService
{
    private const string SessionKey = "ShoppingCart";
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartViewModel> GetCartAsync(ISession session)
    {
        var cartItems = ReadCart(session);
        if (cartItems.Count == 0)
        {
            return new CartViewModel();
        }

        var productIds = cartItems.Select(item => item.ProductId).ToArray();
        var products = await _context.Products
            .Where(product => product.IsActive && productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id);

        return new CartViewModel
        {
            Items = cartItems
                .Where(item => products.ContainsKey(item.ProductId))
                .Select(item =>
                {
                    var product = products[item.ProductId];
                    return new CartLineViewModel
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        ImageUrl = product.ImageUrl,
                        Price = product.Price,
                        Quantity = item.Quantity
                    };
                })
                .ToList()
        };
    }

    public async Task<bool> AddAsync(ISession session, int productId, int quantity)
    {
        if (quantity is < 1 or > 99 ||
            !await _context.Products.AnyAsync(product => product.Id == productId && product.IsActive))
        {
            return false;
        }

        var items = ReadCart(session);
        var existingItem = items.FirstOrDefault(item => item.ProductId == productId);
        if (existingItem is null)
        {
            items.Add(new CartItem { ProductId = productId, Quantity = quantity });
        }
        else
        {
            existingItem.Quantity = Math.Min(99, existingItem.Quantity + quantity);
        }

        SaveCart(session, items);
        return true;
    }

    public void Update(ISession session, int productId, int quantity)
    {
        var items = ReadCart(session);
        var item = items.FirstOrDefault(cartItem => cartItem.ProductId == productId);
        if (item is null)
        {
            return;
        }

        if (quantity <= 0)
        {
            items.Remove(item);
        }
        else
        {
            item.Quantity = Math.Min(quantity, 99);
        }

        SaveCart(session, items);
    }

    public void Remove(ISession session, int productId)
    {
        var items = ReadCart(session);
        items.RemoveAll(item => item.ProductId == productId);
        SaveCart(session, items);
    }

    public void Clear(ISession session)
    {
        session.Remove(SessionKey);
    }

    private static List<CartItem> ReadCart(ISession session)
    {
        var json = session.GetString(SessionKey);
        return string.IsNullOrEmpty(json)
            ? []
            : JsonSerializer.Deserialize<List<CartItem>>(json) ?? [];
    }

    private static void SaveCart(ISession session, List<CartItem> items)
    {
        session.SetString(SessionKey, JsonSerializer.Serialize(items));
    }
}
