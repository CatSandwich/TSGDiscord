using System;

namespace TSGDiscord.Commands.Attributes
{
    public class DescriptionAttribute : Attribute
    {
        public string Description;

        public DescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
