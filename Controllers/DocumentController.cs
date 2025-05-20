using Microsoft.AspNetCore.Mvc;
using KioskManagementWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using KioskManagementWebApp.Data;
using Microsoft.Extensions.Logging;

namespace KioskManagementWebApp.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(ApplicationDbContext context, ILogger<DocumentController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;

            // Kiểm tra dữ liệu khi khởi tạo controller
            var initialDocuments = _context.Documents.ToList();
            _logger.LogInformation($"Initial Document check on startup: [{string.Join(", ", initialDocuments.Select(d => $"Id={d.Id}, Title={d.Title}"))}]");
        }

        // GET: Document
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
            {
                _logger.LogInformation("Index: Role is null or empty, redirecting to Login");
                return RedirectToAction("Login", "Account");
            }

            ViewData["Role"] = role;
            var documents = await _context.Documents.ToListAsync();
            return View(documents);
        }

        // GET: Document/ManageDocuments
        public async Task<IActionResult> ManageDocuments()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                _logger.LogInformation("ManageDocuments: Role is null, empty, or not Admin, redirecting to Index");
                return RedirectToAction("Index");
            }

            ViewData["Role"] = role;

            var viewModel = new DocumentManagementViewModel
            {
                Documents = await _context.Documents.ToListAsync(),
                SoldQuantities = await GetSoldQuantities(),
                TotalSoldDocuments = await GetTotalSoldDocuments(),
                TotalRevenue = await GetTotalRevenue(),
                TotalStock = await _context.Documents.SumAsync(d => d.Stock)
            };

            return View(viewModel);
        }

        // POST: Document/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Document document)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingDocument = await _context.Documents
                        .FirstOrDefaultAsync(d => d.Title.ToLower() == document.Title.ToLower());

                    if (existingDocument != null)
                    {
                        return Json(new { success = false, errors = new[] { $"Tài liệu với tiêu đề '{document.Title}' đã tồn tại." } });
                    }

                    _context.Add(document);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }

                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi tạo tài liệu: {ex.Message}");
                return Json(new { success = false, errors = new[] { $"Lỗi hệ thống: {ex.Message}" } });
            }
        }

        // POST: Document/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string title, string type, decimal price, int stock)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                {
                    return Json(new { success = false, errors = new[] { "Tài liệu không tồn tại." } });
                }

                document.Title = title;
                document.Type = type;
                document.Price = price;
                document.Stock = stock;

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi sửa tài liệu: {ex.Message}");
                return Json(new { success = false, errors = new[] { $"Lỗi hệ thống: {ex.Message}" } });
            }
        }

        // POST: Document/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id);
                if (document == null)
                {
                    return Json(new { success = false, errors = new[] { "Tài liệu không tồn tại." } });
                }

                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi xóa tài liệu: {ex.Message}");
                return Json(new { success = false, errors = new[] { $"Lỗi hệ thống: {ex.Message}" } });
            }
        }

        // POST: Document/Sell
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sell(OrderRequest orderRequest)
        {
            if (orderRequest == null || orderRequest.OrderItems == null || !orderRequest.OrderItems.Any(item => item.Quantity > 0))
            {
                return Json(new { success = false, errors = new[] { "No items selected for the order." } });
            }

            var validOrderItems = orderRequest.OrderItems.Where(item => item.Quantity > 0).ToList();
            if (!validOrderItems.Any())
            {
                return Json(new { success = false, errors = new[] { "No valid items in the order." } });
            }

            decimal totalPrice = 0;
            foreach (var item in validOrderItems)
            {
                var document = await _context.Documents.FindAsync(item.Id);
                if (document == null)
                {
                    return Json(new { success = false, errors = new[] { $"Document with ID {item.Id} not found." } });
                }
                if (document.Stock < item.Quantity)
                {
                    return Json(new { success = false, errors = new[] { $"Not enough stock for document: {document.Title}." } });
                }
                totalPrice += document.Price * item.Quantity;
            }

            var order = new Order
            {
                OrderDate = DateTime.Now,
                TotalPrice = totalPrice,
                Status = "Completed",
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in validOrderItems)
            {
                var document = await _context.Documents.FindAsync(item.Id);
                document.Stock -= item.Quantity;
                _context.Update(document);

                order.OrderItems.Add(new OrderItem
                {
                    DocumentId = item.Id,
                    Quantity = item.Quantity,
                    Price = document.Price
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private async Task<Dictionary<int, int>> GetSoldQuantities()
        {
            var soldQuantities = await _context.OrderItems
                .GroupBy(oi => oi.DocumentId)
                .Select(g => new { DocumentId = g.Key, Quantity = g.Sum(oi => oi.Quantity) })
                .ToDictionaryAsync(g => g.DocumentId, g => g.Quantity);
            return soldQuantities;
        }

        private async Task<int> GetTotalSoldDocuments()
        {
            return await _context.OrderItems.SumAsync(oi => oi.Quantity);
        }

        private async Task<decimal> GetTotalRevenue()
        {
            return await _context.Orders.SumAsync(o => o.TotalPrice);
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }
    }
}