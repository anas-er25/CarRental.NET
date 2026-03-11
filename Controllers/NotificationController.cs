using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;

namespace CarRental.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _db;
        public NotificationController(ApplicationDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var notifs = await _db.Notifications
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();
            return View(notifs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarquerLu(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n != null) { n.EstLu = true; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarquerNonLu(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n != null) { n.EstLu = false; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToutMarquerLu()
        {
            await _db.Notifications.Where(n => !n.EstLu).ExecuteUpdateAsync(s => s.SetProperty(n => n.EstLu, true));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var count = await _db.Notifications.CountAsync(n => !n.EstLu);
            return Json(count);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentes()
        {
            var notifs = await _db.Notifications
                .OrderByDescending(n => n.DateCreation)
                .Take(8)
                .Select(n => new { n.Id, n.Titre, n.Message, n.Type, n.EstLu, n.LienAction, date = n.DateCreation.ToString("dd/MM HH:mm") })
                .ToListAsync();
            var count = await _db.Notifications.CountAsync(n => !n.EstLu);
            return Json(new { notifs, count });
        }
    }
}
