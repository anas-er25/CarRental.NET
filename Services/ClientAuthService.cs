using CarRental.Data;
using CarRental.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace CarRental.Services
{
    public interface IClientAuthService
    {
        Task<Client?> AuthenticateAsync(string email, string password);
        Task<bool> RegisterAsync(Client client, string password);
        string HashPassword(string password);
        bool VerifyPassword(string hash, string password);
    }

    public class ClientAuthService : IClientAuthService
    {
        private readonly ApplicationDbContext _ctx;
        public ClientAuthService(ApplicationDbContext ctx) { _ctx = ctx; }

        public async Task<Client?> AuthenticateAsync(string email, string password)
        {
            var client = await _ctx.Clients.FirstOrDefaultAsync(c => c.Email == email && c.CompteActif);
            if (client == null || string.IsNullOrEmpty(client.PasswordHash)) return null;
            return VerifyPassword(client.PasswordHash, password) ? client : null;
        }

        public async Task<bool> RegisterAsync(Client client, string password)
        {
            if (await _ctx.Clients.AnyAsync(c => c.Email == client.Email || c.CIN == client.CIN))
                return false;
            client.PasswordHash = HashPassword(password);
            client.DateInscription = DateTime.Now;
            client.CompteActif = true;
            _ctx.Clients.Add(client);
            await _ctx.SaveChangesAsync();
            return true;
        }

        public string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password);

        public bool VerifyPassword(string hash, string password)
            => BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
