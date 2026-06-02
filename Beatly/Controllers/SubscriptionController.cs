using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Beatly.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public SubscriptionController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> ActivatePremium()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var existingClaims = await _userManager.GetClaimsAsync(user);
            foreach (var claim in existingClaims)
            {
                if (claim.Type == "Premium")
                {
                    await _userManager.RemoveClaimAsync(user, claim);
                }
            }

            await _userManager.AddClaimAsync(user, new Claim("Premium", "True"));
            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Index", "Home");
        }
    }
}