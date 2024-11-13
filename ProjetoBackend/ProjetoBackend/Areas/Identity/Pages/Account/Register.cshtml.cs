// Licenciado para a .NET Foundation sob um ou mais acordos.
// A .NET Foundation licencia este arquivo sob a licença MIT.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace ProjetoBackend.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     Esta API oferece suporte à infraestrutura padrão da UI do ASP.NET Core Identity e não é destinada ao uso
        ///     direto no seu código. Esta API pode mudar ou ser removida em versões futuras.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     Esta API oferece suporte à infraestrutura padrão da UI do ASP.NET Core Identity e não é destinada ao uso
        ///     direto no seu código. Esta API pode mudar ou ser removida em versões futuras.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     Esta API oferece suporte à infraestrutura padrão da UI do ASP.NET Core Identity e não é destinada ao uso
        ///     direto no seu código. Esta API pode mudar ou ser removida em versões futuras.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     Esta API oferece suporte à infraestrutura padrão da UI do ASP.NET Core Identity e não é destinada ao uso
        ///     direto no seu código. Esta API pode mudar ou ser removida em versões futuras.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     Esta API oferece suporte à infraestrutura padrão da UI do ASP.NET Core Identity e não é destinada ao uso
            ///     direto no seu código. Esta API pode mudar ou ser removida em versões futuras.
            /// </summary>
            [Required(ErrorMessage = "O Campo E-mail é Obrigatório")]
            [EmailAddress(ErrorMessage = "Formato do E-mail Inválido!")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     Esta API oferece suporte à infraestrutura padrão da UI do ASP.NET Core Identity e não é destinada ao uso
            ///     direto no seu código. Esta API pode mudar ou ser removida em versões futuras.
            /// </summary>
            [Required(ErrorMessage = "O Campo Senha é obrigatório")]
            [StringLength(100, ErrorMessage = "O {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Password { get; set; }

            /// <summary>
            ///     Esta API oferece suporte à infraestrutura padrão da UI do ASP.NET Core Identity e não é destinada ao uso
            ///     direto no seu código. Esta API pode mudar ou ser removida em versões futuras.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar senha")]
            [Compare("Password", ErrorMessage = "A senha e a confirmação de senha não coincidem.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuário criou uma nova conta com senha.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirme seu email",
                        $"Por favor, confirme sua conta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Se chegamos até aqui, algo falhou, reexibir o formulário
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Não é possível criar uma instância de '{nameof(IdentityUser)}'. " +
                    $"Certifique-se de que '{nameof(IdentityUser)}' não é uma classe abstrata e possui um construtor sem parâmetros, ou, alternativamente, " +
                    $"sobrescreva a página de registro em /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("A UI padrão exige um repositório de usuários com suporte a email.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}

