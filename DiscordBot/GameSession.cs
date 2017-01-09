using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using PushFight;

namespace DiscordBot
{
    class GameSession
    {
        public PushFightGame Game;
        public Dictionary<Team, IUser> Players;
        public ECode LastStatus;
        public GamePhase LastPhase;

        public GameSession(IUser whitePlayer, IUser blackPlayer)
        {
            Game = new PushFightGame();
            Players = new Dictionary<Team, IUser>()
            {
                { Team.White, whitePlayer },
                { Team.Black, blackPlayer },
            };
        }

        public void SwapTeams()
        {
            var newPlayers = new Dictionary<Team, IUser>()
            {
                {Team.Black, Players[Team.White]},
                {Team.White, Players[Team.Black]},
            };
            Players = newPlayers;
        }

        public void Restart()
        {
            Game = new PushFightGame();
            LastStatus = ECode.Success;
            LastPhase = GamePhase.Invalid;
        }
    }
}
