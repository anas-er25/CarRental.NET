using Microsoft.EntityFrameworkCore;
using CarRental.Models;

namespace CarRental.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Voiture> Voitures { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<IncidentVehicule> Incidents { get; set; }
        public DbSet<DemandeLocation> DemandesLocation { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision
            modelBuilder.Entity<Voiture>()
                .Property(v => v.PrixParJour)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Location>()
                .Property(l => l.PrixTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Location>()
                .Property(l => l.MontantDommage)
                .HasPrecision(18, 2);

            modelBuilder.Entity<IncidentVehicule>()
                .Property(i => i.FraisSupplementaires)
                .HasPrecision(18, 2);

            // Avoid cascade delete cycles
            modelBuilder.Entity<Location>()
                .HasOne(l => l.Client).WithMany(c => c.Locations).HasForeignKey(l => l.ClientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DemandeLocation>()
                .HasOne(d => d.Voiture).WithMany().HasForeignKey(d => d.VoitureId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DemandeLocation>()
                .HasOne(d => d.Client).WithMany().HasForeignKey(d => d.ClientId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<DemandeLocation>()
                .HasOne(d => d.Location).WithMany().HasForeignKey(d => d.LocationId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Location).WithMany().HasForeignKey(n => n.LocationId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Client).WithMany().HasForeignKey(n => n.ClientId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<IncidentVehicule>()
                .HasOne(i => i.Location).WithMany().HasForeignKey(i => i.LocationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<IncidentVehicule>()
                .HasOne(i => i.Client).WithMany().HasForeignKey(i => i.ClientId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Voiture>().HasData(
                new Voiture { Id = 1, Marque = "Dacia", Modele = "Logan", Annee = 2022, Immatriculation = "12345-A-1", PrixParJour = 250, EstDisponible = true, Categorie = "Économique" },
                new Voiture { Id = 2, Marque = "Renault", Modele = "Clio", Annee = 2023, Immatriculation = "67890-B-2", PrixParJour = 300, EstDisponible = true, Categorie = "Économique" },
                new Voiture { Id = 3, Marque = "Volkswagen", Modele = "Golf", Annee = 2022, Immatriculation = "11111-C-3", PrixParJour = 450, EstDisponible = true, Categorie = "Berline" },
                new Voiture { Id = 4, Marque = "Toyota", Modele = "RAV4", Annee = 2023, Immatriculation = "22222-D-4", PrixParJour = 600, EstDisponible = true, Categorie = "SUV" },
                new Voiture { Id = 5, Marque = "Mercedes", Modele = "Classe C", Annee = 2023, Immatriculation = "33333-E-5", PrixParJour = 900, EstDisponible = true, Categorie = "Luxe" }
            );

            // Compte admin par défaut
            // Mot de passe: CarRental2026!
            //string hash = BCrypt.Net.BCrypt.HashPassword("CarRental2026!");
            //Console.WriteLine(hash);
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "$2a$11$ZMAwen7yxbtfte8xg4zZfOTAID6SxkFYiyXxkM7LLJ51ATeN3bl2e",
                    NomComplet = "Administrateur Système",
                    Email = "admin@carrental.com",
                    CompteActif = true,
                    DateCreation = new DateTime(2026, 1, 1)
                }
            );
        }
    }
}
