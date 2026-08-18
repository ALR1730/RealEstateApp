using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Extensions;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserActivityService _userActivityService;

        public AccountController(
            IAccountService accountService,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IUserActivityService userActivityService)
        {
            _accountService = accountService;
            _signInManager = signInManager;
            _userManager = userManager;
            _userActivityService = userActivityService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null, string? remoteError = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }

            if (!string.IsNullOrEmpty(remoteError))
            {
                ModelState.AddModelError(string.Empty, $"Error de autenticación externa: {remoteError}");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = await _userManager.FindByEmailAsync(vm.Email) 
                       ?? await _userManager.FindByNameAsync(vm.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "No existe una cuenta asociada a este correo o usuario.");
                return View(vm);
            }

            if (!user.EmailConfirmed && !await _userManager.IsInRoleAsync(user, Roles.Developer.ToString()))
            {
                ModelState.AddModelError(string.Empty, "Su cuenta no ha sido activada por correo electrónico. Revise su bandeja de entrada.");
                return View(vm);
            }

            if (!user.IsActiveUser())
            {
                return RedirectToAction(nameof(PendingActivation));
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, vm.Password, vm.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await _userActivityService.LogActivityAsync(user.Id, "Inicio de Sesión", "Inicio de sesión exitoso en la plataforma", "bi-box-arrow-in-right text-success");
                var isDeveloperUser = await _userManager.IsInRoleAsync(user, Roles.Developer.ToString());
                if (isDeveloperUser)
                {
                    return RedirectToAction(nameof(DeveloperPanel));
                }
                return RedirectToLocal(vm.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(PendingActivation));
            }

            ModelState.AddModelError(string.Empty, "Credenciales incorrectas. Verifique su usuario/correo y contraseña.");
            return View(vm);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (vm.UserType == "Developer")
            {
                ModelState.AddModelError(string.Empty, "El registro de cuentas de Desarrollador está reservado exclusivamente para los administradores del sistema.");
                return View(vm);
            }

            var origin = $"{Request.Scheme}://{Request.Host}";
            var selectedRole = vm.UserType == "Agent" ? Roles.Agent.ToString() :
                               vm.UserType == "Owner" ? Roles.Owner.ToString() : 
                               Roles.Client.ToString();

            var request = new RegisterRequest
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                UserName = vm.UserName,
                Password = vm.Password,
                ConfirmPassword = vm.ConfirmPassword,
                Phone = vm.Phone
            };

            var response = await _accountService.RegisterUserAsync(request, selectedRole, origin, "Account/ConfirmEmail");

            if (response.HasError)
            {
                ModelState.AddModelError(string.Empty, response.Error ?? "Error en el registro de usuario.");
                return View(vm);
            }

            if (selectedRole == Roles.Agent.ToString())
            {
                return RedirectToAction(nameof(PendingActivation));
            }

            ViewBag.Message = $"¡Registro exitoso! Hemos enviado un enlace de confirmación a su correo ({vm.Email}). Por favor verifique su cuenta para poder iniciar sesión.";
            return View("ConfirmEmailResult");
        }

        [Authorize(Roles = "Developer,Admin")]
        [HttpGet]
        public async Task<IActionResult> DeveloperPanel()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [Authorize(Roles = "Developer,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> GenerateJwtToken([FromBody] AuthenticationRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new AuthenticationResponse { HasError = true, Error = "Debe ingresar el correo y la contraseña." });
            }

            var response = await _accountService.AuthenticateAsync(request);
            if (response.HasError)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var result = await _accountService.ConfirmAccountAsync(userId, token);
            ViewBag.Message = result;
            return View("ConfirmEmailResult");
        }

        [HttpGet]
        public IActionResult PendingActivation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error de autenticación externa: {remoteError}");
                return View("Login", new LoginViewModel { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(PendingActivation));
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "No se pudo obtener el correo electrónico desde Google.");
                return View("Login", new LoginViewModel { ReturnUrl = returnUrl });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Error al crear cuenta con Google.");
                    return View("Login", new LoginViewModel { ReturnUrl = returnUrl });
                }
                await _userManager.AddToRoleAsync(user, Roles.Client.ToString());
            }

            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToLocal(returnUrl);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var profileVm = await _accountService.GetProfileAsync(userId);
            if (profileVm == null)
            {
                return NotFound();
            }

            return View(profileVm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(EditProfileViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || userId != vm.Id)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _accountService.UpdateProfileAsync(vm);

            if (result.HasError)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Ocurrió un error al actualizar su perfil.");
                return View(result);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["SuccessMessage"] = "Su perfil ha sido actualizado exitosamente.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Activity()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var activities = await _userActivityService.GetRecentActivitiesAsync(userId, 50);
            return View(activities);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
