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
                        "place round d1",
                        "place round e1",
                        "place square d2",
                        "place square e2",
                        "place round d3",
                        "place round e3",
                        "place square d4",
                        "place square e4",
                        "place square c1",
                        "place square f4",
                        "move c1",
                        "move c1 b4",
                        "move d2 c2",
                        "push c2 right",
                        "push e4 left",
                        "push d4 up",
                        "push e2 down",
                    };

        int i = 0;
        foreach (var autocmd in cmds)
        {
            game.Input(autocmd, game.CurrentTeam);
            using (FileStream outfile = File.OpenWrite("zzout_" + i + ".png"))
            {
                var img = imgr.Render(game);
                img.CopyTo(outfile);
            }
            i++;
        }
    }
}