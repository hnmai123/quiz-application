
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizApp.Pages;

public class LoginModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();
    public string? ReturnUrl { get; set; }
    public class InputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("/Index");
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.Username == "admin" && Input.Password == "password")
        {
            // Simulate successful login
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Input.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = Input.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Set cookie expiration
            };
            await HttpContext.SignInAsync("CookieAuth", principal, authProperties);
            Console.WriteLine(returnUrl);
            return LocalRedirect(returnUrl);
        }
        else if (Input.Username == "tester" && Input.Password == "password")
        {
            // Simulate successful login for tester
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Input.Username),
                new Claim(ClaimTypes.Role, "Tester")
            };
            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = Input.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Set cookie expiration
            };
            await HttpContext.SignInAsync("CookieAuth", principal, authProperties);
            return LocalRedirect(returnUrl);
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
    public async Task<IActionResult> OnPostLogoutAsync()
    {
        Console.WriteLine("Logout called");
        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToPage("/Login");
    }
} 
