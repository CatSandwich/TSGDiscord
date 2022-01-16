using System;

namespace TSGDiscord.Commands.Attributes
{
    internal class CommandAttribute : Attribute
    {
        public string[] Names;

        public CommandAttribute(params string[] names)
        {
            Names = names;
        }
    }
}
