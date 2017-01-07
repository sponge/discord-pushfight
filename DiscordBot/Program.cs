using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;

class GameSession
{
    public PushFight.PushFightGame Game;
    public Discord.IChannel Channel;
    public Dictionary<PushFight.Team, IUser> Players;

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

class Program
{

    static void Main(string[] args)
    {
        new Program().RunAsync(args).GetAwaiter().GetResult();
    }

    private DiscordSocketClient client;
    private ImageRenderer.ImageRenderer imgr;
    private Dictionary<ulong, GameSession> sessions;

    public async Task RunAsync(string[] args)
    {
        client = new DiscordSocketClient();
        var game = new PushFight.PushFightGame();
        imgr = new ImageRenderer.ImageRenderer();
        sessions = new Dictionary<ulong, GameSession>();

        if (args.Length == 0)
        {
            Console.WriteLine("Specify Discord token on commandline");
            return;
        }

        var token = args[0];

        client.MessageReceived += async (message) =>
        {
            if (message.Content.StartsWith("."))
            {
                var arg = message.Content.Substring(1).Split(' ');

                if (arg[0].ToLower() == "challenge")
                {
                    var challenger = message.Author;
                    var guild = (message.Channel as SocketGuildChannel).Guild;
                    var challenged = message.MentionedUsers.First();
                    var channelName = "pf-" + challenger.Username + "-v-" + challenged.Username;
                    var newChannel = await guild.CreateTextChannelAsync(channelName);
                    var sess = new GameSession(newChannel, challenger, challenged);
                    sessions.Add(newChannel.Id, sess);
                    await message.Channel.SendMessageAsync("Channel created! Head on into #" + newChannel.Name +" to get started!");
                }
                else
                {
                    if (!sessions.ContainsKey(message.Channel.Id)) {
                        return;
                    }

                    var sess = sessions[message.Channel.Id];
                    // do this in a roundabout way so you can test by playing yourself
                    var checkUser = sess.Players[sess.Game.CurrentTeam];

                    if (message.Author != checkUser)
                    {
                        await message.Channel.SendMessageAsync("it's not your turn dummy");
                        return;
                    }

                    var ecode = sess.Game.Input(String.Join(" ", arg), sess.Game.CurrentTeam);
                    var img = imgr.Render(sess.Game);
                    await message.Channel.SendFileAsync(img, "board.png", ecode.ToString());
                }
                
            }
        };

        await client.LoginAsync(TokenType.Bot, token);
        await client.ConnectAsync();
        await Task.Delay(-1);
    }
}