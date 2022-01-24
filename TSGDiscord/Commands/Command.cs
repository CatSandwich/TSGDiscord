using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using TSGDiscord.Commands.Attributes;

namespace TSGDiscord.Commands
{
    public class Command
    {
        public string[] Names;
        public string? Description;
        public bool Hidden;
        public PreconditionAttribute[] Preconditions;
        public Func<Bot, SocketMessage, Task> Handler;

        public Command(MethodInfo method)
        {
            Names = method.GetCustomAttribute<CommandAttribute>()!.Names;
            Description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;
            Hidden = method.IsDefined(typeof(HideFromHelp));
            Preconditions = method.GetCustomAttributes<PreconditionAttribute>().ToArray();
            Handler = (Func<Bot, SocketMessage, Task>)method.CreateDelegate(typeof(Func<Bot, SocketMessage, Task>));
        }

        public override string ToString() => Names[0];
    }
}
