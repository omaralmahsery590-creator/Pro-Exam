using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pro_exam.Authorization;
using pro_exam.DataBaseContext;
using pro_exam.email_service;
using pro_exam.ViewModel;

namespace pro_exam.Controllers
{
    public class AuthController : Controller//, IAuthentication<UserViewModel>
    {
        public IAuthentication<UserViewModel> Authentication { get; }// Dependency injection for authentication and database context

        private readonly AppDBcontext _context;// Dependency injection for database context

        private readonly EmailService mail;// Dependency injection for email service
        public AuthController(IAuthentication<UserViewModel> authentication, AppDBcontext context, EmailService mail)// Constructor to initialize dependencies
        {
            Authentication = authentication;// Initialize authentication dependency
            _context = context;// Initialize database context dependency
            this.mail = mail;// Initialize email service dependency
        }


        [HttpGet]// Action method to display the login view
        public IActionResult Login()// Display the login view
        {
            return View();
        }
        [HttpPost]// Action method to handle login form submission
        public async Task<IActionResult> Login(string Email, string OTP)
        {
            try
            {
                // Retrieve the user by EmployeeNumber
                //var hashedpassword = BCrypt.Net.BCrypt.HashPassword(loginModel.Password);
                var user = await _context.Users.Where(x => x.Email == Email && x.OTP == OTP).SingleOrDefaultAsync();// Find the user with the provided email and OTP
                if (user == null)
                {
                    ViewBag.ErrorMessage = "Email or Password not Correct";
                    return View();
                }

                var datamapping = new UserViewModel// Map the user data to the UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name
                };

                if (datamapping is not null)
                {
                    datamapping.Tokcen = Authentication.GetJsonWebToken(datamapping);
                }

                HttpContext.Session.SetString("token", datamapping.Tokcen);

                return RedirectToAction("ExamDashBoard", "Exam");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Email or Password not Correct";
                return View();
            }

        }
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("token");

            //add log
            return RedirectToAction("Login", "Auth");
        }



        [HttpPost]
        public async Task<IActionResult> UserCode()
        {
            var User = await _context.Users.FindAsync(1);

            if (User == null || string.IsNullOrWhiteSpace(User.Email))// Check if the user exists and has a valid email address
            {
                ViewBag.ErrorMessage = "User not found or email is not configured.";
                return View("Login");
            }

            User.OTP = GenerateOTP(6);
            User.OTPExpirationTime = DateTime.Now.AddMinutes(2);
            _context.Users.Update(User);
            await _context.SaveChangesAsync();
            await mail.SendVerificationEmailAsync(User.Email, User.OTP);
            return RedirectToAction("Login", "Auth");
        }

        // Generate OTP
        private string GenerateOTP(int length = 6)
        {
            var random = new Random();
            return new string(Enumerable.Range(0, length).Select(_ => (char)('0' + random.Next(0, 10))).ToArray());
        }

    }
}