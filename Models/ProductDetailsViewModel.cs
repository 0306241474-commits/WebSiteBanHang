namespace WebBanHang.Models;

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = new();
    public List<Product> RelatedProducts { get; set; } = [];
}
