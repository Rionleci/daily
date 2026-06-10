using System.ComponentModel.DataAnnotations;

namespace Daily.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Benutzername ist erforderlich.")]
    [Display(Name = "Benutzername")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Passwort ist erforderlich.")]
    [DataType(DataType.Password)]
    [Display(Name = "Passwort")]
    public string Password { get; set; } = "";

    [Display(Name = "Angemeldet bleiben")]
    public bool RememberMe { get; set; } = true;

    public string? ReturnUrl { get; set; }
}
