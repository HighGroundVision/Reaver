using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGV.Reaver.Bot.Checks
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ContextMenuRequirePermissionsAttribute : ContextMenuCheckBaseAttribute
    {
        /// <summary>
        /// Gets the permissions required by this attribute.
        /// </summary>
        public Permissions Permissions { get; }


        /// <summary>
        /// Defines that usage of this command is restricted to members with specified permissions. This check also verifies that the bot has the same permissions.
        /// </summary>
        /// <param name="permissions">Permissions required to execute this command.</param>
        /// <param name="ignoreDms">Sets this check's behaviour in DMs. True means the check will always pass in DMs, whereas false means that it will always fail.</param>
        public ContextMenuRequirePermissionsAttribute(Permissions permissions)
        {
            this.Permissions = permissions;
        }

        public override Task<bool> ExecuteChecksAsync(ContextMenuContext ctx)
        {
            var member = ctx.Member;
            if (member == null)
                return Task.FromResult(false);

            var permissions = ctx.Channel.PermissionsFor(member);
            if(ctx.Guild.OwnerId == member.Id)
                return Task.FromResult(true);
            else if((permissions & Permissions.Administrator) != 0)
                return Task.FromResult(true);
            else if ((permissions & this.Permissions) == this.Permissions)
                return Task.FromResult(true);
            else 
                return Task.FromResult(false);
        }
    }
}
