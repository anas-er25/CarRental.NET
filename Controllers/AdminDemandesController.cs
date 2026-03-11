using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;
using CarRental.Services;

namespace CarRental.Controllers
{
    [Authorize]
    public class AdminDemandesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILocationService _locationService;

        public AdminDemandesController(ApplicationDbContext db, ILocationService locationService)
        { _db = db; _locationService = locationService; }

        public async Task<IActionResult> Index(string? statut)
        {
            var q = _db.DemandesLocation
                .Include(d => d.Voiture)
                .Include(d => d.Client)
                .AsQueryable();
            if (!string.IsNullOrEmpty(statut)) q = q.Where(d => d.Statut == statut);
            ViewBag.Statut = statut;
            return View(await q.OrderByDescending(d => d.DateDemande).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var d = await _db.DemandesLocation
                .Include(d => d.Voiture)
                .Include(d => d.Client)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (d == null) return NotFound();
            return View(d);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmer(int id, string? noteAdmin)
        {
            var demande = await _db.DemandesLocation.Include(d => d.Voiture).Include(d => d.Client).FirstOrDefaultAsync(d => d.Id == id);
            if (demande == null) return NotFound();

            var voiture = await _db.Voitures.FindAsync(demande.VoitureId);
            if (voiture == null || !voiture.EstDisponible)
            { TempData["Error"] = "La voiture n'est plus disponible."; return RedirectToAction(nameof(Index)); }

            // Créer la location
            var prix = _locationService.CalculerPrixTotal(voiture.PrixParJour, demande.DateDebut, demande.DateFin);
            var location = new Location
            {
                VoitureId = demande.VoitureId,
                ClientId = demande.ClientId ?? 0,
                DateDebut = demande.DateDebut,
                DateFin = demande.DateFin,
                PrixTotal = prix,
                Statut = "EnCours",
                DateCreation = DateTime.Now,
                Remarques = demande.Remarques
            };

            // Si client anonyme, créer le compte
            if (demande.ClientId == null)
            {
                var newClient = new Client
                {
                    Nom = demande.NomDemandeur ?? "",
                    Prenom = demande.PrenomDemandeur ?? "",
                    CIN = demande.CINDemandeur ?? "",
                    Telephone = demande.TelephoneDemandeur ?? "",
                    Email = demande.EmailDemandeur ?? "",
                    Adresse = demande.AdresseDemandeur,
                    DateInscription = DateTime.Now,
                    CompteActif = true
                };
                _db.Clients.Add(newClient);
                await _db.SaveChangesAsync();
                location.ClientId = newClient.Id;
                demande.ClientId = newClient.Id;
            }

            _db.Locations.Add(location);
            voiture.EstDisponible = false;
            demande.Statut = "Confirmee";
            demande.NoteAdmin = noteAdmin;
            demande.DateTraitement = DateTime.Now;

            // Notification client
            if (demande.ClientId.HasValue)
            {
                _db.Notifications.Add(new Notification
                {
                    Type = "ConfirmationLocation",
                    Titre = "Votre location est confirmée !",
                    Message = $"Votre demande pour {voiture.Marque} {voiture.Modele} du {demande.DateDebut:dd/MM/yyyy} au {demande.DateFin:dd/MM/yyyy} a été confirmée.",
                    ClientId = demande.ClientId,
                    LienAction = "/Portail/Espace"
                });
            }
            await _db.SaveChangesAsync();
            demande.LocationId = location.Id;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Location confirmée ! Prix: {prix:N0} DH";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rejeter(int id, string? noteAdmin)
        {
            var demande = await _db.DemandesLocation.Include(d => d.Voiture).FirstOrDefaultAsync(d => d.Id == id);
            if (demande == null) return NotFound();
            demande.Statut = "Rejetee";
            demande.NoteAdmin = noteAdmin;
            demande.DateTraitement = DateTime.Now;

            if (demande.ClientId.HasValue)
            {
                _db.Notifications.Add(new Notification
                {
                    Type = "DemandeRejetee",
                    Titre = "Demande non confirmée",
                    Message = $"Votre demande pour {demande.Voiture?.Marque} {demande.Voiture?.Modele} n'a pas pu être confirmée.{(string.IsNullOrEmpty(noteAdmin) ? "" : " Motif : " + noteAdmin)}",
                    ClientId = demande.ClientId,
                    LienAction = "/Portail/Espace"
                });
            }
            await _db.SaveChangesAsync();
            TempData["Success"] = "Demande rejetée.";
            return RedirectToAction(nameof(Index));
        }
    }
}
