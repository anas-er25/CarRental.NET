using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models.ViewModels;

namespace CarRental.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var debutMois = new DateTime(now.Year, now.Month, 1);

            // Vérifier retours tardifs et créer notifs automatiquement
            var locationsEnRetard = await _db.Locations
                .Include(l => l.Voiture)
                .Where(l => l.Statut == "EnCours" && l.DateFin < DateTime.Today)
                .ToListAsync();

            foreach (var loc in locationsEnRetard)
            {
                var dejaNotifie = await _db.Notifications.AnyAsync(n =>
                    n.Type == "RetourTardif" && n.LocationId == loc.Id && n.DateCreation.Date == DateTime.Today);
                if (!dejaNotifie)
                {
                    _db.Notifications.Add(new CarRental.Models.Notification
                    {
                        Type = "RetourTardif",
                        Titre = "Retour tardif !",
                        Message = $"{loc.Voiture?.Marque} {loc.Voiture?.Modele} aurait dû être retournée le {loc.DateFin:dd/MM/yyyy}",
                        LienAction = $"/Incident/Retour/{loc.Id}",
                        LocationId = loc.Id
                    });
                }
            }
            if (locationsEnRetard.Any()) await _db.SaveChangesAsync();

            var vm = new DashboardViewModel
            {
                TotalVoitures = await _db.Voitures.CountAsync(),
                VoituresDisponibles = await _db.Voitures.CountAsync(v => v.EstDisponible),
                VoituresLouees = await _db.Voitures.CountAsync(v => !v.EstDisponible),
                TotalClients = await _db.Clients.CountAsync(),
                LocationsEnCours = await _db.Locations.CountAsync(l => l.Statut == "EnCours"),
                LocationsTerminees = await _db.Locations.CountAsync(l => l.Statut == "Terminée"),
                RevenusTotal = await _db.Locations.Where(l => l.Statut == "Terminée").SumAsync(l => (decimal?)(l.PrixTotal + (l.MontantDommage ?? 0))) ?? 0,
                RevenusMois = await _db.Locations.Where(l => l.Statut == "Terminée" && l.DateCreation >= debutMois).SumAsync(l => (decimal?)(l.PrixTotal + (l.MontantDommage ?? 0))) ?? 0,
                DernieresLocations = await _db.Locations.Include(l => l.Voiture).Include(l => l.Client).OrderByDescending(l => l.DateCreation).Take(6).ToListAsync(),
                VoituresDispoList = await _db.Voitures.Where(v => v.EstDisponible).Take(5).ToListAsync(),
                NotifsNonLues = await _db.Notifications.CountAsync(n => !n.EstLu),
                DernieresNotifs = await _db.Notifications.OrderByDescending(n => n.DateCreation).Take(5).ToListAsync(),
                DemandesEnAttente = await _db.DemandesLocation.CountAsync(d => d.Statut == "EnAttente")
            };

            return View(vm);
        }
    }
}
