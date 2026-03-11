//using System.ComponentModel.DataAnnotations;

//namespace CarRental.Models
//{
//    public class DemandeLocation
//    {
//        public int Id { get; set; }

//        // Voiture demandée
//        [Required]
//        public int VoitureId { get; set; }
//        public Voiture? Voiture { get; set; }

//        // Client (null si non authentifié)
//        public int? ClientId { get; set; }
//        public Client? Client { get; set; }

//        // Infos si client non authentifié
//        public string? NomDemandeur { get; set; }
//        public string? PrenomDemandeur { get; set; }
//        public string? CINDemandeur { get; set; }
//        public string? TelephoneDemandeur { get; set; }
//        public string? EmailDemandeur { get; set; }
//        public string? AdresseDemandeur { get; set; }

//        [Required]
//        [DataType(DataType.Date)]
//        public DateTime DateDebut { get; set; }

//        [Required]
//        [DataType(DataType.Date)]
//        public DateTime DateFin { get; set; }

//        public string? Remarques { get; set; }

//        // Statut: EnAttente, Confirmee, Rejetee, Annulee
//        public string Statut { get; set; } = "EnAttente";

//        public DateTime DateDemande { get; set; } = DateTime.Now;

//        public string? NoteAdmin { get; set; }

//        public DateTime? DateTraitement { get; set; }

//        // Si confirmée, la location créée
//        public int? LocationId { get; set; }
//        public Location? Location { get; set; }

//        // Computed
//        public string NomCompletDemandeur => ClientId.HasValue
//            ? $"{Client?.Prenom} {Client?.Nom}"
//            : $"{PrenomDemandeur} {NomDemandeur}";

//        public int NombreJours => Math.Max(1, (DateFin - DateDebut).Days);
//    }
//}
using System.ComponentModel.DataAnnotations;

namespace CarRental.Models
{
    public class DemandeLocation
    {
        public int Id { get; set; }

        // Voiture demandée
        [Required]
        public int VoitureId { get; set; }
        public Voiture? Voiture { get; set; }

        // Client (null si non authentifié)
        public int? ClientId { get; set; }
        public Client? Client { get; set; }

        // Infos si client non authentifié
        public string? NomDemandeur { get; set; }
        public string? PrenomDemandeur { get; set; }
        public string? CINDemandeur { get; set; }
        public string? TelephoneDemandeur { get; set; }
        public string? EmailDemandeur { get; set; }
        public string? AdresseDemandeur { get; set; }

        // Mot de passe pour créer un compte lors de la demande
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? MotDePasseDemandeur { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? ConfirmMotDePasse { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateFin { get; set; }

        public string? Remarques { get; set; }

        // Statut: EnAttente, Confirmee, Rejetee, Annulee
        public string Statut { get; set; } = "EnAttente";

        public DateTime DateDemande { get; set; } = DateTime.Now;

        public string? NoteAdmin { get; set; }

        public DateTime? DateTraitement { get; set; }

        // Si confirmée, la location créée
        public int? LocationId { get; set; }
        public Location? Location { get; set; }

        // Computed
        public string NomCompletDemandeur => ClientId.HasValue
            ? $"{Client?.Prenom} {Client?.Nom}"
            : $"{PrenomDemandeur} {NomDemandeur}";

        public int NombreJours => Math.Max(1, (DateFin - DateDebut).Days);
    }
}