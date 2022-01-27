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
