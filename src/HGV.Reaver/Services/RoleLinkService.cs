
using HGV.Reaver.Data;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IRoleLinkService
    {
        Task Add(RoleLinkEntity item);
        Task<RoleLinkEntity?> Get(ulong guidId, ulong messageId, string emojiName);
        Task Remove(ulong guidId, ulong messageId);
        Task Remove(ulong guidId, ulong messageId, string emojiName);
        Task Purge(ulong guidId);
    }

    public class RoleLinkService : IRoleLinkService
    {

        private readonly ReaverContext context;

        public RoleLinkService(ReaverContext context)
        {
            this.context = context;
        }

        public async Task Add(RoleLinkEntity item)
        {
            await this.context.RoleLinks.AddAsync(item);
            await this.context.SaveChangesAsync();
        }

        public async Task Purge(ulong guidId)
        {
            var collection = await this.context.RoleLinks.Where(i => i.GuidId == guidId).ToListAsync();
            this.context.RemoveRange(collection);
            await this.context.SaveChangesAsync();
        }

        public async Task Remove(ulong guidId, ulong messageId)
        {
            var collection = await this.context.RoleLinks.Where(i => i.GuidId == guidId && i.MessageId == messageId).ToListAsync();
            this.context.RemoveRange(collection);
            await this.context.SaveChangesAsync();
        }

        public async Task Remove(ulong guidId, ulong messageId, string emojiName)
        {
            var collection = await this.context.RoleLinks.Where(i => i.GuidId == guidId && i.MessageId == messageId && i.EmojiName == emojiName).ToListAsync();
            this.context.RemoveRange(collection);
            await this.context.SaveChangesAsync();
        }

        public async Task<RoleLinkEntity?> Get(ulong guidId, ulong messageId, string emojiName)
        {
            var entity = await this.context.RoleLinks.Where(i => i.GuidId == guidId && i.MessageId == messageId && i.EmojiName == emojiName).FirstOrDefaultAsync();
            return entity;
        }

        public async Task<IEnumerable<RoleLinkEntity>> Get(ulong guidId, ulong messageId)
        {
            var collection = await this.context.RoleLinks.Where(i => i.GuidId == guidId && i.MessageId == messageId).ToListAsync();
            return collection;
        }
    }
}
