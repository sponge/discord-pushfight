using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DiscordBot;

class Program
{

    static void Main(string[] args)
    {
        new Program().RunAsync(args).GetAwaiter().GetResult();
    }

    private DiscordSocketClient client;
    private ImageRenderer.ImageRenderer imgr;
    private Dictionary<ulong, GameSession> sessions;

    public async Task SendGameStatus(IMessageChannel channel, GameSession sess)
    {      
        var img = imgr.Render(sess.Game);
        var status = (sess.LastStatus != PushFight.ECode.Success ? sess.LastStatus.ToString() : "");
        // TODO print player's turn, hilight them
        status += sess.Game.Phase.ToString();
        // TODO print a better status
        await channel.SendFileAsync(img, "board.png", status);
    }

    public async Task SendGameHelp(IMessageChannel channel, GameSession sess)
    {
        await channel.SendMessageAsync("TODO: help text goes here");
    }

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

        client.ChannelDestroyed += async (channel) =>
        {
            // TODO: check if it was one of our channels and destroy the game session
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

                    await SendGameStatus(message.Channel, sess);
                    await SendGameHelp(message.Channel, sess);
                } else if (sessions.ContainsKey(message.Channel.Id))
                {
                    var sess = sessions[message.Channel.Id];

                    if (arg[0] == "end")
                    {
                        // TODO destroy the data, await task.delay 15 seconds, delete the channel
                    }
                    else if (arg[0] == "reset")
                    {
                        // TODO reset the game state
                    }
                    else if (arg[0] == "help")
                    {

                        await SendGameHelp(message.Channel, sess);
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

                        await SendGameStatus(message.Channel, sess);
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