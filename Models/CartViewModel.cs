namespace WebBanHang.Models;

public class CartViewModel
{
    public List<CartLineViewModel> Items { get; set; } = [];
    public decimal Total => Items.Sum(item => item.LineTotal);
}

public class CartLineViewModel
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => Price * Quantity;
}
