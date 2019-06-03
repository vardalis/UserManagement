using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using UserManagement.Entities;
using UserManagement.Extensions;
using UserManagement.Helpers;
using UserManagement.Services;
using UserManagement.Services.Helpers;
using UserManagement.Models;

namespace UserManagement.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IAccountService _accountService;
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly IMapper _mapper;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IConfiguration configuration,
            ILogger<AccountController> logger,
            IAccountService accountService, 
            IOptions<JwtOptions> jwtOptions,
            IMapper mapper)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _configuration = configuration;
            _logger = logger;
            _accountService = accountService;
            _jwtOptions = jwtOptions;
            _mapper = mapper;
        }

        // POST api/auth/login
        // [HttpPost("[action]")]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody]CredentialsModel credentialsVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponse { success = true, token = "", message = ModelState.ToString() });

            AuthenticateServiceResult result = await _accountService.AuthenticateAsync(credentialsVM.Email, credentialsVM.Password, _jwtOptions.Value.ExpiresInMinutes,
                _jwtOptions.Value.ValidIssuer, _jwtOptions.Value.ValidAudience, _jwtOptions.Value.SymmetricSecurityKey);

            //if (result == null)
            //    return BadRequest(new AuthResponse { success = false, token = "", message = "Username or password is incorrect" });

            if (result == null)
                return Unauthorized();

            return new AuthResponse {
                success = true,
                token = result.Token,
                expiresInMinutes = _jwtOptions.Value.ExpiresInMinutes,
                message = "Success!",
                email = credentialsVM.Email,
                role = result.Role
            };
        }

        // TODO: Check username/email exists

        // [HttpPost("[action]")]
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel registerVm)
        {
            // get reCAPTHCA key from appsettings.json
            string reCaptchaKey = _configuration.GetSection("GoogleReCaptcha:key").Value;

            if (!ModelState.IsValid)
                BadRequest(ModelState);

            if (!ReCaptchaPassed(
                registerVm.Recaptcha,
                _configuration.GetSection("GoogleReCaptcha:secret").Value,
                _logger
            ))
                return Forbid();

            if (await _userManager.FindByEmailAsync(registerVm.Email) != null)
            {
                ModelState.AddModelError("Email", "Email already in use.");
                return BadRequest(ModelState);
            }

            ApplicationUser user = _mapper.Map<ApplicationUser>(registerVm);

            var result = await _userManager.CreateAsync(user, registerVm.Password);
            await _userManager.AddToRoleAsync(user, "applicant");

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                bool.TryParse(_configuration.GetSection("Emailing:Enable").Value, out bool enableEmail);

                if (enableEmail)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.EmailConfirmationLink(Request.Scheme, Request.Host, Request.PathBase,
                    //    user.Id, code, );

                    var callbackUrl = GenerateEmailConfirmationLink(user.Id, code);

                    try
                    {
                        await _emailSender.SendEmailAsync(registerVm.Email, "Email Confirmation", "PleaseConfirmAccount \n" + callbackUrl);
                    }
                    catch (Exception)
                    {
                        await _userManager.DeleteAsync(user);
                        return StatusCode(503);
                    }

                    return Ok();
                }
            }

            return BadRequest();
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // throw new ApplicationException($"Unable to load user with ID '{userId}'.");
                return BadRequest();
            }

            var result = await _userManager.ConfirmEmailAsync(user, HttpUtility.UrlDecode(code));

            if (result.Succeeded)
                return Ok();

            return BadRequest();
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return Ok();
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                // var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
                // var callbackUrl = Request.Scheme + "://" + Request.Host + "/account/reset-password?userId=" + user.Id + "&code=" + code;
                var callbackUrl = GeneratePasswordResetLink(code);

                // await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                //   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                await _emailSender.SendEmailAsync(model.Email, "Reset password", "Please reset password \n" + callbackUrl);

                return Ok();
            }

            // If we got this far, something failed, redisplay form
            return BadRequest();
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Ok();
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest();
        }

        private string GenerateEmailConfirmationLink(string userId, string code)
        {
            string url = Request.Scheme + "://" + Request.Host + Request.PathBase + "/account/confirm-email";
            var queryParams = new Dictionary<string, string>
            {
                {"userId", userId },
                {"code", code }
            };

            return QueryHelpers.AddQueryString(url, queryParams);
        }

        private string GeneratePasswordResetLink(string code)
        {
            string url = Request.Scheme + "://" + Request.Host + Request.PathBase + "/account/reset-password";
            var queryParams = new Dictionary<string, string>
            {
                {"code", code }
            };

            return QueryHelpers.AddQueryString(url, queryParams);
        }

        private static bool ReCaptchaPassed(string gRecaptchaResponse, string secret, ILogger logger)
        {
            HttpClient httpClient = new HttpClient();
            var res = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={gRecaptchaResponse}").Result;
            if (res.StatusCode != HttpStatusCode.OK)
            {
                logger.LogError("Error while sending request to ReCaptcha");
                return false;
            }

            string JSONres = res.Content.ReadAsStringAsync().Result;
            dynamic JSONdata = JObject.Parse(JSONres);
            if (JSONdata.success != "true")
            {
                return false;
            }

            return true;
        }
    }
}
