//using CarRental.Data;
//using CarRental.Models;
//using CarRental.Models.ViewModels;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;
//using System.Security.Cryptography;
//using System.Text;

//namespace CarRental.Controllers
//{
//    // Portail public + espace client authentifié
//    public class PortailController : Controller
//    {
//        private readonly ApplicationDbContext _db;
//        private readonly IWebHostEnvironment _env;

//        public PortailController(ApplicationDbContext db, IWebHostEnvironment env)
//        { _db = db; _env = env; }

//        // ══════════════════════════════════════════
//        // CATALOGUE VOITURES (public)
//        // ══════════════════════════════════════════
//        [HttpGet]
//        public async Task<IActionResult> Index(DateTime? dateDebut, DateTime? dateFin, string? categorie, string? search)
//        {
//            dateDebut ??= DateTime.Today;
//            dateFin ??= DateTime.Today.AddDays(1);

//            var voituresQuery = _db.Voitures.AsQueryable();

//            if (!string.IsNullOrEmpty(search))
//                voituresQuery = voituresQuery.Where(v => v.Marque.Contains(search) || v.Modele.Contains(search));
//            if (!string.IsNullOrEmpty(categorie))
//                voituresQuery = voituresQuery.Where(v => v.Categorie == categorie);

//            var toutes = await voituresQuery.ToListAsync();

//            // Filtrer par disponibilité sur la plage de dates
//            var locationsEnCours = await _db.Locations
//                .Where(l => l.Statut == "EnCours" &&
//                            l.DateDebut < dateFin && l.DateFin > dateDebut)
//                .Select(l => l.VoitureId).ToListAsync();

//            var demandesConfirmees = await _db.DemandesLocation
//                .Where(d => (d.Statut == "Confirmee" || d.Statut == "EnAttente") &&
//                             d.DateDebut < dateFin && d.DateFin > dateDebut)
//                .Select(d => d.VoitureId).ToListAsync();

//            var indisponibles = locationsEnCours.Union(demandesConfirmees).ToHashSet();
//            var disponibles = toutes.Where(v => !indisponibles.Contains(v.Id)).ToList();

//            var vm = new PortailViewModel
//            {
//                Voitures = disponibles,
//                DateRecherche = dateDebut.Value,
//                DateFin = dateFin.Value,
//                Categorie = categorie,
//                Recherche = search
//            };

//            ViewBag.ClientConnecte = await GetClientConnecte();
//            return View(vm);
//        }

//        // ══════════════════════════════════════════
//        // FICHE VOITURE (public)
//        // ══════════════════════════════════════════
//        public async Task<IActionResult> Voiture(int id, DateTime? dateDebut, DateTime? dateFin)
//        {
//            var v = await _db.Voitures.FindAsync(id);
//            if (v == null) return NotFound();
//            ViewBag.DateDebut = dateDebut ?? DateTime.Today;
//            ViewBag.DateFin = dateFin ?? DateTime.Today.AddDays(1);
//            ViewBag.ClientConnecte = await GetClientConnecte();
//            return View(v);
//        }

//        // ══════════════════════════════════════════
//        // DEMANDE DE LOCATION (public ou client)
//        // ══════════════════════════════════════════
//        [HttpGet]
//        public async Task<IActionResult> Demande(int? id, int? voitureId, DateTime? dateDebut, DateTime? dateFin)
//        {
//            var vid = id ?? voitureId ?? 0;
//            var voiture = await _db.Voitures.FindAsync(vid);
//            if (voiture == null) return NotFound();

//            var client = await GetClientConnecte();
//            var vm = new DemandeLocationViewModel
//            {
//                Voiture = voiture,
//                ClientConnecte = client,
//                EstAuthentifie = client != null
//            };
//            vm.Demande.VoitureId = vid;
//            vm.Demande.DateDebut = dateDebut ?? DateTime.Today;
//            vm.Demande.DateFin = dateFin ?? DateTime.Today.AddDays(1);

//            return View(vm);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Demande(DemandeLocationViewModel vm)
//        {
//            var voiture = await _db.Voitures.FindAsync(vm.Demande.VoitureId);
//            if (voiture == null) return NotFound();

//            var client = await GetClientConnecte();
//            vm.Voiture = voiture;
//            vm.ClientConnecte = client;
//            vm.EstAuthentifie = client != null;

//            if (vm.Demande.DateFin <= vm.Demande.DateDebut)
//            {
//                ModelState.AddModelError("Demande.DateFin", "La date de fin doit être après la date de début.");
//            }
//            if (vm.Demande.DateDebut < DateTime.Today)
//            {
//                ModelState.AddModelError("Demande.DateDebut", "La date de début ne peut pas être dans le passé.");
//            }

//            // Si non authentifié, vérifier infos obligatoires
//            if (client == null)
//            {
//                if (string.IsNullOrWhiteSpace(vm.Demande.NomDemandeur))
//                    ModelState.AddModelError("Demande.NomDemandeur", "Le nom est obligatoire.");
//                if (string.IsNullOrWhiteSpace(vm.Demande.PrenomDemandeur))
//                    ModelState.AddModelError("Demande.PrenomDemandeur", "Le prénom est obligatoire.");
//                if (string.IsNullOrWhiteSpace(vm.Demande.CINDemandeur))
//                    ModelState.AddModelError("Demande.CINDemandeur", "Le CIN est obligatoire.");
//                if (string.IsNullOrWhiteSpace(vm.Demande.TelephoneDemandeur))
//                    ModelState.AddModelError("Demande.TelephoneDemandeur", "Le téléphone est obligatoire.");
//                if (string.IsNullOrWhiteSpace(vm.Demande.EmailDemandeur))
//                    ModelState.AddModelError("Demande.EmailDemandeur", "L'email est obligatoire.");
//            }

//            // Vérifier si compte désactivé
//            if (client != null && !client.CompteActif)
//            {
//                TempData["Error"] = "Votre compte est désactivé. Contactez l'administrateur.";
//                return RedirectToAction(nameof(Index));
//            }

//            if (!ModelState.IsValid) return View(vm);

//            var demande = vm.Demande;
//            if (client != null) demande.ClientId = client.Id;
//            demande.Statut = "EnAttente";
//            demande.DateDemande = DateTime.Now;

//            _db.DemandesLocation.Add(demande);

//            // Créer notification admin
//            var notif = new Notification
//            {
//                Type = "NouvelleDemandeLocation",
//                Titre = "Nouvelle demande de location",
//                Message = $"{(client?.NomComplet ?? $"{demande.PrenomDemandeur} {demande.NomDemandeur}")} demande {voiture.Marque} {voiture.Modele} du {demande.DateDebut:dd/MM/yyyy} au {demande.DateFin:dd/MM/yyyy}",
//                LienAction = "/AdminDemandes/Index",
//                DateCreation = DateTime.Now
//            };
//            _db.Notifications.Add(notif);

//            await _db.SaveChangesAsync();

//            // Lier la notification à la demande
//            notif.LocationId = null;
//            await _db.SaveChangesAsync();

//            TempData["Success"] = "Votre demande a été envoyée ! L'administrateur vous contactera pour confirmation.";
//            return RedirectToAction(nameof(Confirmation), new { id = demande.Id });
//        }

//        public async Task<IActionResult> Confirmation(int id)
//        {
//            var demande = await _db.DemandesLocation
//                .Include(d => d.Voiture)
//                .FirstOrDefaultAsync(d => d.Id == id);
//            if (demande == null) return NotFound();
//            ViewBag.ClientConnecte = await GetClientConnecte();
//            return View(demande);
//        }

//        // ══════════════════════════════════════════
//        // AUTH CLIENT
//        // ══════════════════════════════════════════
//        [HttpGet]
//        public async Task<IActionResult> Login(string? returnUrl = null)
//        {
//            // Check if a client is already logged in via ClientCookie scheme
//            var client = await GetClientConnecte();
//            if (client != null)
//            {
//                return RedirectToAction(nameof(Espace));
//            }

//            ViewBag.ReturnUrl = returnUrl;
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
//        {
//            var client = await _db.Clients.FirstOrDefaultAsync(c => c.Email == email);
//            if (client == null || !VerifyPassword(password, client.PasswordHash))
//            {
//                ViewBag.Error = "Email ou mot de passe incorrect.";
//                ViewBag.ReturnUrl = returnUrl;
//                return View();
//            }
//            if (!client.CompteActif)
//            {
//                ViewBag.Error = $"Votre compte est désactivé. {(string.IsNullOrEmpty(client.MotifDesactivation) ? "" : "Motif : " + client.MotifDesactivation)}";
//                ViewBag.ReturnUrl = returnUrl;
//                return View();
//            }
//            await SignInClient(client);
//            TempData["Success"] = $"Bienvenue, {client.Prenom} !";
//            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
//            return RedirectToAction(nameof(Espace));
//        }

//        [HttpGet]
//        public async Task<IActionResult> Register()
//        {
//            if (await GetClientConnecte() != null) return RedirectToAction(nameof(Espace));
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Register(Client client, string password, string confirmPassword)
//        {
//            if (password != confirmPassword)
//                ModelState.AddModelError("confirmPassword", "Les mots de passe ne correspondent pas.");
//            if (await _db.Clients.AnyAsync(c => c.CIN == client.CIN))
//                ModelState.AddModelError("CIN", "Ce CIN existe déjà.");
//            if (await _db.Clients.AnyAsync(c => c.Email == client.Email))
//                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");

//            if (!ModelState.IsValid) return View(client);

//            client.PasswordHash = HashPassword(password);
//            client.DateInscription = DateTime.Now;
//            client.CompteActif = true;
//            _db.Clients.Add(client);
//            await _db.SaveChangesAsync();
//            await SignInClient(client);
//            TempData["Success"] = $"Bienvenue {client.Prenom} ! Votre compte a été créé.";
//            return RedirectToAction(nameof(Espace));
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Logout()
//        {
//            await HttpContext.SignOutAsync("ClientCookie");
//            return RedirectToAction(nameof(Index));
//        }

//        // ══════════════════════════════════════════
//        // ESPACE CLIENT (authentifié)
//        // ══════════════════════════════════════════
//        [Authorize(AuthenticationSchemes = "ClientCookie")]
//        public async Task<IActionResult> Espace()
//        {
//            var client = await GetClientConnecte();
//            if (client == null) return RedirectToAction(nameof(Login));

//            var locations = await _db.Locations
//                .Include(l => l.Voiture)
//                .Where(l => l.ClientId == client.Id)
//                .OrderByDescending(l => l.DateCreation).ToListAsync();

//            var demandes = await _db.DemandesLocation
//                .Include(d => d.Voiture)
//                .Where(d => d.ClientId == client.Id)
//                .OrderByDescending(d => d.DateDemande).ToListAsync();

//            var vm = new EspaceClientViewModel
//            {
//                Client = client,
//                Locations = locations,
//                Demandes = demandes
//            };
//            return View(vm);
//        }
//        [Authorize(AuthenticationSchemes = "ClientCookie")]
//        public async Task<IActionResult> DetailLocation(int id)
//        {
//            var client = await GetClientConnecte();
//            if (client == null) return RedirectToAction(nameof(Login));

//            var location = await _db.Locations
//                .Include(l => l.Voiture)
//                .Include(l => l.Client)
//                .FirstOrDefaultAsync(l => l.Id == id && l.ClientId == client.Id);

//            if (location == null) return NotFound();

//            var incidents = await _db.Incidents
//                .Where(i => i.LocationId == id).ToListAsync();

//            var vm = new LocationDetailViewModel { Location = location, Incidents = incidents };
//            return View(vm);
//        }
//        [Authorize(AuthenticationSchemes = "ClientCookie")]
//        public async Task<IActionResult> Facture(int id)
//        {
//            var client = await GetClientConnecte();
//            // Allow admin OR the client
//            Location? location = null;
//            if (client != null)
//                location = await _db.Locations.Include(l => l.Voiture).Include(l => l.Client)
//                    .FirstOrDefaultAsync(l => l.Id == id && l.ClientId == client.Id);
//            if (location == null && User.IsInRole("Admin"))
//                location = await _db.Locations.Include(l => l.Voiture).Include(l => l.Client)
//                    .FirstOrDefaultAsync(l => l.Id == id);
//            if (location == null) return NotFound();

//            var incidents = await _db.Incidents.Where(i => i.LocationId == id).ToListAsync();
//            var vm = new LocationDetailViewModel { Location = location, Incidents = incidents };
//            return View(vm);
//        }
//        [Authorize(AuthenticationSchemes = "ClientCookie")]
//        public async Task<IActionResult> Profil()
//        {
//            var client = await GetClientConnecte();
//            if (client == null) return RedirectToAction(nameof(Login));
//            return View(client);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Profil(Client model, string? ancienMotDePasse, string? nouveauMotDePasse)
//        {
//            var client = await GetClientConnecte();
//            if (client == null) return RedirectToAction(nameof(Login));

//            if (await _db.Clients.AnyAsync(c => c.CIN == model.CIN && c.Id != client.Id))
//                ModelState.AddModelError("CIN", "Ce CIN existe déjà.");
//            if (await _db.Clients.AnyAsync(c => c.Email == model.Email && c.Id != client.Id))
//                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");

//            if (!ModelState.IsValid) return View(client);

//            client.Nom = model.Nom; client.Prenom = model.Prenom;
//            client.Telephone = model.Telephone; client.Adresse = model.Adresse;
//            client.Email = model.Email;

//            if (!string.IsNullOrEmpty(nouveauMotDePasse))
//            {
//                if (!VerifyPassword(ancienMotDePasse ?? "", client.PasswordHash))
//                { ModelState.AddModelError("ancienMotDePasse", "Ancien mot de passe incorrect."); return View(client); }
//                client.PasswordHash = HashPassword(nouveauMotDePasse);
//            }

//            await _db.SaveChangesAsync();
//            TempData["Success"] = "Profil mis à jour !";
//            return RedirectToAction(nameof(Profil));
//        }

//        // ══════════════════════════════════════════
//        // HELPERS
//        // ══════════════════════════════════════════
//        private async Task<Client?> GetClientConnecte()
//        {
//            var claim = User.FindFirst("ClientId");
//            if (claim == null) return null;
//            if (!int.TryParse(claim.Value, out int clientId)) return null;
//            return await _db.Clients.FindAsync(clientId);
//        }

//        private async Task SignInClient(Client client)
//        {
//            var claims = new List<Claim>
//            {
//                new Claim("ClientId", client.Id.ToString()),
//                new Claim(ClaimTypes.Name, client.NomComplet),
//                new Claim(ClaimTypes.Email, client.Email),
//                new Claim(ClaimTypes.Role, "Client")
//            };
//            var identity = new ClaimsIdentity(claims, "ClientCookie");
//            await HttpContext.SignInAsync("ClientCookie", new ClaimsPrincipal(identity),
//                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) });
//        }

//        private static string HashPassword(string password)
//        {
//            using var sha = SHA256.Create();
//            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password + "CarRentalSalt2024")));
//        }

//        private static bool VerifyPassword(string password, string? hash)
//        {
//            if (string.IsNullOrEmpty(hash)) return false;
//            return HashPassword(password) == hash;
//        }
//    }
//}
using CarRental.Data;
using CarRental.Models;
using CarRental.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CarRental.Controllers
{
    // Portail public + espace client authentifié
    public class PortailController : Controller, IAsyncActionFilter
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public PortailController(ApplicationDbContext db, IWebHostEnvironment env)
        { _db = db; _env = env; }

        // Définit ViewBag.ClientConnecte automatiquement pour toutes les actions
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ViewBag.ClientConnecte = await GetClientConnecte();
            await next();
        }

        // ══════════════════════════════════════════
        // CATALOGUE VOITURES (public)
        // ══════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Index(DateTime? dateDebut, DateTime? dateFin, string? categorie, string? search)
        {
            dateDebut ??= DateTime.Today;
            dateFin ??= DateTime.Today.AddDays(1);

            // Récupérer toutes les voitures disponibles (comme l'admin)
            var voituresQuery = _db.Voitures.Where(v => v.EstDisponible).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                voituresQuery = voituresQuery.Where(v => v.Marque.Contains(search) || v.Modele.Contains(search));
            if (!string.IsNullOrEmpty(categorie))
                voituresQuery = voituresQuery.Where(v => v.Categorie == categorie);

            var disponibles = await voituresQuery.OrderBy(v => v.Marque).ThenBy(v => v.Modele).ToListAsync();

            var vm = new PortailViewModel
            {
                Voitures = disponibles,
                DateRecherche = dateDebut.Value,
                DateFin = dateFin.Value,
                Categorie = categorie,
                Recherche = search
            };

            ViewBag.ClientConnecte = await GetClientConnecte();
            return View(vm);
        }

        // ══════════════════════════════════════════
        // FICHE VOITURE (public)
        // ══════════════════════════════════════════
        public async Task<IActionResult> Voiture(int id, DateTime? dateDebut, DateTime? dateFin)
        {
            var v = await _db.Voitures.FindAsync(id);
            if (v == null) return NotFound();
            ViewBag.DateDebut = dateDebut ?? DateTime.Today;
            ViewBag.DateFin = dateFin ?? DateTime.Today.AddDays(1);
            return View(v);
        }

        // ══════════════════════════════════════════
        // DEMANDE DE LOCATION (public ou client)
        // ══════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Demande(int? id, int? voitureId, DateTime? dateDebut, DateTime? dateFin)
        {
            var vid = id ?? voitureId ?? 0;
            var voiture = await _db.Voitures.FindAsync(vid);
            if (voiture == null) return NotFound();

            var client = await GetClientConnecte();
            var vm = new DemandeLocationViewModel
            {
                Voiture = voiture,
                ClientConnecte = client,
                EstAuthentifie = client != null
            };
            vm.Demande.VoitureId = vid;
            vm.Demande.DateDebut = dateDebut ?? DateTime.Today;
            vm.Demande.DateFin = dateFin ?? DateTime.Today.AddDays(1);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Demande(DemandeLocationViewModel vm)
        {
            var voiture = await _db.Voitures.FindAsync(vm.Demande.VoitureId);
            if (voiture == null) return NotFound();

            var client = await GetClientConnecte();
            vm.Voiture = voiture;

            // ── Mode CONNEXION : le client veut se connecter et réserver ──
            if (vm.Mode == "connexion" && client == null)
            {
                if (string.IsNullOrWhiteSpace(vm.LoginEmail) || string.IsNullOrWhiteSpace(vm.LoginPassword))
                {
                    ModelState.AddModelError("LoginEmail", "Email et mot de passe obligatoires.");
                }
                else
                {
                    var clientLogin = await _db.Clients.FirstOrDefaultAsync(c => c.Email == vm.LoginEmail);
                    if (clientLogin == null || !VerifyPassword(vm.LoginPassword, clientLogin.PasswordHash))
                        ModelState.AddModelError("LoginEmail", "Email ou mot de passe incorrect.");
                    else if (!clientLogin.CompteActif)
                        ModelState.AddModelError("LoginEmail", "Votre compte est désactivé.");
                    else
                        client = clientLogin;
                }
            }

            vm.ClientConnecte = client;
            vm.EstAuthentifie = client != null;

            // ── Validation dates ──
            if (vm.Demande.DateFin <= vm.Demande.DateDebut)
                ModelState.AddModelError("Demande.DateFin", "La date de fin doit être après la date de début.");
            if (vm.Demande.DateDebut < DateTime.Today)
                ModelState.AddModelError("Demande.DateDebut", "La date de début ne peut pas être dans le passé.");

            // ── Mode NOUVEAU CLIENT : vérifier infos + mot de passe ──
            if (vm.Mode == "nouveau" && client == null)
            {
                if (string.IsNullOrWhiteSpace(vm.Demande.NomDemandeur))
                    ModelState.AddModelError("Demande.NomDemandeur", "Le nom est obligatoire.");
                if (string.IsNullOrWhiteSpace(vm.Demande.PrenomDemandeur))
                    ModelState.AddModelError("Demande.PrenomDemandeur", "Le prénom est obligatoire.");
                if (string.IsNullOrWhiteSpace(vm.Demande.CINDemandeur))
                    ModelState.AddModelError("Demande.CINDemandeur", "Le CIN est obligatoire.");
                if (string.IsNullOrWhiteSpace(vm.Demande.TelephoneDemandeur))
                    ModelState.AddModelError("Demande.TelephoneDemandeur", "Le téléphone est obligatoire.");
                if (string.IsNullOrWhiteSpace(vm.Demande.EmailDemandeur))
                    ModelState.AddModelError("Demande.EmailDemandeur", "L'email est obligatoire.");
                if (string.IsNullOrWhiteSpace(vm.Demande.MotDePasseDemandeur) || vm.Demande.MotDePasseDemandeur.Length < 6)
                    ModelState.AddModelError("Demande.MotDePasseDemandeur", "Le mot de passe doit contenir au moins 6 caractères.");
                else if (vm.Demande.MotDePasseDemandeur != vm.Demande.ConfirmMotDePasse)
                    ModelState.AddModelError("Demande.ConfirmMotDePasse", "Les mots de passe ne correspondent pas.");
                if (!string.IsNullOrWhiteSpace(vm.Demande.EmailDemandeur) && await _db.Clients.AnyAsync(c => c.Email == vm.Demande.EmailDemandeur))
                    ModelState.AddModelError("Demande.EmailDemandeur", "Cet email est déjà utilisé. Utilisez 'Se connecter'.");
                if (!string.IsNullOrWhiteSpace(vm.Demande.CINDemandeur) && await _db.Clients.AnyAsync(c => c.CIN == vm.Demande.CINDemandeur))
                    ModelState.AddModelError("Demande.CINDemandeur", "Ce CIN est déjà enregistré.");
            }

            // Vérifier si compte désactivé
            if (client != null && !client.CompteActif)
            {
                TempData["Error"] = "Votre compte est désactivé. Contactez l'administrateur.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid) return View(vm);

            // ── Créer le compte si nouveau client ──
            if (vm.Mode == "nouveau" && client == null)
            {
                var newClient = new Client
                {
                    Nom = vm.Demande.NomDemandeur ?? "",
                    Prenom = vm.Demande.PrenomDemandeur ?? "",
                    CIN = vm.Demande.CINDemandeur ?? "",
                    Telephone = vm.Demande.TelephoneDemandeur ?? "",
                    Email = vm.Demande.EmailDemandeur ?? "",
                    Adresse = vm.Demande.AdresseDemandeur,
                    PasswordHash = HashPassword(vm.Demande.MotDePasseDemandeur!),
                    DateInscription = DateTime.Now,
                    CompteActif = true
                };
                _db.Clients.Add(newClient);
                await _db.SaveChangesAsync();
                client = newClient;
                // Connecter le client automatiquement
                await SignInClient(client);
                vm.ClientConnecte = client;
            }
            else if (vm.Mode == "connexion" && client != null)
            {
                // Connecter le client
                await SignInClient(client);
            }

            var demande = vm.Demande;
            if (client != null) demande.ClientId = client.Id;
            demande.Statut = "EnAttente";
            demande.DateDemande = DateTime.Now;

            _db.DemandesLocation.Add(demande);

            // Notification admin
            _db.Notifications.Add(new Notification
            {
                Type = "NouvelleDemandeLocation",
                Titre = "Nouvelle demande de location",
                Message = $"{(client?.NomComplet ?? $"{demande.PrenomDemandeur} {demande.NomDemandeur}")} demande {voiture.Marque} {voiture.Modele} du {demande.DateDebut:dd/MM/yyyy} au {demande.DateFin:dd/MM/yyyy}",
                LienAction = "/AdminDemandes/Index",
                DateCreation = DateTime.Now
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Votre demande a été envoyée ! L'administrateur vous contactera pour confirmation.";
            return RedirectToAction(nameof(Confirmation), new { id = demande.Id });
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var demande = await _db.DemandesLocation
                .Include(d => d.Voiture)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (demande == null) return NotFound();
            return View(demande);
        }

        // ══════════════════════════════════════════
        // AUTH CLIENT
        // ══════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            // Check if a client is already logged in via ClientCookie scheme
            var client = await GetClientConnecte();
            if (client != null)
            {
                return RedirectToAction(nameof(Espace));
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var client = await _db.Clients.FirstOrDefaultAsync(c => c.Email == email);
            if (client == null || !VerifyPassword(password, client.PasswordHash))
            {
                ViewBag.Error = "Email ou mot de passe incorrect.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
            if (!client.CompteActif)
            {
                ViewBag.Error = $"Votre compte est désactivé. {(string.IsNullOrEmpty(client.MotifDesactivation) ? "" : "Motif : " + client.MotifDesactivation)}";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
            await SignInClient(client);
            TempData["Success"] = $"Bienvenue, {client.Prenom} !";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(Espace));
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            if (await GetClientConnecte() != null) return RedirectToAction(nameof(Espace));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Client client, string password, string confirmPassword)
        {
            if (password != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Les mots de passe ne correspondent pas.");
            if (await _db.Clients.AnyAsync(c => c.CIN == client.CIN))
                ModelState.AddModelError("CIN", "Ce CIN existe déjà.");
            if (await _db.Clients.AnyAsync(c => c.Email == client.Email))
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");

            if (!ModelState.IsValid) return View(client);

            client.PasswordHash = HashPassword(password);
            client.DateInscription = DateTime.Now;
            client.CompteActif = true;
            _db.Clients.Add(client);
            await _db.SaveChangesAsync();
            await SignInClient(client);
            TempData["Success"] = $"Bienvenue {client.Prenom} ! Votre compte a été créé.";
            return RedirectToAction(nameof(Espace));
        }

        [HttpGet, HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("ClientCookie");
            return RedirectToAction(nameof(Index));
        }

        // ══════════════════════════════════════════
        // ESPACE CLIENT (authentifié)
        // ══════════════════════════════════════════
        [Authorize(AuthenticationSchemes = "ClientCookie")]
        public async Task<IActionResult> Espace()
        {
            var client = await GetClientConnecte();
            if (client == null) return RedirectToAction(nameof(Login));

            var locations = await _db.Locations
                .Include(l => l.Voiture)
                .Where(l => l.ClientId == client.Id)
                .OrderByDescending(l => l.DateCreation).ToListAsync();

            var demandes = await _db.DemandesLocation
                .Include(d => d.Voiture)
                .Where(d => d.ClientId == client.Id)
                .OrderByDescending(d => d.DateDemande).ToListAsync();

            var notifications = await _db.Notifications
                .Where(n => n.ClientId == client.Id)
                .OrderByDescending(n => n.DateCreation).Take(20).ToListAsync();

            var voituresDisponibles = await _db.Voitures
                .Where(v => v.EstDisponible)
                .OrderBy(v => v.Marque).ThenBy(v => v.Modele)
                .ToListAsync();

            var vm = new EspaceClientViewModel
            {
                Client = client,
                Locations = locations,
                Demandes = demandes,
                Notifications = notifications,
                VoituresDisponibles = voituresDisponibles
            };
            return View(vm);
        }
        [Authorize(AuthenticationSchemes = "ClientCookie")]
        public async Task<IActionResult> DetailLocation(int id)
        {
            var client = await GetClientConnecte();
            if (client == null) return RedirectToAction(nameof(Login));

            var location = await _db.Locations
                .Include(l => l.Voiture)
                .Include(l => l.Client)
                .FirstOrDefaultAsync(l => l.Id == id && l.ClientId == client.Id);

            if (location == null) return NotFound();

            var incidents = await _db.Incidents
                .Where(i => i.LocationId == id).ToListAsync();

            var vm = new LocationDetailViewModel { Location = location, Incidents = incidents };
            return View(vm);
        }
        [Authorize(AuthenticationSchemes = "ClientCookie")]
        public async Task<IActionResult> Facture(int id)
        {
            var client = await GetClientConnecte();
            // Allow admin OR the client
            Location? location = null;
            if (client != null)
                location = await _db.Locations.Include(l => l.Voiture).Include(l => l.Client)
                    .FirstOrDefaultAsync(l => l.Id == id && l.ClientId == client.Id);
            if (location == null && User.IsInRole("Admin"))
                location = await _db.Locations.Include(l => l.Voiture).Include(l => l.Client)
                    .FirstOrDefaultAsync(l => l.Id == id);
            if (location == null) return NotFound();

            var incidents = await _db.Incidents.Where(i => i.LocationId == id).ToListAsync();
            var vm = new LocationDetailViewModel { Location = location, Incidents = incidents };
            return View(vm);
        }
        [Authorize(AuthenticationSchemes = "ClientCookie")]
        public async Task<IActionResult> Profil()
        {
            var client = await GetClientConnecte();
            if (client == null) return RedirectToAction(nameof(Login));
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profil(Client model, string? ancienMotDePasse, string? nouveauMotDePasse)
        {
            var client = await GetClientConnecte();
            if (client == null) return RedirectToAction(nameof(Login));

            if (await _db.Clients.AnyAsync(c => c.CIN == model.CIN && c.Id != client.Id))
                ModelState.AddModelError("CIN", "Ce CIN existe déjà.");
            if (await _db.Clients.AnyAsync(c => c.Email == model.Email && c.Id != client.Id))
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");

            if (!ModelState.IsValid) return View(client);

            client.Nom = model.Nom; client.Prenom = model.Prenom;
            client.Telephone = model.Telephone; client.Adresse = model.Adresse;
            client.Email = model.Email;

            if (!string.IsNullOrEmpty(nouveauMotDePasse))
            {
                if (!VerifyPassword(ancienMotDePasse ?? "", client.PasswordHash))
                { ModelState.AddModelError("ancienMotDePasse", "Ancien mot de passe incorrect."); return View(client); }
                client.PasswordHash = HashPassword(nouveauMotDePasse);
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Profil mis à jour !";
            return RedirectToAction(nameof(Profil));
        }

        // ══════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════
        private async Task<Client?> GetClientConnecte()
        {
            var result = await HttpContext.AuthenticateAsync("ClientCookie");
            if (!result.Succeeded) return null;
            var claim = result.Principal?.FindFirst("ClientId");
            if (claim == null) return null;
            if (!int.TryParse(claim.Value, out int clientId)) return null;
            return await _db.Clients.FindAsync(clientId);
        }

        private async Task SignInClient(Client client)
        {
            var claims = new List<Claim>
            {
                new Claim("ClientId", client.Id.ToString()),
                new Claim(ClaimTypes.Name, client.NomComplet),
                new Claim(ClaimTypes.Email, client.Email),
                new Claim(ClaimTypes.Role, "Client")
            };
            var identity = new ClaimsIdentity(claims, "ClientCookie");
            await HttpContext.SignInAsync("ClientCookie", new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) });
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password + "CarRentalSalt2024")));
        }

        private static bool VerifyPassword(string password, string? hash)
        {
            if (string.IsNullOrEmpty(hash)) return false;
            return HashPassword(password) == hash;
        }
    }
}