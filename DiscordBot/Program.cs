using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DiscordBot;
using PushFight;

class Program
{

    static void Main(string[] args)
    {
        new Program().RunAsync(args).GetAwaiter().GetResult();
    }

    private DiscordSocketClient client;
    private ImageRenderer.ImageRenderer imgr;
    private Dictionary<ulong, GameSession> sessions;

    public async Task SendGameStatusAsync(IMessageChannel channel, GameSession sess)
    {      
        var img = imgr.Render(sess.Game);
        var player = sess.Players[sess.Game.CurrentTeam];

        var status = (sess.LastStatus != ECode.Success ? "**"+ sess.LastStatus.ToString() +"**\n" : "");
        if (sess.Game.Phase != GamePhase.Complete) {
            status += player.Mention + "'s turn to ";
            status += sess.Game.Phase == GamePhase.Placement ? "place a pawn." : "push.";
        } else
        {
            status += "Game Over! " + sess.Players[sess.Game.Winner].Mention + " wins!";
        }
        status += "\n";

        if (sess.Game.Phase == GamePhase.Placement)
        {
            var round = sess.Game.RemainingPieces.Where(r => r.Team == sess.Game.CurrentTeam && r.PawnType == PawnType.Round).First().Count;
            var square = sess.Game.RemainingPieces.Where(r => r.Team == sess.Game.CurrentTeam && r.PawnType == PawnType.Square).First().Count;
            status += String.Format("Pieces Remaining: ⚪: {0}, ⬜: {1}\n", round, square);
        }
        else if (sess.Game.Phase == GamePhase.Push)
        {
            status += String.Format("{0} moves remaining.\n", sess.Game.RemainingMoves);
        }

        await channel.SendFileAsync(img, "board.png", status);

        if (sess.Game.Phase == GamePhase.Complete)
        {
            await channel.SendMessageAsync("Type .rematch to play again.");
        }
    }

    public async Task SendGameHelpAsync(IMessageChannel channel, GameSession sess)
    {
        await channel.SendMessageAsync("TODO: help text goes here");
    }

    public async Task RunAsync(string[] args)
    {
        client = new DiscordSocketClient();
        var game = new PushFightGame();
        imgr = new ImageRenderer.ImageRenderer();
        sessions = new Dictionary<ulong, GameSession>();

        if (args.Length == 0)
        {
            Console.WriteLine("Specify Discord token on commandline");
            return;
        }

        var token = args[0];

        client.ChannelDestroyed += async (channel) =>
        {
            if (!sessions.ContainsKey(channel.Id))
            {
                sessions.Remove(channel.Id);
                await Task.Delay(1); // FIXME: vs complains about no awaits but there's no need for one
            }
        };

        client.MessageReceived += async (message) =>
        {
            if (message.Content.StartsWith("."))
            {
                var arg = message.Content.Substring(1).Split(' ');
                arg[0] = arg[0].ToLower();

                if (arg[0] == "challenge")
                {
                    var challenger = message.Author;
                    var guild = (message.Channel as SocketGuildChannel).Guild;
                    var challenged = message.MentionedUsers.First();
                    var channelName = "pf-" + challenger.Username + "-v-" + challenged.Username;
                    var newChannel = await guild.CreateTextChannelAsync(channelName);

                    var sess = new GameSession(challenger, challenged);
                    sessions.Add(newChannel.Id, sess);

                    await message.Channel.SendMessageAsync("Channel created! Head on into " + newChannel.Mention +" to get started!");

                    await SendGameStatusAsync(newChannel, sess);
                    await SendGameHelpAsync(newChannel, sess);
                } else if (sessions.ContainsKey(message.Channel.Id))
                {
                    var sess = sessions[message.Channel.Id];

                    if (arg[0] == "end")
                    {
                        // TODO destroy the data, await task.delay 15 seconds, delete the channel
                    }
                    else if (arg[0] == "rematch")
                    {
                        sess.Game = new PushFightGame();
                        sess.LastStatus = ECode.Success;
                        await message.Channel.SendMessageAsync("Restarting match.");
                        await SendGameStatusAsync(message.Channel, sess);
                    }
                    else if (arg[0] == "help")
                    {

                        await SendGameHelpAsync(message.Channel, sess);
                    }
                    else
                    {
                        // map teams to players instead of players to teams so you can challenge and play yourself
                        var checkUser = sess.Players[sess.Game.CurrentTeam];

                        if (message.Author != checkUser)
                        {
                            await message.Channel.SendMessageAsync("it's not your turn dummy");
                            return;
                        }

                        sess.LastStatus = sess.Game.Input(String.Join(" ", arg), sess.Game.CurrentTeam);

                        await SendGameStatusAsync(message.Channel, sess);
                    }
                }
                
            }
        };

        await client.LoginAsync(TokenType.Bot, token);
        await client.ConnectAsync();
        Console.WriteLine("Connected and active!");
        await Task.Delay(-1);
    }
}