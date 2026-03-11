//using CarRental.Models;

//namespace CarRental.Models.ViewModels
//{
//    // ── Admin Dashboard ──
//    public class DashboardViewModel
//    {
//        public int TotalVoitures { get; set; }
//        public int VoituresDisponibles { get; set; }
//        public int VoituresLouees { get; set; }
//        public int TotalClients { get; set; }
//        public int LocationsEnCours { get; set; }
//        public int LocationsTerminees { get; set; }
//        public decimal RevenusTotal { get; set; }
//        public decimal RevenusMois { get; set; }
//        public List<Location> DernieresLocations { get; set; } = new();
//        public List<Voiture> VoituresDispoList { get; set; } = new();
//        public int NotifsNonLues { get; set; }
//        public List<Notification> DernieresNotifs { get; set; } = new();
//        public int DemandesEnAttente { get; set; }
//    }

//    // ── Admin create location ──
//    public class LocationCreateViewModel
//    {
//        public Location Location { get; set; } = new();
//        public List<Voiture> VoituresDisponibles { get; set; } = new();
//        public List<Client> Clients { get; set; } = new();
//    }

//    // ── Portail public (catalogue voitures) ──
//    public class PortailViewModel
//    {
//        public List<Voiture> Voitures { get; set; } = new();
//        public DateTime DateRecherche { get; set; } = DateTime.Today;
//        public DateTime DateFin { get; set; } = DateTime.Today.AddDays(1);
//        public string? Categorie { get; set; }
//        public string? Recherche { get; set; }
//    }

//    // ── Demande location (public ou client) ──
//    public class DemandeLocationViewModel
//    {
//        public DemandeLocation Demande { get; set; } = new();
//        public Voiture? Voiture { get; set; }
//        public Client? ClientConnecte { get; set; }
//        public bool EstAuthentifie { get; set; }
//    }

//    // ── Espace client connecté ──
//    public class EspaceClientViewModel
//    {
//        public Client Client { get; set; } = null!;
//        public List<Location> Locations { get; set; } = new();
//        public List<DemandeLocation> Demandes { get; set; } = new();
//        public int LocActives => Locations.Count(l => l.Statut == "EnCours");
//        public int LocTerminees => Locations.Count(l => l.Statut == "Terminée");
//        public decimal TotalDepense => Locations.Where(l => l.Statut == "Terminée").Sum(l => l.TotalAvecDommage);
//    }

//    // ── Détail location + facture ──
//    public class LocationDetailViewModel
//    {
//        public Location Location { get; set; } = null!;
//        public List<IncidentVehicule> Incidents { get; set; } = new();
//        public decimal TotalIncidents => Incidents.Sum(i => i.FraisSupplementaires);
//        public decimal TotalFacture => Location.TotalAvecDommage + TotalIncidents;
//    }

//    // ── Retour voiture avec incidents ──
//    public class RetourViewModel
//    {
//        public Location Location { get; set; } = null!;
//        public List<IncidentVehicule> IncidentsExistants { get; set; } = new();
//        public string? TypeIncident { get; set; }
//        public string? DescriptionIncident { get; set; }
//        public decimal FraisIncident { get; set; }
//        public bool ADommage { get; set; }
//        public string? DescriptionDommage { get; set; }
//        public decimal? MontantDommage { get; set; }
//        public bool DommageRembourse { get; set; }
//        public bool RetourAnticipe => Location.Id > 0 && DateTime.Today < Location.DateFin;
//        public bool RetourTardif => Location.Id > 0 && DateTime.Today > Location.DateFin;
//        public int JoursEcart => Math.Abs((DateTime.Today - Location.DateFin).Days);
//    }

//    // ── Gestion compte client (admin) ──
//    public class GestionCompteViewModel
//    {
//        public Client Client { get; set; } = null!;
//        public string? MotifAction { get; set; }
//        public List<Location> Locations { get; set; } = new();
//        public List<IncidentVehicule> Incidents { get; set; } = new();
//        public List<DemandeLocation> Demandes { get; set; } = new();
//    }

//    // ── Gestion demandes admin ──
//    public class DemandeAdminViewModel
//    {
//        public DemandeLocation Demande { get; set; } = null!;
//        public Voiture? Voiture { get; set; }
//        public Client? ClientExistant { get; set; }
//        public string? NoteAdmin { get; set; }
//    }
//}
using CarRental.Models;

namespace CarRental.Models.ViewModels
{
    // ── Admin Dashboard ──
    public class DashboardViewModel
    {
        public int TotalVoitures { get; set; }
        public int VoituresDisponibles { get; set; }
        public int VoituresLouees { get; set; }
        public int TotalClients { get; set; }
        public int LocationsEnCours { get; set; }
        public int LocationsTerminees { get; set; }
        public decimal RevenusTotal { get; set; }
        public decimal RevenusMois { get; set; }
        public List<Location> DernieresLocations { get; set; } = new();
        public List<Voiture> VoituresDispoList { get; set; } = new();
        public int NotifsNonLues { get; set; }
        public List<Notification> DernieresNotifs { get; set; } = new();
        public int DemandesEnAttente { get; set; }
    }

    // ── Admin create location ──
    public class LocationCreateViewModel
    {
        public Location Location { get; set; } = new();
        public List<Voiture> VoituresDisponibles { get; set; } = new();
        public List<Client> Clients { get; set; } = new();
    }

    // ── Portail public (catalogue voitures) ──
    public class PortailViewModel
    {
        public List<Voiture> Voitures { get; set; } = new();
        public DateTime DateRecherche { get; set; } = DateTime.Today;
        public DateTime DateFin { get; set; } = DateTime.Today.AddDays(1);
        public string? Categorie { get; set; }
        public string? Recherche { get; set; }
    }

    // ── Demande location (public ou client) ──
    public class DemandeLocationViewModel
    {
        public DemandeLocation Demande { get; set; } = new();
        public Voiture? Voiture { get; set; }
        public Client? ClientConnecte { get; set; }
        public bool EstAuthentifie { get; set; }

        // Pour le mode connexion inline
        public string? LoginEmail { get; set; }
        public string? LoginPassword { get; set; }
        public string Mode { get; set; } = "nouveau"; // "nouveau" ou "connexion"
    }

    // ── Espace client connecté ──
    public class EspaceClientViewModel
    {
        public Client Client { get; set; } = null!;
        public List<Location> Locations { get; set; } = new();
        public List<DemandeLocation> Demandes { get; set; } = new();
        public List<Notification> Notifications { get; set; } = new();
        public List<Voiture> VoituresDisponibles { get; set; } = new();
        public int LocActives => Locations.Count(l => l.Statut == "EnCours");
        public int LocTerminees => Locations.Count(l => l.Statut == "Terminée");
        public decimal TotalDepense => Locations.Where(l => l.Statut == "Terminée").Sum(l => l.TotalAvecDommage);
        public int NotifsNonLues => Notifications.Count(n => !n.EstLu);
    }

    // ── Détail location + facture ──
    public class LocationDetailViewModel
    {
        public Location Location { get; set; } = null!;
        public List<IncidentVehicule> Incidents { get; set; } = new();
        public decimal TotalIncidents => Incidents.Sum(i => i.FraisSupplementaires);
        public decimal TotalFacture => Location.TotalAvecDommage + TotalIncidents;
    }

    // ── Retour voiture avec incidents ──
    public class RetourViewModel
    {
        public Location Location { get; set; } = null!;
        public List<IncidentVehicule> IncidentsExistants { get; set; } = new();
        public string? TypeIncident { get; set; }
        public string? DescriptionIncident { get; set; }
        public decimal FraisIncident { get; set; }
        public bool ADommage { get; set; }
        public string? DescriptionDommage { get; set; }
        public decimal? MontantDommage { get; set; }
        public bool DommageRembourse { get; set; }
        public bool RetourAnticipe => Location.Id > 0 && DateTime.Today < Location.DateFin;
        public bool RetourTardif => Location.Id > 0 && DateTime.Today > Location.DateFin;
        public int JoursEcart => Math.Abs((DateTime.Today - Location.DateFin).Days);
    }

    // ── Gestion compte client (admin) ──
    public class GestionCompteViewModel
    {
        public Client Client { get; set; } = null!;
        public string? MotifAction { get; set; }
        public List<Location> Locations { get; set; } = new();
        public List<IncidentVehicule> Incidents { get; set; } = new();
        public List<DemandeLocation> Demandes { get; set; } = new();
    }

    // ── Gestion demandes admin ──
    public class DemandeAdminViewModel
    {
        public DemandeLocation Demande { get; set; } = null!;
        public Voiture? Voiture { get; set; }
        public Client? ClientExistant { get; set; }
        public string? NoteAdmin { get; set; }
    }
}