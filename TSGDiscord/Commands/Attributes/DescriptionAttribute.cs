using System;
using System.Collections.Generic;
using System.Text;

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
