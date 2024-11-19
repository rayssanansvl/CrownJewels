// Licenciado para a .NET Foundation sob um ou mais contratos.
// A .NET Foundation licencia este arquivo sob a licença MIT.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace ProjetoBackend.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginWith2faModel> _logger;

        public LoginWith2faModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<LoginWith2faModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        ///     Esta API dá suporte à infraestrutura padrão da interface do usuário do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode mudar ou ser removida em versões futuras.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     Esta API dá suporte à infraestrutura padrão da interface do usuário do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode mudar ou ser removida em versões futuras.
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        ///     Esta API dá suporte à infraestrutura padrão da interface do usuário do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode mudar ou ser removida em versões futuras.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     Esta API dá suporte à infraestrutura padrão da interface do usuário do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode mudar ou ser removida em versões futuras.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     Esta API dá suporte à infraestrutura padrão da interface do usuário do ASP.NET Core Identity e não se destina a ser usada
            ///     diretamente do seu código. Esta API pode mudar ou ser removida em versões futuras.
            /// </summary>
            [Required]
            [StringLength(7, ErrorMessage = "O {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Código do autenticador")]
            public string TwoFactorCode { get; set; }

            /// <summary>
            ///     Esta API dá suporte à infraestrutura padrão da interface do usuário do ASP.NET Core Identity e não se destina a ser usada
            ///     diretamente do seu código. Esta API pode mudar ou ser removida em versões futuras.
            /// </summary>
            [Display(Name = "Lembrar deste dispositivo")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            // Certifique-se de que o usuário passou pela tela de nome de usuário e senha primeiro
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"Não foi possível carregar o usuário para autenticação de dois fatores.");
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Não foi possível carregar o usuário para autenticação de dois fatores.");
            }

            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);

            var userId = await _userManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuário com ID '{UserId}' logado com autenticação de dois fatores.", user.Id);
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("Conta do usuário com ID '{UserId}' bloqueada.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Código do autenticador inválido inserido para o usuário com ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Código do autenticador inválido.");
                return Page();
            }
        }
    }
}

