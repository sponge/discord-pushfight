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
                arg[0] = arg[0].ToLower();

                if (arg[0] == "challenge")
                {
                    var challenger = message.Author;
                    var guild = (message.Channel as SocketGuildChannel).Guild;
                    var challenged = message.MentionedUsers.First();
                    var channelName = "pf-" + challenger.Username + "-v-" + challenged.Username;
                    var newChannel = await guild.CreateTextChannelAsync(channelName);
                    var sess = new GameSession(newChannel, challenger, challenged);

                    sessions.Add(newChannel.Id, sess);
                    await message.Channel.SendMessageAsync("Channel created! Head on into " + newChannel.Mention +" to get started!");

                    var img = imgr.Render(sess.Game);
                    // TODO print status
                    await newChannel.SendFileAsync(img, "board.png", "");
                    // TODO print help
                }
                else if (arg[0] == "end")
                {
                    // TODO destroy the data, await task.delay 15 seconds, delete the channel
                }
                else if (arg[0] == "reset")
                {
                    // TODO reset the game state
                }
                else if (arg[0] == "help")
                {
                    // TODO print help
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
        Console.WriteLine("Connected and active!");
        await Task.Delay(-1);
    }
}