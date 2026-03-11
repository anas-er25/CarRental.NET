using System.ComponentModel.DataAnnotations;

namespace CarRental.Models
{
    public class IncidentVehicule
    {
        public int Id { get; set; }

        [Required]
        public int VoitureId { get; set; }
        public Voiture? Voiture { get; set; }

        [Required]
        public int LocationId { get; set; }
        public Location? Location { get; set; }

        public int ClientId { get; set; }
        public Client? Client { get; set; }

        // Types: Accident, Rayure, BrisDeGlace, DeteriorationInterieure, Autre
        [Required]
        [Display(Name = "Type d'incident")]
        public string TypeIncident { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Frais supplémentaires (DH)")]
        [Range(0, 999999)]
        public decimal FraisSupplementaires { get; set; } = 0;

        [Display(Name = "Remboursé")]
        public bool EstRembourse { get; set; } = false;

        [Display(Name = "Date de l'incident")]
        public DateTime DateIncident { get; set; } = DateTime.Now;

        [Display(Name = "Photo de l'incident")]
        public string? PhotoUrl { get; set; }
    }
}
