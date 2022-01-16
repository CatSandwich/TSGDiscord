using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace TSGDiscord.Commands.Attributes.Preconditions
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequireRoleAttribute : PreconditionAttribute
    {
        public ulong[] Roles;

        public RequireRoleAttribute(params ulong[] roles)
        {
            Roles = roles;
        }

        public override async Task Check(Bot bot, SocketMessage sm)
        {
            if (!(sm.Author is SocketGuildUser socketGuildUser)) throw new Commands.PreconditionFailedException("Failed to find user's roles.");
            if (socketGuildUser.Roles.All(role => !Roles.Contains(role.Id))) throw new Commands.PreconditionFailedException("Insufficient permissions.");
        }
    }
}
