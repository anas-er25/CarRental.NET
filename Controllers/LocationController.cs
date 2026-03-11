//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.EntityFrameworkCore;
//using CarRental.Data;
//using CarRental.Models;
//using CarRental.Models.ViewModels;
//using CarRental.Services;

//namespace CarRental.Controllers
//{
//    [Authorize]
//    public class LocationController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly ILocationService _locationService;

//        public LocationController(ApplicationDbContext context, ILocationService locationService)
//        { _context = context; _locationService = locationService; }

//        public async Task<IActionResult> Index(string? statut)
//        {
//            var query = _context.Locations.Include(l => l.Voiture).Include(l => l.Client).AsQueryable();
//            if (!string.IsNullOrEmpty(statut)) query = query.Where(l => l.Statut == statut);
//            ViewBag.Statut = statut;
//            return View(await query.OrderByDescending(l => l.DateCreation).ToListAsync());
//        }

//        public async Task<IActionResult> Details(int id)
//        {
//            var location = await _context.Locations.Include(l => l.Voiture).Include(l => l.Client).FirstOrDefaultAsync(l => l.Id == id);
//            if (location == null) return NotFound();
//            return View(location);
//        }

//        public async Task<IActionResult> Create(int? voitureId)
//        {
//            var vm = new LocationCreateViewModel
//            {
//                VoituresDisponibles = await _context.Voitures.Where(v => v.EstDisponible).ToListAsync(),
//                Clients = await _context.Clients.OrderBy(c => c.Nom).ToListAsync()
//            };
//            vm.Location.DateDebut = DateTime.Today;
//            vm.Location.DateFin = DateTime.Today.AddDays(1);
//            if (voitureId.HasValue) vm.Location.VoitureId = voitureId.Value;
//            return View(vm);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(LocationCreateViewModel vm)
//        {
//            var location = vm.Location;
//            if (location.DateFin <= location.DateDebut)
//                ModelState.AddModelError("Location.DateFin", "La date de fin doit être après la date de début.");
//            if (location.DateDebut < DateTime.Today)
//                ModelState.AddModelError("Location.DateDebut", "La date de début ne peut pas être dans le passé.");
//            if (!_locationService.VoitureDisponible(location.VoitureId, location.DateDebut, location.DateFin))
//                ModelState.AddModelError("Location.VoitureId", "Cette voiture n'est pas disponible pour ces dates.");

//            if (ModelState.IsValid)
//            {
//                var voiture = await _context.Voitures.FindAsync(location.VoitureId);
//                if (voiture == null) return NotFound();
//                location.PrixTotal = _locationService.CalculerPrixTotal(voiture.PrixParJour, location.DateDebut, location.DateFin);
//                location.Statut = "EnCours";
//                location.DateCreation = DateTime.Now;
//                voiture.EstDisponible = false;
//                _context.Add(location);
//                await _context.SaveChangesAsync();
//                TempData["Success"] = $"Location créée ! Prix : {location.PrixTotal:N0} DH";
//                return RedirectToAction(nameof(Index));
//            }
//            vm.VoituresDisponibles = await _context.Voitures.Where(v => v.EstDisponible).ToListAsync();
//            vm.Clients = await _context.Clients.OrderBy(c => c.Nom).ToListAsync();
//            return View(vm);
//        }

//        public async Task<IActionResult> Retour(int id)
//        {
//            var location = await _context.Locations.Include(l => l.Voiture).Include(l => l.Client).FirstOrDefaultAsync(l => l.Id == id);
//            if (location == null) return NotFound();
//            if (location.Statut != "EnCours") { TempData["Error"] = "Location déjà terminée."; return RedirectToAction(nameof(Index)); }
//            return View(location);
//        }

//        [HttpPost, ActionName("Retour")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> RetourConfirmed(int id, bool ADommage, string? DescriptionDommage, decimal? MontantDommage, bool DommageRembourse)
//        {
//            var location = await _context.Locations.Include(l => l.Voiture).FirstOrDefaultAsync(l => l.Id == id);
//            if (location == null) return NotFound();
//            location.Statut = "Terminée";
//            location.ADommage = ADommage;
//            if (ADommage)
//            {
//                location.DescriptionDommage = DescriptionDommage;
//                location.MontantDommage = MontantDommage;
//                location.DommageRembourse = DommageRembourse;
//            }
//            if (location.Voiture != null) location.Voiture.EstDisponible = true;
//            await _context.SaveChangesAsync();
//            var msg = ADommage && MontantDommage > 0
//                ? $"Retour enregistré. Dommage : {MontantDommage:N0} DH à rembourser."
//                : "Retour enregistré avec succès !";
//            TempData["Success"] = msg;
//            return RedirectToAction(nameof(Index));
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Annuler(int id)
//        {
//            var location = await _context.Locations.Include(l => l.Voiture).FirstOrDefaultAsync(l => l.Id == id);
//            if (location == null) return NotFound();
//            location.Statut = "Annulée";
//            if (location.Voiture != null) location.Voiture.EstDisponible = true;
//            await _context.SaveChangesAsync();
//            TempData["Success"] = "Location annulée.";
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;
using CarRental.Models.ViewModels;
using CarRental.Services;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace CarRental.Controllers
{
    [Authorize]
    public class LocationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILocationService _locationService;
        private readonly IWebHostEnvironment _env;

        public LocationController(ApplicationDbContext context, ILocationService locationService, IWebHostEnvironment env)
        { _context = context; _locationService = locationService; _env = env; }

        public async Task<IActionResult> Index(string? statut)
        {
            var query = _context.Locations.Include(l => l.Voiture).Include(l => l.Client).AsQueryable();
            if (!string.IsNullOrEmpty(statut)) query = query.Where(l => l.Statut == statut);
            ViewBag.Statut = statut;
            return View(await query.OrderByDescending(l => l.DateCreation).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var location = await _context.Locations.Include(l => l.Voiture).Include(l => l.Client).FirstOrDefaultAsync(l => l.Id == id);
            if (location == null) return NotFound();
            return View(location);
        }

        public async Task<IActionResult> Create(int? voitureId)
        {
            var vm = new LocationCreateViewModel
            {
                VoituresDisponibles = await _context.Voitures.Where(v => v.EstDisponible).ToListAsync(),
                Clients = await _context.Clients.OrderBy(c => c.Nom).ToListAsync()
            };
            vm.Location.DateDebut = DateTime.Today;
            vm.Location.DateFin = DateTime.Today.AddDays(1);
            if (voitureId.HasValue) vm.Location.VoitureId = voitureId.Value;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LocationCreateViewModel vm)
        {
            var location = vm.Location;
            if (location.DateFin <= location.DateDebut)
                ModelState.AddModelError("Location.DateFin", "La date de fin doit être après la date de début.");
            if (location.DateDebut < DateTime.Today)
                ModelState.AddModelError("Location.DateDebut", "La date de début ne peut pas être dans le passé.");
            if (!_locationService.VoitureDisponible(location.VoitureId, location.DateDebut, location.DateFin))
                ModelState.AddModelError("Location.VoitureId", "Cette voiture n'est pas disponible pour ces dates.");

            if (ModelState.IsValid)
            {
                var voiture = await _context.Voitures.FindAsync(location.VoitureId);
                if (voiture == null) return NotFound();
                location.PrixTotal = _locationService.CalculerPrixTotal(voiture.PrixParJour, location.DateDebut, location.DateFin);
                location.Statut = "EnCours";
                location.DateCreation = DateTime.Now;
                voiture.EstDisponible = false;
                _context.Add(location);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Location créée ! Prix : {location.PrixTotal:N0} DH";
                return RedirectToAction(nameof(Index));
            }
            vm.VoituresDisponibles = await _context.Voitures.Where(v => v.EstDisponible).ToListAsync();
            vm.Clients = await _context.Clients.OrderBy(c => c.Nom).ToListAsync();
            return View(vm);
        }

        public async Task<IActionResult> Retour(int id)
        {
            var location = await _context.Locations.Include(l => l.Voiture).Include(l => l.Client).FirstOrDefaultAsync(l => l.Id == id);
            if (location == null) return NotFound();
            if (location.Statut != "EnCours") { TempData["Error"] = "Location déjà terminée."; return RedirectToAction(nameof(Index)); }
            return View(location);
        }

        [HttpPost, ActionName("Retour")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetourConfirmed(int id, bool ADommage, string? DescriptionDommage, decimal? MontantDommage, bool DommageRembourse, string? TypeIncidentRetour, IFormFile? PhotoDommage)
        {
            var location = await _context.Locations.Include(l => l.Voiture).FirstOrDefaultAsync(l => l.Id == id);
            if (location == null) return NotFound();
            location.Statut = "Terminée";
            location.ADommage = ADommage;

            if (ADommage)
            {
                location.DescriptionDommage = DescriptionDommage;
                location.MontantDommage = MontantDommage;
                location.DommageRembourse = DommageRembourse;

                // Créer un incident
                string? photoUrl = null;
                if (PhotoDommage != null && PhotoDommage.Length > 0)
                {
                    var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var dir = Path.Combine(webRootPath, "uploads", "incidents");
                    Directory.CreateDirectory(dir);
                    var fname = $"{Guid.NewGuid()}{Path.GetExtension(PhotoDommage.FileName)}";
                    using var stream = new FileStream(Path.Combine(dir, fname), FileMode.Create);
                    await PhotoDommage.CopyToAsync(stream);
                    photoUrl = $"/uploads/incidents/{fname}";
                }

                _context.Incidents.Add(new Models.IncidentVehicule
                {
                    VoitureId = location.VoitureId,
                    LocationId = location.Id,
                    ClientId = location.ClientId,
                    TypeIncident = string.IsNullOrEmpty(TypeIncidentRetour) ? "Autre" : TypeIncidentRetour,
                    Description = DescriptionDommage,
                    FraisSupplementaires = MontantDommage ?? 0,
                    EstRembourse = DommageRembourse,
                    DateIncident = DateTime.Today,
                    PhotoUrl = photoUrl
                });

                _context.Notifications.Add(new Models.Notification
                {
                    Type = "SignalementDommage",
                    Titre = $"Dommage signalé: {TypeIncidentRetour ?? "Autre"}",
                    Message = $"{location.Voiture?.Marque} {location.Voiture?.Modele}: {DescriptionDommage}. Frais: {MontantDommage:N0} DH",
                    LienAction = $"/Location/Details/{location.Id}"
                });
            }

            if (location.Voiture != null) location.Voiture.EstDisponible = true;
            await _context.SaveChangesAsync();

            var msg = ADommage && MontantDommage > 0
                ? $"Retour enregistré. Dommage ({TypeIncidentRetour}): {MontantDommage:N0} DH."
                : "Retour enregistré avec succès !";
            TempData["Success"] = msg;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Annuler(int id)
        {
            var location = await _context.Locations.Include(l => l.Voiture).FirstOrDefaultAsync(l => l.Id == id);
            if (location == null) return NotFound();
            location.Statut = "Annulée";
            if (location.Voiture != null) location.Voiture.EstDisponible = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Location annulée.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ExportPDF(int id)
        {
            var location = await _context.Locations
                .Include(l => l.Voiture)
                .Include(l => l.Client)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (location == null) return NotFound();

            var incidents = await _context.Incidents
                .Where(i => i.LocationId == id)
                .ToListAsync();

            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Header
            document.Add(new Paragraph("FACTURE DE LOCATION")
                .SetFont(boldFont)
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(new DeviceRgb(0, 51, 102)));

            document.Add(new Paragraph($"Location #{location.Id}")
                .SetFont(normalFont)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.GRAY));

            document.Add(new Paragraph($"Date d'émission : {DateTime.Now:dd/MM/yyyy}")
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Client Info
            document.Add(new Paragraph("INFORMATIONS CLIENT")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginTop(10));

            var clientTable = new Table(2);
            clientTable.SetWidth(UnitValue.CreatePercentValue(100));
            clientTable.AddCell(new Cell().Add(new Paragraph("Nom complet :").SetFont(boldFont)));
            clientTable.AddCell(new Cell().Add(new Paragraph(location.Client?.NomComplet ?? "N/A")));
            clientTable.AddCell(new Cell().Add(new Paragraph("CIN :").SetFont(boldFont)));
            clientTable.AddCell(new Cell().Add(new Paragraph(location.Client?.CIN ?? "N/A")));
            clientTable.AddCell(new Cell().Add(new Paragraph("Téléphone :").SetFont(boldFont)));
            clientTable.AddCell(new Cell().Add(new Paragraph(location.Client?.Telephone ?? "N/A")));
            clientTable.AddCell(new Cell().Add(new Paragraph("Email :").SetFont(boldFont)));
            clientTable.AddCell(new Cell().Add(new Paragraph(location.Client?.Email ?? "N/A")));
            document.Add(clientTable);

            // Vehicle Info
            document.Add(new Paragraph("INFORMATIONS VÉHICULE")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginTop(20));

            var vehicleTable = new Table(2);
            vehicleTable.SetWidth(UnitValue.CreatePercentValue(100));
            vehicleTable.AddCell(new Cell().Add(new Paragraph("Véhicule :").SetFont(boldFont)));
            vehicleTable.AddCell(new Cell().Add(new Paragraph($"{location.Voiture?.Marque} {location.Voiture?.Modele}")));
            vehicleTable.AddCell(new Cell().Add(new Paragraph("Immatriculation :").SetFont(boldFont)));
            vehicleTable.AddCell(new Cell().Add(new Paragraph(location.Voiture?.Immatriculation ?? "N/A")));
            vehicleTable.AddCell(new Cell().Add(new Paragraph("Catégorie :").SetFont(boldFont)));
            vehicleTable.AddCell(new Cell().Add(new Paragraph(location.Voiture?.Categorie ?? "N/A")));
            document.Add(vehicleTable);

            // Rental Period
            document.Add(new Paragraph("PÉRIODE DE LOCATION")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginTop(20));

            var periodTable = new Table(2);
            periodTable.SetWidth(UnitValue.CreatePercentValue(100));
            periodTable.AddCell(new Cell().Add(new Paragraph("Date de début :").SetFont(boldFont)));
            periodTable.AddCell(new Cell().Add(new Paragraph(location.DateDebut.ToString("dd/MM/yyyy"))));
            periodTable.AddCell(new Cell().Add(new Paragraph("Date de fin :").SetFont(boldFont)));
            periodTable.AddCell(new Cell().Add(new Paragraph(location.DateFin.ToString("dd/MM/yyyy"))));
            periodTable.AddCell(new Cell().Add(new Paragraph("Nombre de jours :").SetFont(boldFont)));
            periodTable.AddCell(new Cell().Add(new Paragraph($"{(location.DateFin - location.DateDebut).Days} jours")));
            periodTable.AddCell(new Cell().Add(new Paragraph("Prix par jour :").SetFont(boldFont)));
            periodTable.AddCell(new Cell().Add(new Paragraph($"{location.Voiture?.PrixParJour:N0} DH")));
            document.Add(periodTable);

            // Financial Summary
            document.Add(new Paragraph("RÉCAPITULATIF FINANCIER")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginTop(20));

            var financialTable = new Table(new float[] { 3, 1 });
            financialTable.SetWidth(UnitValue.CreatePercentValue(100));

            financialTable.AddCell(new Cell().Add(new Paragraph("Prix de location").SetFont(normalFont)));
            financialTable.AddCell(new Cell().Add(new Paragraph($"{location.PrixTotal:N0} DH").SetTextAlignment(TextAlignment.RIGHT)));

            if (location.ADommage && location.MontantDommage.HasValue && location.MontantDommage > 0)
            {
                financialTable.AddCell(new Cell().Add(new Paragraph("Frais de dommage").SetFont(normalFont).SetFontColor(ColorConstants.RED)));
                financialTable.AddCell(new Cell().Add(new Paragraph($"{location.MontantDommage:N0} DH").SetTextAlignment(TextAlignment.RIGHT).SetFontColor(ColorConstants.RED)));
            }

            if (incidents.Any())
            {
                foreach (var incident in incidents)
                {
                    financialTable.AddCell(new Cell().Add(new Paragraph($"Incident: {incident.TypeIncident}").SetFont(normalFont).SetFontSize(9)));
                    financialTable.AddCell(new Cell().Add(new Paragraph($"{incident.FraisSupplementaires:N0} DH").SetTextAlignment(TextAlignment.RIGHT).SetFontSize(9)));
                }
            }

            var totalIncidents = incidents.Sum(i => i.FraisSupplementaires);
            var totalFinal = location.TotalAvecDommage + totalIncidents;

            financialTable.AddCell(new Cell().Add(new Paragraph("TOTAL À PAYER").SetFont(boldFont).SetFontSize(14))
                .SetBackgroundColor(new DeviceRgb(0, 51, 102)).SetFontColor(ColorConstants.WHITE));
            financialTable.AddCell(new Cell().Add(new Paragraph($"{totalFinal:N0} DH").SetFont(boldFont).SetFontSize(14).SetTextAlignment(TextAlignment.RIGHT))
                .SetBackgroundColor(new DeviceRgb(0, 51, 102)).SetFontColor(ColorConstants.WHITE));

            document.Add(financialTable);

            // Incidents Details
            if (incidents.Any())
            {
                document.Add(new Paragraph("DÉTAILS DES INCIDENTS")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetMarginTop(20));

                foreach (var incident in incidents)
                {
                    document.Add(new Paragraph($"• {incident.TypeIncident} ({incident.DateIncident:dd/MM/yyyy})")
                        .SetFont(boldFont)
                        .SetMarginTop(5));
                    if (!string.IsNullOrEmpty(incident.Description))
                    {
                        document.Add(new Paragraph(incident.Description)
                            .SetFont(normalFont)
                            .SetFontSize(9)
                            .SetMarginLeft(15));
                    }
                    document.Add(new Paragraph($"Frais: {incident.FraisSupplementaires:N0} DH - {(incident.EstRembourse ? "Remboursé" : "Non remboursé")}")
                        .SetFont(normalFont)
                        .SetFontSize(9)
                        .SetMarginLeft(15)
                        .SetFontColor(incident.EstRembourse ? ColorConstants.GREEN : ColorConstants.RED));
                }
            }

            // Footer
            document.Add(new Paragraph("\n\nMerci de votre confiance !")
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(30));

            document.Add(new Paragraph("CarRental - Location de véhicules de qualité")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.GRAY));

            document.Close();

            var fileName = $"Facture_Location_{location.Id}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(stream.ToArray(), "application/pdf", fileName);
        }
    }
}