using System.ComponentModel.DataAnnotations;

namespace  catalog.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Потребителското име е задължително")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Потребителското име трябва да е между 3 и 50 символа")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Имейлът е задължителен")]
    [EmailAddress(ErrorMessage = "Невалиден имейл адрес")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Паролата е задължителна")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Паролата трябва да е поне 6 символа")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Потвърдете паролата")]
    [Compare("Password", ErrorMessage = "Паролите не съвпадат")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}