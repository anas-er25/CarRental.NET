using System.ComponentModel.DataAnnotations;

namespace CarRental.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;
        // Types: NouvelleDemandeLocation, ConfirmationAttente, RetourPrevu, RetourTardif,
        //        Annulation, SignalementDommage, RetourAnticipe, CompteDesactive

        [Required]
        public string Titre { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? LienAction { get; set; }   // URL vers la page concernée

        public bool EstLu { get; set; } = false;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Relations optionnelles pour navigation rapide
        public int? LocationId { get; set; }
        public Location? Location { get; set; }

        public int? ClientId { get; set; }
        public Client? Client { get; set; }
    }
}
