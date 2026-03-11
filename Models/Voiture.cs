using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models
{
    public class Voiture
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La marque est obligatoire")]
        [StringLength(50)]
        [Display(Name = "Marque")]
        public string Marque { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le modèle est obligatoire")]
        [StringLength(50)]
        [Display(Name = "Modèle")]
        public string Modele { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'année est obligatoire")]
        [Range(2000, 2030)]
        [Display(Name = "Année")]
        public int Annee { get; set; }

        [Required(ErrorMessage = "L'immatriculation est obligatoire")]
        [StringLength(20)]
        [Display(Name = "Immatriculation")]
        public string Immatriculation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prix par jour est obligatoire")]
        [Range(0.01, 10000)]
        [Display(Name = "Prix / Jour (DH)")]
        public decimal PrixParJour { get; set; }

        [Display(Name = "Disponible")]
        public bool EstDisponible { get; set; } = true;

        [Display(Name = "Catégorie")]
        public string Categorie { get; set; } = "Économique";

        // Images stockées comme chemins séparés par |
        [Display(Name = "Images")]
        public string? ImagesJson { get; set; }

        [NotMapped]
        public List<string> Images
        {
            get => string.IsNullOrEmpty(ImagesJson) ? new List<string>() : ImagesJson.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
            set => ImagesJson = value != null ? string.Join('|', value) : null;
        }

        [NotMapped]
        public string? ImagePrincipale => Images.FirstOrDefault();

        public ICollection<Location> Locations { get; set; } = new List<Location>();
    }
}
