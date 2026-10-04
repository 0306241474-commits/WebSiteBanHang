using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;

namespace WebBanHang.Data;

public static class StoreDataSeeder
{
    private static readonly (string Name, string LegacyName, string Description)[] Categories =
    [
        ("Women's Tops", "Áo nữ", "Fashionable tops for women."),
        ("Dresses & Skirts", "Đầm váy", "Dresses and skirts for every occasion."),
        ("Bags", "Túi xách", "Stylish and practical bags."),
        ("Shoes", "Giày dép", "Shoes for everyday style."),
        ("Accessories", "Phụ kiện", "Accessories to complete your look.")
    ];

    private static readonly (
        string Name,
        string LegacyName,
        string CategoryName,
        string LegacyCategoryName,
        decimal Price,
        string Description,
        int ImageNumber)[] Products =
    [
        ("Elegant Linen Shirt", "Áo sơ mi linen thanh lịch", "Women's Tops", "Áo nữ", 459000m, "A breathable linen shirt that's easy to style.", 1),
        ("Puff-Sleeve Blouse", "Áo kiểu tay phồng", "Women's Tops", "Áo nữ", 389000m, "A feminine puff-sleeve design for work or casual outings.", 2),
        ("Basic Cotton T-Shirt", "Áo thun cotton basic", "Women's Tops", "Áo nữ", 229000m, "A soft cotton T-shirt with an easy-to-wear classic fit.", 3),
        ("Floral Midi Dress", "Đầm midi hoa nhí", "Dresses & Skirts", "Đầm váy", 629000m, "A light midi dress with a delicate floral print.", 4),
        ("Square-Neck Party Dress", "Đầm dự tiệc cổ vuông", "Dresses & Skirts", "Đầm váy", 799000m, "An elegant square-neck dress for special occasions.", 5),
        ("A-Line Skirt", "Chân váy chữ A", "Dresses & Skirts", "Đầm váy", 429000m, "An A-line skirt that pairs easily with a variety of tops.", 6),
        ("Mini Shoulder Bag", "Túi đeo vai mini", "Bags", "Túi xách", 559000m, "A compact shoulder bag with a minimalist design.", 7),
        ("Canvas Tote Bag", "Túi tote canvas", "Bags", "Túi xách", 319000m, "A spacious tote bag for everyday use.", 8),
        ("Mary Jane Flats", "Giày búp bê quai ngang", "Shoes", "Giày dép", 489000m, "Comfortable Mary Jane flats for everyday wear.", 9),
        ("Strappy Sandals", "Sandal quai mảnh", "Shoes", "Giày dép", 519000m, "Lightweight strappy sandals that pair beautifully with dresses.", 10),
        ("Silk Scarf", "Khăn choàng lụa", "Accessories", "Phụ kiện", 279000m, "A soft silk scarf to add a finishing touch to any outfit.", 11),
        ("Slim Belt", "Thắt lưng bản nhỏ", "Accessories", "Phụ kiện", 199000m, "A slim belt with a refined, elegant design.", 12)
    ];

    public static async Task SeedCatalogAsync(ApplicationDbContext context)
    {
        var categoryNames = Categories
            .SelectMany(category => new[] { category.Name, category.LegacyName })
            .ToArray();
        var categories = await context.Categories
            .Where(category => categoryNames.Contains(category.Name))
            .ToListAsync();

        foreach (var (name, legacyName, description) in Categories)
        {
            var category = categories.FirstOrDefault(item => item.Name == name)
                ?? categories.FirstOrDefault(item => item.Name == legacyName);

            if (category is null)
            {
                category = new Category();
                categories.Add(category);
                context.Categories.Add(category);
            }

            category.Name = name;
            category.Description = description;
            category.DisplayOrder = Array.IndexOf(Categories, (name, legacyName, description)) + 1;
            category.IsActive = true;
        }

        await context.SaveChangesAsync();

        var categoriesByName = categories.ToDictionary(category => category.Name);
        var productNames = Products
            .SelectMany(product => new[] { product.Name, product.LegacyName })
            .ToArray();
        var products = await context.Products
            .Where(product => productNames.Contains(product.Name))
            .ToListAsync();

        foreach (var (name, legacyName, categoryName, _, price, description, imageNumber) in Products)
        {
            var product = products.FirstOrDefault(item => item.Name == name)
                ?? products.FirstOrDefault(item => item.Name == legacyName);

            if (product is null)
            {
                product = new Product();
                products.Add(product);
                context.Products.Add(product);
            }

            product.Name = name;
            product.CategoryId = categoriesByName[categoryName].Id;
            product.Price = price;
            product.Description = description;
            product.ImageUrl = $"/img/product-img/product-{imageNumber}.jpg";
            product.IsActive = true;
        }

        var translatedNames = Products.ToDictionary(product => product.LegacyName, product => product.Name);
        var legacyProductNames = translatedNames.Keys.ToArray();
        var orderItems = await context.OrderItems
            .Where(item => legacyProductNames.Contains(item.ProductName))
            .ToListAsync();

        foreach (var item in orderItems)
        {
            item.ProductName = translatedNames[item.ProductName];
        }

        var translatedStatuses = new Dictionary<string, string>
        {
            ["Chờ xác nhận"] = OrderStatuses.Pending,
            ["Đang xử lý"] = OrderStatuses.Processing,
            ["Đang giao"] = OrderStatuses.Shipped,
            ["Hoàn thành"] = OrderStatuses.Completed,
            ["Đã hủy"] = OrderStatuses.Cancelled
        };
        var legacyStatuses = translatedStatuses.Keys.ToArray();
        var orders = await context.Orders
            .Where(order => legacyStatuses.Contains(order.Status))
            .ToListAsync();

        foreach (var order in orders)
        {
            order.Status = translatedStatuses[order.Status];
        }

        await context.SaveChangesAsync();
    }
}
