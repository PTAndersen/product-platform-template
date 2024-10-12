using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PPTWebApp.Data.Models;

namespace PPTWebApp.Controllers;
[Route("account")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string returnUrl = "/")
    {
        await _signInManager.SignOutAsync();

        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }
}


