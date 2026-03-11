using System.ComponentModel.DataAnnotations;

namespace CarRental.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(50)]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(50)]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le CIN est obligatoire")]
        [StringLength(20)]
        [Display(Name = "CIN")]
        public string CIN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le téléphone est obligatoire")]
        [Display(Name = "Téléphone")]
        public string Telephone { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Adresse")]
        public string? Adresse { get; set; }

        [Display(Name = "Date d'inscription")]
        public DateTime DateInscription { get; set; } = DateTime.Now;

        // ── Authentification client ──
        [Display(Name = "Mot de passe (hash)")]
        public string? PasswordHash { get; set; }

        [Display(Name = "Compte activé")]
        public bool CompteActif { get; set; } = true;

        [Display(Name = "Motif désactivation")]
        public string? MotifDesactivation { get; set; }

        [Display(Name = "Date désactivation")]
        public DateTime? DateDesactivation { get; set; }

        [Display(Name = "Remarque admin")]
        public string? RemarqueAdmin { get; set; }

        // Navigation
        public ICollection<Location> Locations { get; set; } = new List<Location>();

        // Computed
        public string NomComplet => $"{Prenom} {Nom}";
        public bool ACompte => !string.IsNullOrEmpty(PasswordHash);
    }
}
