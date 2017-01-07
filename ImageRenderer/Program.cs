using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        var game = new PushFight.PushFightGame();
        var imgr = new ImageRenderer.ImageRenderer();

        var cmds = new List<string>
                    {
                        "place round a4",
                        "place round a5",
                        "place square b4",
                        "place square b5",
                        "place round c4",
                        "place round c5",
                        "place square d4",
                        "place square d5",
                        "place square c3",
                        "place square c6",
                        "m d4",
                        "p d4 d",
                        "p b5 u",
                        "p d5 d",
                    };

        int i = 0;
        foreach (var autocmd in cmds)
        {
            game.Input(autocmd, game.CurrentTeam);
            using (FileStream outfile = File.OpenWrite("zzout_" + i + ".png"))
            {
                var img = imgr.Render(game);
            }
            i++;
        }
    }
}