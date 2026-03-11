using CarRental.Data;
using CarRental.Models;

namespace CarRental.Services
{
    public interface INotificationService
    {
        Task CreerAsync(string type, string titre, string message, string? lien = null, int? locationId = null, int? clientId = null);
        Task<int> CompterNonLuesAsync();
        Task MarquerLuAsync(int id);
        Task MarquerToutesLuesAsync();
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _ctx;
        public NotificationService(ApplicationDbContext ctx) { _ctx = ctx; }

        public async Task CreerAsync(string type, string titre, string message, string? lien = null, int? locationId = null, int? clientId = null)
        {
            _ctx.Notifications.Add(new Notification
            {
                Type = type,
                Titre = titre,
                Message = message,
                LienAction = lien,
                LocationId = locationId,
                ClientId = clientId,
                EstLu = false,
                DateCreation = DateTime.Now
            });
            await _ctx.SaveChangesAsync();
        }

        public async Task<int> CompterNonLuesAsync()
            => await Task.FromResult(_ctx.Notifications.Count(n => !n.EstLu));

        public async Task MarquerLuAsync(int id)
        {
            var n = await _ctx.Notifications.FindAsync(id);
            if (n != null) { n.EstLu = true; await _ctx.SaveChangesAsync(); }
        }

        public async Task MarquerToutesLuesAsync()
        {
            var unread = _ctx.Notifications.Where(n => !n.EstLu).ToList();
            unread.ForEach(n => n.EstLu = true);
            await _ctx.SaveChangesAsync();
        }
    }
}
