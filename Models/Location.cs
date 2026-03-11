using System.ComponentModel.DataAnnotations;

namespace CarRental.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Voiture")]
        public int VoitureId { get; set; }
        public Voiture? Voiture { get; set; }

        [Required]
        [Display(Name = "Client")]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Required(ErrorMessage = "La date de début est obligatoire")]
        [Display(Name = "Date de début")]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }

        [Required(ErrorMessage = "La date de fin est obligatoire")]
        [Display(Name = "Date de fin")]
        [DataType(DataType.Date)]
        public DateTime DateFin { get; set; }

        [Display(Name = "Prix Total (DH)")]
        public decimal PrixTotal { get; set; }

        [Display(Name = "Statut")]
        public string Statut { get; set; } = "EnCours";

        [Display(Name = "Date de création")]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [Display(Name = "Remarques")]
        public string? Remarques { get; set; }

        [Display(Name = "Accident / Dommage")]
        public bool ADommage { get; set; } = false;

        [Display(Name = "Description du dommage")]
        public string? DescriptionDommage { get; set; }

        [Display(Name = "Montant à rembourser (DH)")]
        [Range(0, 999999)]
        public decimal? MontantDommage { get; set; }

        [Display(Name = "Dommage remboursé")]
        public bool DommageRembourse { get; set; } = false;

        public int NombreJours => (DateFin - DateDebut).Days;
        public decimal TotalAvecDommage => PrixTotal + (MontantDommage ?? 0);
    }
}
