using CarRental.Models;
using CarRental.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Services
{
    public interface ILocationService
    {
        decimal CalculerPrixTotal(decimal prixParJour, DateTime dateDebut, DateTime dateFin);
        bool VoitureDisponible(int voitureId, DateTime dateDebut, DateTime dateFin, int? excludeLocationId = null);
        Task<List<Location>> GetLocationsEnCoursAsync();
    }

    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;

        public LocationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public decimal CalculerPrixTotal(decimal prixParJour, DateTime dateDebut, DateTime dateFin)
        {
            int jours = (dateFin - dateDebut).Days;
            if (jours <= 0) return 0;
            return prixParJour * jours;
        }

        public bool VoitureDisponible(int voitureId, DateTime dateDebut, DateTime dateFin, int? excludeLocationId = null)
        {
            var query = _context.Locations
                .Where(l => l.VoitureId == voitureId
                         && l.Statut == "EnCours"
                         && l.DateDebut < dateFin
                         && l.DateFin > dateDebut);

            if (excludeLocationId.HasValue)
                query = query.Where(l => l.Id != excludeLocationId.Value);

            return !query.Any();
        }

        public async Task<List<Location>> GetLocationsEnCoursAsync()
        {
            return await _context.Locations
                .Include(l => l.Voiture)
                .Include(l => l.Client)
                .Where(l => l.Statut == "EnCours")
                .OrderByDescending(l => l.DateCreation)
                .ToListAsync();
        }
    }
}
