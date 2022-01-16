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
        public PreconditionAttribute[] Preconditions;
        public Func<Bot, SocketMessage, Task> Handler;

        public Command(MethodInfo method)
        {
            Names = method.GetCustomAttribute<CommandAttribute>()!.Names;
            Description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;
            Preconditions = method.GetCustomAttributes<PreconditionAttribute>().ToArray();
            Handler = (Func<Bot, SocketMessage, Task>)method.CreateDelegate(typeof(Func<Bot, SocketMessage, Task>));
        }

        public override string ToString() => Names;
    }
}
