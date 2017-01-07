using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot
{
    class GameSession
    {
        public PushFight.PushFightGame Game;
        public Discord.IChannel Channel;
        public Dictionary<PushFight.Team, IUser> Players;
        public PushFight.ECode lastStatus;

        public GameSession(IChannel channel, IUser whitePlayer, IUser blackPlayer)
        {
            Game = new PushFight.PushFightGame();
            Channel = channel;
            Players = new Dictionary<PushFight.Team, IUser>()
        {
            { PushFight.Team.White, whitePlayer },
            { PushFight.Team.Black, blackPlayer },
        };
        }
    }
}
