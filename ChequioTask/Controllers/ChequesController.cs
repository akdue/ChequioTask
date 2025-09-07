using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChequioTask.Data;
using ChequioTask.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace ChequioTask.Controllers
{
    // Auth required for all actions in this controller
    [Authorize]
    public class ChequesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChequesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // READ (list) — any authenticated user with search & filters
        public async Task<IActionResult> Index(string? q, ChequeStatus? status, DateTime? from, DateTime? to)
        {
            // Base query
            var query = _context.Cheques.AsQueryable();

            // Text search: cheque number OR payee name
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(c => c.Number.Contains(q) || c.PayeeName.Contains(q));

            // Status filter
            if (status.HasValue)
                query = query.Where(c => c.Status == status);

            // Date range: IssueDate >= from, DueDate <= to
            if (from.HasValue)
                query = query.Where(c => c.IssueDate.Date >= from.Value.Date);

            if (to.HasValue)
                query = query.Where(c => c.DueDate.Date <= to.Value.Date);

            // Provide status list to the view (pre-selected)
            ViewBag.StatusList = new SelectList(
                Enum.GetValues<ChequeStatus>()
                    .Cast<ChequeStatus>()
                    .Select(s => new { Id = (int)s, Name = s.ToString() }),
                "Id", "Name", status.HasValue ? (int)status.Value : null
            );

            var items = await query
                .OrderByDescending(c => c.IssueDate)
                .ToListAsync();

            return View(items);
        }

        // READ (details) — any authenticated user
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cheque = await _context.Cheques.FirstOrDefaultAsync(m => m.Id == id);
            if (cheque == null) return NotFound();

            return View(cheque);
        }

        // CREATE (GET) — Admin only
        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        // CREATE (POST) — Admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Number,PayeeName,Amount,Currency,IssueDate,DueDate,Status,Notes")] Cheque cheque)
        {
            if (!ModelState.IsValid) return View(cheque);

            // Server-side creation timestamp (UTC)
            cheque.CreatedAtUtc = DateTime.UtcNow;

            // Optional: uniqueness guard for Number
            var exists = await _context.Cheques.AnyAsync(c => c.Number == cheque.Number);
            if (exists)
            {
                ModelState.AddModelError(nameof(Cheque.Number), "Cheque number already exists.");
                return View(cheque);
            }

            _context.Cheques.Add(cheque);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // EDIT (GET) — Admin only
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cheque = await _context.Cheques.FindAsync(id);
            if (cheque == null) return NotFound();

            return View(cheque);
        }

        // EDIT (POST) — Admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,PayeeName,Amount,Currency,IssueDate,DueDate,Status,Notes")] Cheque cheque)
        {
            // If route id and model id don't match, show validation error instead of 404
            if (id != cheque.Id)
            {
                ModelState.AddModelError(string.Empty, "Invalid request: id mismatch.");
                return View(cheque);
            }

            if (!ModelState.IsValid)
                return View(cheque);

            // Preserve original CreatedAtUtc (server-owned)
            var original = await _context.Cheques.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (original is null)
            {
                // If the row disappeared between GET and POST
                ModelState.AddModelError(string.Empty, "The cheque no longer exists.");
                return View(cheque);
            }
            cheque.CreatedAtUtc = original.CreatedAtUtc;

            // Enforce unique Number on update
            var exists = await _context.Cheques.AnyAsync(c => c.Number == cheque.Number && c.Id != cheque.Id);
            if (exists)
            {
                ModelState.AddModelError(nameof(Cheque.Number), "Cheque number already exists.");
                return View(cheque);
            }

            try
            {
                _context.Update(cheque);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // If deleted by someone else while saving
                if (!await _context.Cheques.AnyAsync(e => e.Id == cheque.Id))
                {
                    ModelState.AddModelError(string.Empty, "The cheque was deleted by another operation.");
                    return View(cheque);
                }
                throw;
            }

            // Success → back to list
            return RedirectToAction(nameof(Index));
        }


        // DELETE (GET) — Admin only
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cheque = await _context.Cheques.FirstOrDefaultAsync(m => m.Id == id);
            if (cheque == null) return NotFound();

            return View(cheque);
        }

        // DELETE (POST) — Admin only
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cheque = await _context.Cheques.FindAsync(id);
            if (cheque != null)
            {
                _context.Cheques.Remove(cheque);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Cheques/Print/
        [Authorize]
        public async Task<IActionResult> Print(int id)
        {
            var cheque = await _context.Cheques
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cheque == null) return NotFound();

            return View(cheque); // Views/Cheques/Print.cshtml
        }

        // Helper
        private Task<bool> ChequeExistsAsync(int id) =>
            _context.Cheques.AnyAsync(e => e.Id == id);
    }
}
