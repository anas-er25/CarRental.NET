using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;

namespace CarRental.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ClientController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Clients.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.Nom.Contains(search) || c.Prenom.Contains(search) || c.CIN.Contains(search) || c.Email.Contains(search));
            ViewBag.Search = search;
            return View(await query.OrderBy(c => c.Nom).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = await _context.Clients.Include(c => c.Locations).ThenInclude(l => l.Voiture).FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (await _context.Clients.AnyAsync(c => c.CIN == client.CIN))
                ModelState.AddModelError("CIN", "Ce CIN existe déjà.");
            if (ModelState.IsValid)
            {
                client.DateInscription = DateTime.Now;
                _context.Add(client);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Client {client.NomComplet} ajouté !";
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // Quick create from modal (returns JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuick(string Nom, string Prenom, string CIN, string Telephone, string Email)
        {
            if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Prenom) || string.IsNullOrWhiteSpace(CIN) || string.IsNullOrWhiteSpace(Telephone) || string.IsNullOrWhiteSpace(Email))
                return Json(new { success = false, message = "Tous les champs sont obligatoires." });

            if (await _context.Clients.AnyAsync(c => c.CIN == CIN))
                return Json(new { success = false, message = "Ce CIN existe déjà dans le système." });

            var client = new Client { Nom = Nom, Prenom = Prenom, CIN = CIN, Telephone = Telephone, Email = Email, DateInscription = DateTime.Now };
            _context.Add(client);
            await _context.SaveChangesAsync();
            return Json(new { success = true, id = client.Id, nomComplet = client.NomComplet });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id) return NotFound();
            if (await _context.Clients.AnyAsync(c => c.CIN == client.CIN && c.Id != id))
                ModelState.AddModelError("CIN", "Ce CIN est déjà utilisé.");
            if (ModelState.IsValid)
            {
                _context.Update(client);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Client mis à jour !";
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                bool hasActive = await _context.Locations.AnyAsync(l => l.ClientId == id && l.Statut == "EnCours");
                if (hasActive) { TempData["Error"] = "Client avec location en cours, suppression impossible."; return RedirectToAction(nameof(Index)); }
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Client supprimé !";
            }
            return RedirectToAction(nameof(Index));
        }

        // ── ACTIVER / DÉSACTIVER ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactiver(int id, string? motif, string? remarque)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            client.CompteActif = false;
            client.MotifDesactivation = motif;
            client.RemarqueAdmin = remarque;
            client.DateDesactivation = DateTime.Now;
            _context.Notifications.Add(new Notification
            {
                Type = "CompteDesactive",
                Titre = "Compte désactivé",
                Message = $"Le compte de {client.NomComplet} a été désactivé. Motif : {motif ?? "Non précisé"}",
                ClientId = id,
                LienAction = $"/Client/Details/{id}"
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Compte de {client.NomComplet} désactivé.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activer(int id, string? remarque)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            client.CompteActif = true;
            client.MotifDesactivation = null;
            client.DateDesactivation = null;
            client.RemarqueAdmin = remarque;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Compte de {client.NomComplet} réactivé.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ── INCIDENTS CLIENT ──
        public async Task<IActionResult> Incidents(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            var incidents = await _context.Incidents
                .Include(i => i.Voiture).Include(i => i.Location)
                .Where(i => i.ClientId == id)
                .OrderByDescending(i => i.DateIncident).ToListAsync();
            ViewBag.Client = client;
            return View("~/Views/Incident/Historique.cshtml", incidents);
        }
    }
}
