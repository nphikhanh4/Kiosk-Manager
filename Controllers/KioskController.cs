using Microsoft.AspNetCore.Mvc;
using KioskManagementWebApp.Data;
using KioskManagementWebApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace KioskManagementWebApp.Controllers
{
    public class KioskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KioskController> _logger;

        public KioskController(ApplicationDbContext context, ILogger<KioskController> logger)
        {
            _context = context;
            _logger = logger;

            // Kiểm tra dữ liệu khi khởi tạo controller
            var initialKiosks = _context.Kiosks.ToList();
            _logger.LogInformation($"Initial Kiosk check on startup: [{string.Join(", ", initialKiosks.Select(k => $"Id={k.Id}, Name={k.Name}"))}]");
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
            {
                Console.WriteLine("Index: Role is null or empty, redirecting to Login");
                return RedirectToAction("Login", "Account");
            }

            ViewData["Role"] = role;

            var kiosks = _context.Kiosks
                .Include(k => k.Renters)
                .Where(a => a.Active == true)
                .ToList();

            // Lấy tháng hiện tại định dạng yyyy-MM
            var currentMonth = DateTime.Today.ToString("yyyy-MM");

            // Lấy danh sách Payments chưa thanh toán cho tháng hiện tại
            var unpaidPayments = _context.Payments
                .Where(p => p.PaymentMonth == currentMonth && !p.IsPaid)
                .Select(p => new { p.RenterId, p.Amount })
                .ToList();

            // Tạo danh sách KioskId có Payments chưa thanh toán
            var unpaidKioskIds = _context.Renters
                .Where(r => r.IsActive && unpaidPayments.Select(p => p.RenterId).Contains(r.Id))
                .Select(r => r.KioskId)
                .ToList();

            // Tạo dictionary để lưu Amount theo RenterId
            var unpaidAmounts = unpaidPayments.ToDictionary(p => p.RenterId, p => p.Amount);

            ViewData["UnpaidKioskIds"] = unpaidKioskIds;
            ViewData["UnpaidCount"] = unpaidKioskIds.Count;
            ViewData["UnpaidAmounts"] = unpaidAmounts; // Truyền Amount vào ViewData

            return View(kiosks);
        }

        private void UpdateExpiredRentals()
        {
            var expiredRenters = _context.Renters
                .Where(r => r.RentalEndDate != null && r.RentalEndDate <= DateTime.Today && r.IsActive)
                .ToList();

            foreach (var renter in expiredRenters)
            {
                var kiosk = _context.Kiosks.Find(renter.KioskId);
                if (kiosk != null)
                {
                    kiosk.IsRented = false;
                }
                renter.IsActive = false;
            }

            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult Create(Kiosk kiosk)
        {
            // Kiểm tra trùng Name và Location, chỉ chặn nếu kiosk hiện tại có Active = true
            var existingKiosk = _context.Kiosks
                .Any(k => k.Name.ToLower() == kiosk.Name.ToLower()
                       && k.Location.ToLower() == kiosk.Location.ToLower()
                       && k.Active);
            if (existingKiosk)
            {
                ModelState.AddModelError("Name", "Tên kiosk đã tồn tại tại vị trí này và đang hoạt động.");
            }

            if (ModelState.IsValid)
            {
                kiosk.IsRented = false;
                kiosk.Active = true; // Đặt Active = true cho kiosk mới
                _context.Add(kiosk);
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new
            {
                success = false,
                errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });
        }

        [HttpPost]
        public IActionResult Edit(int id, string name, string location, decimal rentalFee)
        {
            var existingKiosk = _context.Kiosks
                .Include(k => k.Renters)
                .FirstOrDefault(k => k.Id == id);

            if (existingKiosk == null)
            {
                return Json(new { success = false, errors = new[] { "Kiosk không tồn tại." } });
            }

            // Kiểm tra xem Kiosk có Renter đang active không
            var activeRenter = existingKiosk.Renters?.FirstOrDefault(r => r.IsActive);
            if (activeRenter != null)
            {
                return Json(new { success = false, errors = new[] { "Kiosk đang được thuê, không thể chỉnh sửa." } });
            }

            existingKiosk.Name = name;
            existingKiosk.Location = location;
            existingKiosk.RentalFee = rentalFee;

            if (_context.SaveChanges() > 0)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = new[] { "Không thể cập nhật Kiosk." } });
        }

        [HttpPost]
        public IActionResult DeleteKiosk([FromBody] IdRequest request)
        {
            try
            {
                var kiosk = _context.Kiosks
                    .Include(k => k.Renters)
                    .FirstOrDefault(k => k.Id == request.Id);

                if (kiosk == null)
                {
                    return Json(new { success = false, errors = new[] { "Kiosk không tồn tại." } });
                }

                // Đặt Active = false
                kiosk.Active = false;
                _context.Kiosks.Update(kiosk);

                var changes = _context.SaveChanges();
                if (changes > 0)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, errors = new[] { "Không thể xóa kiosk." } });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Lỗi khi xóa kiosk: {ex.InnerException?.Message}");
                return Json(new { success = false, errors = new[] { $"Lỗi hệ thống: {ex.InnerException?.Message ?? ex.Message}" } });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi hệ thống: {ex.Message}");
                return Json(new { success = false, errors = new[] { $"Lỗi hệ thống: {ex.Message}" } });
            }
        }

        [HttpPost]
        public IActionResult RentKiosk([FromBody] RentRequest request)
        {
            try
            {
                var kiosk = _context.Kiosks
                    .Include(k => k.Renters)
                    .FirstOrDefault(k => k.Id == request.Id);
                if (kiosk == null)
                {
                    return Json(new { success = false, errors = new[] { "Kiosk không tồn tại." } });
                }

                // Tạo Renter mới
                var newRenter = new Renter
                {
                    KioskId = request.Id,
                    RenterName = request.RenterName,
                    RentalStartDate = DateTime.Parse(request.RentalStartDate),
                    RentalEndDate = request.RentalEndDate != null ? DateTime.Parse(request.RentalEndDate) : (DateTime?)null,
                    IsActive = true
                };
                _context.Renters.Add(newRenter);

                kiosk.IsRented = true;

                _context.SaveChanges();
                _context.Database.ExecuteSqlRaw("EXEC GeneratePaymentRecords;");
                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Lỗi khi lưu thay đổi: {ex.InnerException?.Message}");
                return Json(new { success = false, errors = new[] { $"Lỗi hệ thống: {ex.InnerException?.Message ?? ex.Message}" } });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi gọi stored procedure hoặc xử lý: {ex.Message}");
                return Json(new { success = false, errors = new[] { $"Lỗi hệ thống: {ex.Message}" } });
            }
        }

        [HttpPost]
        public IActionResult ReturnKiosk([FromBody] IdRequest request)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                int id = request.Id;

                // Lấy kiosk và renters
                var kiosk = _context.Kiosks
                    .Include(k => k.Renters)
                    .FirstOrDefault(k => k.Id == id);

                if (kiosk == null)
                {
                    return Json(new { success = false, errors = new[] { "Kiosk không tồn tại." } });
                }

                // Tìm Renter đang active
                var activeRenter = kiosk.Renters?.FirstOrDefault(r => r.IsActive);
                if (activeRenter == null)
                {
                    return Json(new { success = false, errors = new[] { "Kiosk chưa được thuê." } });
                }

                // Xóa tất cả bản ghi Payments liên quan đến Renter
                var payments = _context.Payments.Where(p => p.RenterId == activeRenter.Id).ToList();
                if (payments.Any())
                {
                    _context.Payments.RemoveRange(payments);
                }

                // Cập nhật trạng thái
                kiosk.IsRented = false;
                activeRenter.IsActive = false;
                _context.Renters.Update(activeRenter);

                // Lưu thay đổi
                var changes = _context.SaveChanges();
                if (changes > 0)
                {
                    transaction.Commit();
                    return Json(new { success = true });
                }

                return Json(new { success = false, errors = new[] { "Không thể cập nhật trạng thái Kiosk." } });
            }
            catch (DbUpdateException ex)
            {
                transaction.Rollback();
                _logger.LogError($"Lỗi khi lưu thay đổi: {ex.InnerException?.Message}");
                return Json(new { success = false, errors = new[] { $"Lỗi hệ thống: {ex.InnerException?.Message ?? ex.Message}" } });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError($"Lỗi hệ thống: {ex.Message}");
                return Json(new { success = false, errors = new[] { $"Lỗi hệ thống: {ex.Message}" } });
            }
        }

        [HttpPost]
        public IActionResult Pay([FromBody] PayRequest request)
        {
            try
            {
                // Lấy tháng hiện tại định dạng yyyy-MM
                var currentMonth = DateTime.Today.ToString("yyyy-MM");

                // Tìm bản ghi Payments cho RenterId và tháng hiện tại
                var payment = _context.Payments
                    .FirstOrDefault(p => p.RenterId == request.RenterId && p.PaymentMonth == currentMonth && !p.IsPaid);

                if (payment == null)
                {
                    return Json(new { success = false, errors = new[] { "Không tìm thấy bản ghi thanh toán chưa trả cho tháng hiện tại." } });
                }

                // Cập nhật trạng thái thanh toán
                payment.IsPaid = true;
                payment.PaymentDate = DateTime.Now;

                _context.Payments.Update(payment);

                var changes = _context.SaveChanges();
                if (changes > 0)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, errors = new[] { "Không thể cập nhật trạng thái thanh toán." } });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Lỗi khi cập nhật thanh toán: {ex.InnerException?.Message}");
                return Json(new { success = false, errors = new[] { $"Lỗi hệ thống: {ex.InnerException?.Message ?? ex.Message}" } });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi hệ thống: {ex.Message}");
                return Json(new { success = false, errors = new[] { $"Lỗi hệ thống: {ex.Message}" } });
            }
        }

        public IActionResult Details(int id)
        {
            var kiosk = _context.Kiosks
                .Include(k => k.Renters)
                .FirstOrDefault(k => k.Id == id);
            if (kiosk == null)
            {
                return NotFound();
            }
            return View(kiosk);
        }
    }

    public class IdRequest
    {
        public int Id { get; set; }
    }
}