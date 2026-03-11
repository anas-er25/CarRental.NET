using System.ComponentModel.DataAnnotations;

namespace CarRental.Models
{
    public class Admin
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
        [StringLength(50)]
        [Display(Name = "Nom d'utilisateur")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [Display(Name = "Mot de passe (hash)")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom complet est obligatoire")]
        [StringLength(100)]
        [Display(Name = "Nom complet")]
        public string NomComplet { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email invalide")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Compte actif")]
        public bool CompteActif { get; set; } = true;

        [Display(Name = "Date de création")]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [Display(Name = "Dernière connexion")]
        public DateTime? DerniereConnexion { get; set; }
    }
}
