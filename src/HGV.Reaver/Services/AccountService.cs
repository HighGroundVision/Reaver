using HGV.Reaver.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IAccountService
    {
        Task Add(UserLinkEntity user);
        Task Remove(ulong GuildId, ulong UserId);
        Task<UserLinkEntity?> Get(ulong GuildId, ulong UserId);
    }

    public class AccountService : IAccountService
    {
        private readonly ReaverContext context;

        public AccountService(ReaverContext context)
        {
            this.context = context;
        }

        public async Task Add(UserLinkEntity user)
        {
            var existing = await this.context.UserLinks.FirstOrDefaultAsync(i => i.GuidId == user.GuidId && i.UserId == user.UserId);
            if(existing == null)
            {
                await this.context.AddAsync(user);
            }
            else
            {
                existing.SteamId = user.SteamId;
                existing.Email = user.Email;
            }

            await this.context.SaveChangesAsync();
        }

        public async Task Remove(ulong GuildId, ulong UserId)
        {
            var collection = await this.context.UserLinks.Where(i => i.GuidId == GuildId && i.UserId == UserId).ToListAsync();
            this.context.RemoveRange(collection);
            await this.context.SaveChangesAsync();
        }

        public async Task<UserLinkEntity?> Get(ulong GuildId, ulong UserId)
        {
            var entity = await this.context.UserLinks.Where(i => i.GuidId == GuildId && i.UserId == UserId).FirstOrDefaultAsync();
            return entity;
        }

    }
}
