// Licenciado para a .NET Foundation sob um ou mais contratos.
// A .NET Foundation licencia este arquivo sob a licença MIT.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ProjetoBackend.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
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
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     Esta API dá suporte à infraestrutura padrão da interface do usuário do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode mudar ou ser removida em versões futuras.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     Esta API dá suporte à infraestrutura padrão da interface do usuário do ASP.NET Core Identity e não se destina a ser usada
        ///     diretamente do seu código. Esta API pode mudar ou ser removida em versões futuras.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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
            [Required(ErrorMessage = "O Campo E-mail é Obrigatório")]
            [EmailAddress(ErrorMessage = "Formato do E-mail Inválido!")]
            public string Email { get; set; }

            /// <summary>
            ///     Esta API dá suporte à infraestrutura padrão da interface do usuário do ASP.NET Core Identity e não se destina a ser usada
            ///     diretamente do seu código. Esta API pode mudar ou ser removida em versões futuras.
            /// </summary>
            [Required(ErrorMessage = "O Campo Senha é obrigatório")]
            [DataType(DataType.Password, ErrorMessage = "O formato da senha é inválido")]
            public string Password { get; set; }

            /// <summary>
            ///     Esta API dá suporte à infraestrutura padrão da interface do usuário do ASP.NET Core Identity e não se destina a ser usada
            ///     diretamente do seu código. Esta API pode mudar ou ser removida em versões futuras.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Limpar o cookie externo existente para garantir um processo de login limpo
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Isso não conta falhas de login para bloqueio de conta
                // Para habilitar falhas de senha para acionar bloqueio de conta, defina lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuário Logado");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Conta de usuário bloqueada.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Falha no Login, tente Novamente!");
                    return Page();
                }
            }

            // Se chegarmos aqui, algo falhou, exibir formulário novamente
            return Page();
        }
    }
}

