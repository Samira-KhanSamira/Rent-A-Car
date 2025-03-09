using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RentACar01.Models;
using RentACar01.ViewModels;
using RentACar01.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RentACar01.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult RegisterAs()
        {
            return View();
        }


        public IActionResult Register(string userType)
        {
            if (string.IsNullOrEmpty(userType))
            {
                return RedirectToAction("Index", "Home");
            }

            @ViewBag.UserType = userType; // ViewData only works for View().  for Rederection() TempData

            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email or username already exists
                if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
                {
                    ModelState.AddModelError("", " Username already exists.");
                    ViewBag.UserType = model.UserType;
                    return View(model);
                }

                // Hash Password
                string hashedPassword = HashPassword(model.Password);

                User newUser = new User
                {
                    UserType=model.UserType,
                    FullName = model.FullName,
                    DOB = model.DOB,
                    Gender = model.Gender,
                    NID = model.NID,
                    Email = model.Email,
                    PhoneNum = model.PhoneNum,
                    UserName = model.UserName,
                    PasswordHash = hashedPassword,
                    Image = model.Image,
                    CreatedAt = model.CreatedAt,
                    Status = "Pending"
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            return View(model);
        }

        
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);

                if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }

                // Set session or authentication logic (if needed)
                HttpContext.Session.SetString("UserName", user.UserName);
                
                if(user.UserType=="CarOwner")
                {
                    return RedirectToAction("Profile_CarOwner", "CarOwner", new { userName = user.UserName });
                }
                 
                else if (user.UserType == "Customer")
                {
                    return RedirectToAction("Profile_Customer", "Customer", new { userName = user.UserName });
                }
                    
                else if (user.UserType == "HouseOwner")
                {
                    return RedirectToAction("Profile_HouseOwner", "HouseOwner", new { userName = user.UserName });
                }
                
            }
            return View(model);
        }

        // 🔹 Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // 🔹 Password Hashing Function
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // 🔹 Password Verification Function
        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            string enteredHash = HashPassword(enteredPassword);
            return enteredHash == storedHash;
        }
    }
}
