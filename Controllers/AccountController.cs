using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KioskManagementWebApp.Data;
using KioskManagementWebApp.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;

namespace KioskManagementWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
            {
                return RedirectToAction("Index", "Tuition");
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return View();
            }

            username = username.Trim();
            password = password.Trim();

            var account = _context.Accounts
                .FirstOrDefault(a => a.Username == username);

            if (account == null || !BCrypt.Net.BCrypt.Verify(password, account.Password))
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
                ViewBag.Username = username;
                return View();
            }

            HttpContext.Session.SetString("Username", account.Username);
            HttpContext.Session.SetString("Role", account.Role);
            if (account.Role == "Student")
            {
                HttpContext.Session.SetString("StudentId", account.StudentId ?? "");
            }

            return RedirectToAction("Index", "Tuition");
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
            {
                return RedirectToAction("Index", "Tuition");
            }
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string username, string password, string role, string studentId)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin bắt buộc.";
                return View();
            }

            username = username.Trim();
            password = password.Trim();
            studentId = studentId?.Trim();

            if (_context.Accounts.Any(a => a.Username == username))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                ViewBag.Username = username;
                ViewBag.Role = role;
                ViewBag.StudentId = studentId;
                return View();
            }

            if (role == "Student" && string.IsNullOrEmpty(studentId))
            {
                ViewBag.Error = "Vui lòng nhập mã sinh viên.";
                ViewBag.Username = username;
                ViewBag.Role = role;
                return View();
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var newAccount = new Account
            {
                Username = username,
                Password = hashedPassword,
                Role = role,
                StudentId = role == "Student" ? studentId : null
            };

            try
            {
                _context.Accounts.Add(newAccount);
                _context.SaveChanges();
                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Đăng ký thất bại: " + ex.Message;
                ViewBag.Username = username;
                ViewBag.Role = role;
                ViewBag.StudentId = studentId;
                return View();
            }
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}