using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your phone number.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(256)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Please enter your shipping address.")]
    [StringLength(200)]
    public string ShippingAddress { get; set; } = string.Empty;

    public CartViewModel Cart { get; set; } = new();
}
