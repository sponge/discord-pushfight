using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot
{
    class GameSession
    {
        public PushFight.PushFightGame Game;
        public Dictionary<PushFight.Team, IUser> Players;
        public PushFight.ECode LastStatus;
        public PushFight.GamePhase LastPhase;

        public GameSession(IUser whitePlayer, IUser blackPlayer)
        {
            Game = new PushFight.PushFightGame();
            Players = new Dictionary<PushFight.Team, IUser>()
        {
            { PushFight.Team.White, whitePlayer },
            { PushFight.Team.Black, blackPlayer },
        };
        }
    }
}
