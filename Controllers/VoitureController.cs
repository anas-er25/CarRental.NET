using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;

namespace CarRental.Controllers
{
    [Authorize]
    public class VoitureController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public VoitureController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(string? search, string? categorie, bool? disponible)
        {
            var query = _context.Voitures.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(v => v.Marque.Contains(search) || v.Modele.Contains(search) || v.Immatriculation.Contains(search));
            if (!string.IsNullOrEmpty(categorie))
                query = query.Where(v => v.Categorie == categorie);
            if (disponible.HasValue)
                query = query.Where(v => v.EstDisponible == disponible.Value);
            ViewBag.Search = search; ViewBag.Categorie = categorie; ViewBag.Disponible = disponible;
            return View(await query.OrderBy(v => v.Marque).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var voiture = await _context.Voitures.Include(v => v.Locations).ThenInclude(l => l.Client).FirstOrDefaultAsync(v => v.Id == id);
            if (voiture == null) return NotFound();
            return View(voiture);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voiture voiture, List<IFormFile>? ImageFiles)
        {
            if (ModelState.IsValid)
            {
                voiture.Images = await SaveImages(ImageFiles);
                _context.Add(voiture);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Voiture {voiture.Marque} {voiture.Modele} ajoutée !";
                return RedirectToAction(nameof(Index));
            }
            return View(voiture);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var voiture = await _context.Voitures.FindAsync(id);
            if (voiture == null) return NotFound();
            return View(voiture);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Voiture voiture, List<IFormFile>? ImageFiles)
        {
            if (id != voiture.Id) return NotFound();
            if (ModelState.IsValid)
            {
                // Add new images to existing ones
                var newImgs = await SaveImages(ImageFiles);
                var existing = voiture.Images; // from hidden field ImagesJson
                existing.AddRange(newImgs);
                voiture.Images = existing;
                _context.Update(voiture);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Voiture mise à jour !";
                return RedirectToAction(nameof(Index));
            }
            return View(voiture);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var voiture = await _context.Voitures.FirstOrDefaultAsync(v => v.Id == id);
            if (voiture == null) return NotFound();
            return View(voiture);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var voiture = await _context.Voitures.FindAsync(id);
            if (voiture != null)
            {
                bool hasActive = await _context.Locations.AnyAsync(l => l.VoitureId == id && l.Statut == "EnCours");
                if (hasActive) { TempData["Error"] = "Voiture avec location en cours, suppression impossible."; return RedirectToAction(nameof(Index)); }
                _context.Voitures.Remove(voiture);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Voiture supprimée !";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<string>> SaveImages(List<IFormFile>? files)
        {
            var paths = new List<string>();
            if (files == null || !files.Any()) return paths;

            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadDir = Path.Combine(webRootPath, "uploads", "voitures");
            Directory.CreateDirectory(uploadDir);
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var ext = Path.GetExtension(file.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadDir, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                    paths.Add($"/uploads/voitures/{fileName}");
                }
            }
            return paths;
        }
    }
}
