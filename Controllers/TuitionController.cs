using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KioskManagementWebApp.Data;
using KioskManagementWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace KioskManagementWebApp.Controllers
{
    public class TuitionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TuitionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Tuition/Index
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
            {
                Console.WriteLine("Index: Role is null or empty, redirecting to Login");
                return RedirectToAction("Login", "Account");
            }

            ViewData["Role"] = role;

            if (role == "Admin")
            {
                var tuitionFees = _context.TuitionFees
                    .Include(t => t.TuitionFeeDetails)
                    .Include(t => t.Student)
                    .ToList();
                Console.WriteLine($"Admin Index: {tuitionFees.Count} records loaded.");
                return View("AdminIndex", tuitionFees);
            }
            else if (role == "Student")
            {
                var studentId = HttpContext.Session.GetString("StudentId");
                if (string.IsNullOrEmpty(studentId))
                {
                    Console.WriteLine("Index: StudentId is null or empty, redirecting to Login");
                    return RedirectToAction("Login", "Account");
                }

                var tuitionFees = _context.TuitionFees
                    .Include(t => t.TuitionFeeDetails)
                    .Include(t => t.Student)
                    .Where(t => t.StudentId == studentId)
                    .Where(t => t.DueDate >= DateTime.Now)
                    .ToList();
                Console.WriteLine($"Student Index for {studentId}: {tuitionFees.Count} records loaded.");
                return View("StudentIndex", tuitionFees);
            }

            Console.WriteLine("Index: Invalid role, redirecting to Login");
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public IActionResult PayTuition(int id)
        {
            var tuitionFee = _context.TuitionFees.FirstOrDefault(t => t.Id == id);
            if (tuitionFee == null)
            {
                return Json(new { success = false, message = "Không tìm thấy học phí." });
            }

            if (tuitionFee.IsPaid)
            {
                return Json(new { success = false, message = "Học phí đã được thanh toán." });
            }

            tuitionFee.IsPaid = true;
            tuitionFee.PaymentDate = new DateTime(2025, 5, 14, 10, 39, 0); // Thời gian hiện tại: 10:39 AM, 14/05/2025
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                paymentDate = tuitionFee.PaymentDate.Value.ToString("dd/MM/yyyy")
            });
        }

        [HttpGet]
        public IActionResult GetTuitionDetails(int id)
        {
            var tuitionFee = _context.TuitionFees
                .Include(t => t.TuitionFeeDetails)
                .Include(t => t.Student)
                .FirstOrDefault(t => t.Id == id);

            if (tuitionFee == null)
            {
                return Json(new { success = false, message = "Không tìm thấy học phí." });
            }

            var tuitionDetails = tuitionFee.TuitionFeeDetails?.Select(d => new
            {
                subjectName = d.SubjectName,
                subjectFee = d.SubjectFee
            }).ToList();
            var result = new
            {
                success = true,
                studentId = tuitionFee.StudentId,
                studentName = tuitionFee.Student?.StudentName ?? "N/A",
                department = tuitionFee.Student?.Department ?? "N/A",
                amount = tuitionFee.Amount.ToString("N2"),
                dueDate = tuitionFee.DueDate.ToString("dd/MM/yyyy"),
                status = tuitionFee.IsPaid ? "Đã thanh toán" : "Chưa thanh toán",
                paymentDate = tuitionFee.PaymentDate.HasValue ? tuitionFee.PaymentDate.Value.ToString("dd/MM/yyyy") : "N/A",
                tuitionDetails = tuitionDetails
            };

            return Json(result);
        }

        public IActionResult OverdueTuition()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Account");
            }

            var currentDate = DateTime.Now; // 11:38 AM +07, 14/05/2025

            if (role == "Admin")
            {
                var overdueTuitions = _context.TuitionFees
                    .Include(t => t.Student)
                    .Include(t => t.TuitionFeeDetails)
                    .Where(t => !t.IsPaid && t.DueDate < currentDate)
                    .ToList();

                Console.WriteLine($"Admin OverdueTuition: {overdueTuitions.Count} records loaded.");
                ViewData["Role"] = role;
                return View("OverdueTuition", overdueTuitions);
            }
            else if (role == "Student")
            {
                var studentId = HttpContext.Session.GetString("StudentId");
                if (string.IsNullOrEmpty(studentId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var overdueTuitions = _context.TuitionFees
                    .Include(t => t.Student)
                    .Include(t => t.TuitionFeeDetails)
                    .Where(t => t.StudentId == studentId && !t.IsPaid && t.DueDate < currentDate)
                    .ToList();

                Console.WriteLine($"Student OverdueTuition for {studentId}: {overdueTuitions.Count} records loaded.");
                ViewData["Role"] = role;
                return View("OverdueTuition", overdueTuitions);
            }

            return RedirectToAction("Login", "Account");
        }
    }
}