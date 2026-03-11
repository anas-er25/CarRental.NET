using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;
using CarRental.Models.ViewModels;

namespace CarRental.Controllers
{
    [Authorize]
    public class IncidentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        public IncidentController(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

        public async Task<IActionResult> Retour(int id)
        {
            var location = await _db.Locations.Include(l => l.Voiture).Include(l => l.Client)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (location == null) return NotFound();
            if (location.Statut != "EnCours") { TempData["Error"] = "Location déjà terminée."; return RedirectToAction("Index", "Location"); }

            var incidents = await _db.Incidents.Where(i => i.LocationId == id).ToListAsync();
            var vm = new RetourViewModel { Location = location, IncidentsExistants = incidents };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retour(RetourViewModel vm, IFormFile? photoIncident)
        {
            var location = await _db.Locations.Include(l => l.Voiture).FirstOrDefaultAsync(l => l.Id == vm.Location.Id);
            if (location == null) return NotFound();

            // Retour anticipé ou tardif
            var today = DateTime.Today;
            if (today < location.DateFin)
            {
                // Retour anticipé: recalculer prix
                var joursReels = Math.Max(1, (today - location.DateDebut).Days);
                var prixOriginal = location.PrixTotal;
                location.PrixTotal = (location.Voiture?.PrixParJour ?? 0) * joursReels;
                location.DateFin = today;
                _db.Notifications.Add(new Notification
                {
                    Type = "RetourAnticipe",
                    Titre = "Retour anticipé",
                    Message = $"Retour anticipé de {location.Voiture?.Marque} {location.Voiture?.Modele}. Prix recalculé: {location.PrixTotal:N0} DH (était {prixOriginal:N0} DH)",
                    LienAction = $"/Location/Details/{location.Id}"
                });
            }
            else if (today > location.DateFin)
            {
                // Retour tardif: frais supplémentaires
                var joursSupp = (today - location.DateFin).Days;
                var fraisTardif = joursSupp * (location.Voiture?.PrixParJour ?? 0) * 1.5m; // +50% pénalité
                _db.Incidents.Add(new IncidentVehicule
                {
                    VoitureId = location.VoitureId,
                    LocationId = location.Id,
                    ClientId = location.ClientId,
                    TypeIncident = "RetourTardif",
                    Description = $"Retour tardif de {joursSupp} jour(s). Pénalité appliquée.",
                    FraisSupplementaires = fraisTardif,
                    DateIncident = today
                });
                _db.Notifications.Add(new Notification
                {
                    Type = "RetourTardif",
                    Titre = "Retour tardif détecté",
                    Message = $"{location.Voiture?.Marque} {location.Voiture?.Modele}: retour tardif de {joursSupp} jour(s). Frais: {fraisTardif:N0} DH",
                    LienAction = $"/Location/Details/{location.Id}"
                });
            }

            // Traiter incident signalé
            if (vm.ADommage && !string.IsNullOrEmpty(vm.TypeIncident))
            {
                string? photoUrl = null;
                if (photoIncident != null && photoIncident.Length > 0)
                {
                    var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var dir = Path.Combine(webRootPath, "uploads", "incidents");
                    Directory.CreateDirectory(dir);
                    var fname = $"{Guid.NewGuid()}{Path.GetExtension(photoIncident.FileName)}";
                    using var stream = new FileStream(Path.Combine(dir, fname), FileMode.Create);
                    await photoIncident.CopyToAsync(stream);
                    photoUrl = $"/uploads/incidents/{fname}";
                }

                var incident = new IncidentVehicule
                {
                    VoitureId = location.VoitureId,
                    LocationId = location.Id,
                    ClientId = location.ClientId,
                    TypeIncident = vm.TypeIncident,
                    Description = vm.DescriptionIncident,
                    FraisSupplementaires = vm.FraisIncident,
                    DateIncident = today,
                    PhotoUrl = photoUrl
                };
                _db.Incidents.Add(incident);

                location.ADommage = true;
                location.DescriptionDommage = vm.DescriptionIncident;
                location.MontantDommage = (location.MontantDommage ?? 0) + vm.FraisIncident;

                _db.Notifications.Add(new Notification
                {
                    Type = "SignalementDommage",
                    Titre = $"Dommage signalé: {vm.TypeIncident}",
                    Message = $"{location.Voiture?.Marque} {location.Voiture?.Modele}: {vm.TypeIncident}. Frais: {vm.FraisIncident:N0} DH",
                    LienAction = $"/Location/Details/{location.Id}"
                });
            }

            location.Statut = "Terminée";
            if (location.Voiture != null) location.Voiture.EstDisponible = true;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Retour enregistré avec succès.";
            return RedirectToAction("Index", "Location");
        }

        public async Task<IActionResult> Historique(int? voitureId)
        {
            // If no voitureId → show all incidents
            IQueryable<IncidentVehicule> query = _db.Incidents
                .Include(i => i.Client)
                .Include(i => i.Location)
                .Include(i => i.Voiture);

            if (voitureId.HasValue && voitureId.Value > 0)
            {
                var voiture = await _db.Voitures.FindAsync(voitureId.Value);
                if (voiture == null) return NotFound();
                query = query.Where(i => i.VoitureId == voitureId.Value);
                ViewBag.Voiture = voiture;
            }

            var incidents = await query.OrderByDescending(i => i.DateIncident).ToListAsync();
            return View(incidents);
        }

        public async Task<IActionResult> HistoriqueClient(int clientId)
        {
            var client = await _db.Clients.FindAsync(clientId);
            if (client == null) return NotFound();
            var incidents = await _db.Incidents
                .Include(i => i.Voiture)
                .Include(i => i.Location)
                .Where(i => i.ClientId == clientId)
                .OrderByDescending(i => i.DateIncident)
                .ToListAsync();
            ViewBag.Client = client;
            return View(incidents);
        }
    }
}
