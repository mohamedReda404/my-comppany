using Company.Ali.DAL.Helper;
using Company.Ali.DAL.Models;
using Company.Ali.DAL.Models.Sms;
using Company.Ali.PL.Dtos;
using Company.Ali.PL.Helper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Company.Ali.PL.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMailService _mailService;
        private readonly ITwilioService _twilioService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IMailService mailService, ITwilioService twilioService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mailService = mailService;
            _twilioService = twilioService;
        }

        #region SignUp

        [HttpGet]

        public IActionResult SignUp()
        {
            return View();
        }

        // P@ssW0rd
        [HttpPost]

        public async Task<IActionResult> SignUp(SignUpDto model)
        {
            if (ModelState.IsValid) //  Server Side Validation
            {
              var user = await _userManager.FindByNameAsync(model.UserName);

                if(user is null)
                {
                   user = await _userManager.FindByEmailAsync(model.Email);

                    if(user is null)
                    {
                         user = new AppUser
                        {
                            UserName = model.UserName,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Email = model.Email,
                            IsAgree = model.IsAgree
                        };

                        var result = await _userManager.CreateAsync(user, model.Password);

                        if (result.Succeeded)
                        {
                            return RedirectToAction("SignIn");
                        }

                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }

                ModelState.AddModelError("", "Invalid SignUp !!");
  
            }


            return View();
        }



        #endregion

        #region SignIn

        [HttpGet]

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> SignIn(SignInDto model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user is not null)
                {
                  var flag = await  _userManager.CheckPasswordAsync(user, model.Password);
                    if (flag)
                    {
                        // Sign In

                     var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                        if (result.Succeeded)
                        {
                            return RedirectToAction(nameof(HomeController.Index), "Home");

                        }

                    }
                }

                ModelState.AddModelError("", "Invalid Login !");
            }


            return View(model);
        }

        #endregion

        #region SignOut
        [HttpGet]

        public new async Task<IActionResult> SignOut()
        {
           await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(SignIn));
        }


        #endregion


        #region Forget Password

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendResetPasswordUrl(ForgetPasswordDto model)
        {

            if (ModelState.IsValid)
            {

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user is not null)
                {
                    // Generate Token

                  var token = await  _userManager.GeneratePasswordResetTokenAsync(user);



                    // Create URL

                 var url = Url.Action("ResetPassword", "Account", new {email = model.Email, token}, Request.Scheme);



                    // Create Email

                    var email = new Email()
                    {
                        To = model.Email,
                        Subject = "Reset Password",
                        Body = url
                    };


                    // Send Email

                    //var flag =  EmailSettings.SendEmail(email);
                       _mailService.SendEmail(email);

                        return RedirectToAction("CheckYourInbox");
                    
                }

            }
            ModelState.AddModelError("", "Invalid Reset Password Operation");
            return View("ForgetPassword", model);
        }

        [HttpPost]
        public async Task<IActionResult> SendResetPasswordSms(ForgetPasswordDto model)
        {

            if (ModelState.IsValid)
            {

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user is not null)
                {
                    // Generate Token

                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);



                    // Create URL

                    var url = Url.Action("ResetPassword", "Account", new { email = model.Email, token }, Request.Scheme);



                    // Create Sms

                    var sms = new Sms()
                    {
                        To = user.PhoneNumber,

                        Body = url
                    };

                    _twilioService.SendSms(sms);
    
                    return RedirectToAction(nameof(CheckYourPhone));

                }

            }
            ModelState.AddModelError("", "Invalid Reset Password Operation");
            return View("ForgetPassword", model);
        }

        [HttpGet]
        public IActionResult CheckYourInbox()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult CheckYourPhone()
        {
            return View();
        }

        #endregion

        #region Reset Password

        [HttpGet]

        public IActionResult ResetPassword(string email, string token)
        {

            TempData["email"] = email;
            TempData["token"] = token;

            return View();
        }

        [HttpPost]

        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (ModelState.IsValid)
            {
                var email = TempData["email"] as string;
                
                var token = TempData["token"] as string;

                if (email is null || token is null) return BadRequest("Invalid Operation");

                var user = await _userManager.FindByEmailAsync(email);

                if(user is not null)
                {
                  var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                    if(result.Succeeded)
                    {
                        return RedirectToAction("SignIn");
                    }
                }

                ModelState.AddModelError("", "Invalid Reset Password Operation");
            }

            return View();
        }

        #endregion


        public IActionResult GoogleLogin()
        {
            var prop = new AuthenticationProperties()
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(prop, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            var cliams = result.Principal.Identities.FirstOrDefault().Claims.Select(
                claim => new
                {
                    claim.Type,
                    claim.Value,
                    claim.Issuer,
                    claim.OriginalIssuer,

                }


                );

            return RedirectToAction("Index", "Home");
        }
    }
}
