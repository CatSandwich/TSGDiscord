using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using TSGDiscord.Commands.Attributes.Preconditions;

namespace TSGDiscord.Commands.Attributes
{
    internal class RequireOfficerAttribute : RequireRoleAttribute
    {
        public RequireOfficerAttribute()
        {
            Roles = Config.OfficerRoles;
        }
    }
}
