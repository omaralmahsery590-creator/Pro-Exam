using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pro_exam.Authorization;
using pro_exam.DataBaseContext;
using pro_exam.email_service;
using pro_exam.ViewModel;

namespace pro_exam.Controllers
{
    public class AuthController : Controller
    {
        public IAuthentication<UserViewModel> Authentication { get; }

        private readonly AppDBcontext _context;
        private readonly EmailService mail;
        private readonly ILogger<AuthController> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public AuthController(
            IAuthentication<UserViewModel> authentication,
            AppDBcontext context,
            EmailService mail,
            ILogger<AuthController> logger,
            IServiceScopeFactory scopeFactory)
        {
            Authentication = authentication;
            _context = context;
            this.mail = mail;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string OTP)
        {
            try
            {
                var user = await _context.Users
                    .Where(x => x.Email == Email && x.OTP == OTP)
                    .SingleOrDefaultAsync();

                if (user == null)
                {
                    ViewBag.ErrorMessage = "Email or OTP not Correct";
                    return View();
                }

                // فحص صلاحية الـ OTP
                if (user.OTPExpirationTime < DateTime.Now)
                {
                    ViewBag.ErrorMessage = "OTP has expired. Please request a new one.";
                    return View();
                }

                var datamapping = new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name
                };

                datamapping.Tokcen = Authentication.GetJsonWebToken(datamapping);
                HttpContext.Session.SetString("token", datamapping.Tokcen);

                return RedirectToAction("ExamDashBoard", "Exam");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", Email);
                ViewBag.ErrorMessage = "An error occurred. Please try again.";
                return View();
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("token");
            return RedirectToAction("Login", "Auth");
        }


        [HttpPost]
        public async Task<IActionResult> UserCode(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    TempData["ErrorMessage"] = "Please enter your email address before requesting an OTP.";
                    return RedirectToAction("Login", "Auth");
                }

                var user = await _context.Users
                    .Where(x => x.Email == email.Trim())
                    .FirstOrDefaultAsync();

                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                {
                    TempData["ErrorMessage"] = "No account found for that email address.";
                    return RedirectToAction("Login", "Auth");
                }

                // 1. توليد OTP جديد وتحديث قاعدة البيانات
                var newOtp = GenerateOTP(6);
                user.OTP = newOtp;
                user.OTPExpirationTime = DateTime.Now.AddMinutes(2);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // 2. إرسال الإيميل بالخلفية - المستخدم ما رح يستنى
                var userEmail = user.Email;
                _ = Task.Run(async () =>
                {
                    // مهم: لازم نعمل scope جديد لأنه الكونترولر رح يكون انتهى
                    using var scope = _scopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
                    var bgLogger = scope.ServiceProvider.GetRequiredService<ILogger<AuthController>>();

                    try
                    {
                        await emailService.SendVerificationEmailAsync(userEmail, newOtp);
                        bgLogger.LogInformation("OTP email sent successfully to {Email}", userEmail);
                    }
                    catch (Exception ex)
                    {
                        bgLogger.LogError(ex, "Failed to send OTP email to {Email}", userEmail);
                    }
                });

                // 3. رجّع المستخدم فوراً
                TempData["SuccessMessage"] = "OTP sent to your email. Please check your inbox.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OTP");
                TempData["ErrorMessage"] = "An error occurred. Please try again.";
                return RedirectToAction("Login", "Auth");
            }
        }

        // Generate OTP
        private string GenerateOTP(int length = 6)
        {
            var random = new Random();
            return new string(Enumerable.Range(0, length)
                .Select(_ => (char)('0' + random.Next(0, 10))).ToArray());
        }
    }
}
