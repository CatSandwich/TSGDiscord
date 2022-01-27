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
