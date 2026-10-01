namespace WebBanHang.Models;

public class ShopViewModel
{
    public List<Category> Categories { get; set; } = [];
    public List<Product> Products { get; set; } = [];
    public int? SelectedCategoryId { get; set; }
}
