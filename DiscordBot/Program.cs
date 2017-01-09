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
        status += "\n\n";

        // 2nd line: appropriate metadata pertaining to phase (pieces left to place, moves left)
        if (sess.Game.Phase == GamePhase.Placement)
        {
            var round = sess.Game.RemainingPieces.Where(r => r.Team == sess.Game.CurrentTeam && r.PawnType == PawnType.Round).First().Count;
            var square = sess.Game.RemainingPieces.Where(r => r.Team == sess.Game.CurrentTeam && r.PawnType == PawnType.Square).First().Count;
            status += String.Format("Pieces Remaining: ⚪: {0}, ⬜: {1}\n", round, square);
        }
        else if (sess.Game.Phase == GamePhase.Push)
        {
            status += String.Format("{0} move{1} remaining.\n", sess.Game.RemainingMoves, sess.Game.RemainingMoves > 1 ? "s" : "");
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
            case GamePhase.Placement:
                output = @"Place your pawns on your half of the board. White starts on the left, Black on the right.

.__p__lace (__r__ound|__s__quare) cell
";
                break;

            case GamePhase.Push:
                output = @"Move up to two pawns to any connected cell, and then push a square piece.
.__m__ove start-cell - show all valid moves for the given cell.
.__m__ove start-cell end-cell
.__p__ush cell (__u__p|__d__own|__l__eft|__r__ight)";
                break;

            case GamePhase.Complete:
                output = "Type .rematch to play again, .swap to swap teams and play again, or .end to end this session and remove the channel.";
                break;

            default:
                output = "No help for phase "+ sess.Game.Phase.ToString();
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

                // only challenge can be issued from all text channels
                if (arg[0] == "challenge")
                {
                    var challenger = message.Author;
                    var guild = (message.Channel as SocketGuildChannel).Guild;

                    if (!message.MentionedUsers.Any())
                    {
                        await message.Channel.SendMessageAsync("You need to mention someone in order to challenge them.");
                        return;
                    }

                    var challenged = message.MentionedUsers.First();

                    var channelName = "pf-" + challenger.Username + "-v-" + challenged.Username;
                    var newChannel = await guild.CreateTextChannelAsync(channelName);

                    var sess = new GameSession(challenger, challenged);
                    sessions.Add(newChannel.Id, sess);

                    await message.Channel.SendMessageAsync("Channel created! Head on into " + newChannel.Mention +" to get started!");
                    await SendGameStatusAsync(newChannel, sess);
                } else if (sessions.ContainsKey(message.Channel.Id))
                {
                    // these commands are only available from game channels
                    var sess = sessions[message.Channel.Id];

                    switch (arg[0])
                    {
                        case "end":
                        case "quit":
                            await message.Channel.SendMessageAsync("Ending game, and removing channel in 10 seconds. Thanks for playing!");
                            sessions.Remove(message.Channel.Id);
                            await Task.Delay(10000);
                            await (message.Channel as SocketGuildChannel).DeleteAsync();
                            break;

                        case "rematch":
                        case "restart":
                            sess.Restart();
                            await message.Channel.SendMessageAsync("Restarting match.");
                            await SendGameStatusAsync(message.Channel, sess);
                            break;

                        case "swap":
                            sess.SwapTeams();
                            sess.Restart();
                            await message.Channel.SendMessageAsync("Swapping teams and restarting match.");
                            await SendGameStatusAsync(message.Channel, sess);
                            break;

                        case "help":
                            await SendGameHelpAsync(message.Channel, sess);
                            break;

                        default:
                            // it's not a special command, just pass it on to the pushfight instance
                            // map teams to players instead of players to teams so you can challenge and play yourself
                            var checkUser = sess.Players[sess.Game.CurrentTeam];

                            if (message.Author != checkUser)
                            {
                                await message.Channel.SendMessageAsync("it's not your turn dummy");
                                break;
                            }

                            sess.LastStatus = sess.Game.Input(String.Join(" ", arg), sess.Game.CurrentTeam);

                            if (sess.LastStatus != ECode.Success)
                            {
                                await message.Channel.SendMessageAsync("**Error: " + PushFightGame.GetError(sess.LastStatus) + "**");
                                break;
                            }

                            await SendGameStatusAsync(message.Channel, sess);
                            break;
                    }
                }
                
            }
        };

        await client.LoginAsync(TokenType.Bot, token);
        await client.ConnectAsync();
        await client.SetGameAsync(".challenge to play!");
        Console.WriteLine("Connected and active!");
        await Task.Delay(-1);
    }
}