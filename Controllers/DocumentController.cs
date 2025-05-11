using Microsoft.AspNetCore.Mvc;
using KioskManagementWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using KioskManagementWebApp.Data;

namespace KioskManagementWebApp.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Document
        public async Task<IActionResult> Index()
        {
            var documents = await _context.Documents.ToListAsync();
            return View(documents);
        }

        // GET: Document/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Document/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Document document)
        {
            if (ModelState.IsValid)
            {
                _context.Add(document);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // GET: Document/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        // POST: Document/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Document document)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.Id))
                    {
                        return Json(new { success = false, errors = new[] { "Document not found." } });
                    }
                    throw;
                }
            }
            return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // POST: Document/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return Json(new { success = false, errors = new[] { "Document not found." } });
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
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

            // Lọc các mục có số lượng lớn hơn 0
            var validOrderItems = orderRequest.OrderItems.Where(item => item.Quantity > 0).ToList();
            if (!validOrderItems.Any())
            {
                return Json(new { success = false, errors = new[] { "No valid items in the order." } });
            }

            // Kiểm tra tồn kho và tính tổng giá
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

            // Tạo đơn hàng mới
            var order = new Order
            {
                OrderDate = DateTime.Now,
                TotalPrice = totalPrice,
                Status = "Completed", // Đặt trạng thái thành Completed khi đặt hàng thành công
                OrderItems = new List<OrderItem>()
            };

            // Thêm các mục vào đơn hàng và cập nhật tồn kho
            foreach (var item in validOrderItems)
            {
                var document = await _context.Documents.FindAsync(item.Id);
                document.Stock -= item.Quantity; // Giảm tồn kho
                _context.Update(document);

                order.OrderItems.Add(new OrderItem
                {
                    DocumentId = item.Id,
                    Quantity = item.Quantity,
                    Price = document.Price
                });
            }

            // Lưu đơn hàng vào cơ sở dữ liệu
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }
    }
}