using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Thrift.Models;
using Thrift.Repositories;
using Thrift.ViewModels;

namespace Thrift.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly UserRepository _userRepository;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            UserRepository userRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Email is already taken.");
                    return View(model);
                }

                var user = new User
                {
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true, // For development, set to true
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Add to default role
                    await _userManager.AddToRoleAsync(user, "User");

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            // Clear existing external cookie
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userRepository.GetByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    // Update last login time
                    await _userRepository.UpdateLastLoginAsync(user.Id);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                if (result.IsLockedOut)
                {
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                PreferredCurrency = user.PreferredCurrency,
                DefaultTheme = user.DefaultTheme
            };

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                PreferredCurrency = user.PreferredCurrency,
                DefaultTheme = user.DefaultTheme
            };

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                PreferredCurrency = user.PreferredCurrency,
                DefaultTheme = user.DefaultTheme
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.PreferredCurrency = model.PreferredCurrency;
            user.DefaultTheme = model.DefaultTheme;

            await _userRepository.UpdateAsync(user.Id, user);

            TempData["StatusMessage"] = "Your profile has been updated";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Your password has been changed.";

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup2FA()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = await LoadSharedKeyAndQrCodeUriAsync(user);
            return Json(new { success = true, data = model });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enable2FA(Setup2FAViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                var loadSharedKeyAndQrCodeUriModel = await LoadSharedKeyAndQrCodeUriAsync(user);
                return Json(new { success = false, message = "Invalid verification code.", data = loadSharedKeyAndQrCodeUriModel });
            }

            // Strip spaces and hyphens
            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                var loadSharedKeyAndQrCodeUriModel = await LoadSharedKeyAndQrCodeUriAsync(user);
                return Json(new { success = false, message = "Verification code is invalid.", data = loadSharedKeyAndQrCodeUriModel });
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            
            // Generate recovery codes
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            return Json(new 
            { 
                success = true, 
                message = "Your authenticator app has been verified. Two-factor authentication is now enabled.",
                recoveryCodes = recoveryCodes?.ToArray()
            });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable2FA()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                return Json(new { success = false, message = "Unexpected error occurred disabling 2FA." });
            }

            return Json(new { success = true, message = "2FA has been disabled. You can re-enable 2FA when you setup an authenticator app" });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get2FAStatus()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new TwoFactorAuthenticationViewModel
            {
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user)
            };

            return Json(model);
        }        private async Task<Setup2FAViewModel> LoadSharedKeyAndQrCodeUriAsync(User user)
        {
            // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var qrCodeUri = GenerateQrCodeUri(user.Email ?? string.Empty, unformattedKey ?? string.Empty);

            var model = new Setup2FAViewModel
            {
                SharedKey = FormatKey(unformattedKey ?? string.Empty),
                AuthenticatorUri = qrCodeUri,
                QrCodeSetupImageUrl = GenerateQrCodeUrl(qrCodeUri)
            };

            return model;
        }

        private string GenerateQrCodeUrl(string qrCodeUri)
        {
            // Use Google Charts API for QR code generation
            var encodedUri = Uri.EscapeDataString(qrCodeUri);
            return $"https://chart.googleapis.com/chart?chs=200x200&chld=M|0&cht=qr&chl={encodedUri}";
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            return string.Format(
                AuthenticatorUriFormat,
                UrlEncoder.Default.Encode("Thrift"),
                UrlEncoder.Default.Encode(email),
                unformattedKey);
        }
    }
}