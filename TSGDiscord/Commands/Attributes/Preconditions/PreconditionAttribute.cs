using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace TSGDiscord.Commands.Attributes
{
    public abstract class PreconditionAttribute : Attribute
    {
        public abstract Task Check(Bot bot, SocketMessage sm);
    }
}
