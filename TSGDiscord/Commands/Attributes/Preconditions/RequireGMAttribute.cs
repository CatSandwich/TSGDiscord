using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace TSGDiscord.Commands.Attributes.Preconditions
{
    public class RequireGMAttribute : RequireRoleAttribute
    {
        public RequireGMAttribute()
        {
            Roles = Config.GuildMasterRoles;
        }
    }
}
