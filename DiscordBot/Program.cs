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

        // prefix message: error if exists, who's turn, and what they need to do
        var status = (sess.LastStatus != ECode.Success ? "**"+ sess.LastStatus.ToString() +"**\n" : "");
        if (sess.Game.Phase != GamePhase.Complete) {
            status += player.Mention + "'s turn to ";
            status += sess.Game.Phase == GamePhase.Placement ? "place a pawn." : "push.";
        } else
        {
            status += "Game Over! " + sess.Players[sess.Game.Winner].Mention + " wins!";
        }
        status += "\n";

        // 2nd line: appropriate metadata pertaining to phase (pieces left to place, moves left)
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

        // send the board image, with the prefix message
        await channel.SendFileAsync(img, "board.png", status);

        // 2nd message for anything that should go after board
        if (sess.Game.Phase != sess.LastPhase)
        {
            // print out help if the phase changed
            await SendGameHelpAsync(channel, sess);
            sess.LastPhase = sess.Game.Phase;
        }
    }

    public async Task SendGameHelpAsync(IMessageChannel channel, GameSession sess)
    {
        string output;
        switch (sess.Game.Phase)
        {
            case GamePhase.Complete:
                output = "Type .rematch to play again, or .end to end this session and remove the channel.";
                break;

            default:
                output = "TODO: help text goes here";
                break;
        }

        await channel.SendMessageAsync(output);     
    }

    public async Task RunAsync(string[] args)
    {
        client = new DiscordSocketClient();
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
            sessions.Remove(channel.Id);
            await Task.Delay(1); // FIXME: vs complains about no awaits but there's no need for one
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
                } else if (sessions.ContainsKey(message.Channel.Id))
                {
                    var sess = sessions[message.Channel.Id];

                    if (arg[0] == "end")
                    {
                        await message.Channel.SendMessageAsync("Ending game, and removing channel in 10 seconds. Thanks for playing!");
                        sessions.Remove(message.Channel.Id);
                        await Task.Delay(10000);
                        await (message.Channel as SocketGuildChannel).DeleteAsync();
                    }
                    else if (arg[0] == "rematch")
                    {
                        sess.Game = new PushFightGame();
                        sess.LastStatus = ECode.Success;
                        sess.LastPhase = GamePhase.Invalid;
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