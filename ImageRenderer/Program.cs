﻿using System;
using System.Collections.Generic;
using ImageSharp;
using PushFight;
using System.IO;
using ImageSharp.Drawing.Shapes;

class ImageRenderer
{
    Image Board;
    Image BlackRound;
    Image BlackSquare;
    Image WhiteRound;
    Image WhiteSquare;
    Image Anchor;

    public ImageRenderer()
    {
        using (var infile = File.OpenRead("img/board.png"))
        {
            Board = new Image(infile);
        }

        using (var infile = File.OpenRead("img/br.png"))
        {
            BlackRound = new Image(infile);
        }

        using (var infile = File.OpenRead("img/bs.png"))
        {
            BlackSquare = new Image(infile);
        }

        using (var infile = File.OpenRead("img/wr.png"))
        {
            WhiteRound = new Image(infile);
        }

        using (var infile = File.OpenRead("img/ws.png"))
        {
            WhiteSquare = new Image(infile);
        }

        using (var infile = File.OpenRead("img/anchor.png"))
        {
            Anchor = new Image(infile);
        }
    }

    public void Render(PushFightGame game, Stream outStream)
    {
        var output = new Image(Board.Width, Board.Height);

        output.DrawImage(Board, 100, new Size(Board.Width, Board.Height), new Point(0, 0));

        for (int x = 0; x < game.Board.GetLength(0); x++)
        {
            for (int y = 0; y < game.Board.GetLength(1); y++)
            {
                var cell = game.Board[x, y];

                var pos = new Point((int)(88.5 * y), (int)(91 * x) - 23);

                if (cell.Highlight)
                {
                    output.Fill(new Color(255, 255, 0, 90), new RectangularPolygon(new Rectangle(pos.X, pos.Y, 88, 91)));
                }

                if (cell.Contents.Type == PawnType.Empty)
                {
                    continue;
                }

                Image img;
                if ( cell.Contents.Team == Team.Black)
                {
                    img = cell.Contents.Type == PawnType.Round ? BlackRound : BlackSquare;
                }
                else
                {
                    img = cell.Contents.Type == PawnType.Round ? WhiteRound : WhiteSquare;
                }

                output.DrawImage(img, 100, new Size(img.Width, img.Height), new Point(pos.X + 4, pos.Y));

                if (cell.Anchored)
                {
                    output.DrawImage(Anchor, 100, new Size(Anchor.Width, Anchor.Height), new Point(pos.X + 17, pos.Y + 13));
                }
            }
        }

        output.Save(outStream);
    }
}

class Program
{
    static void Main(string[] args)
    {
        var game = new PushFight.PushFightGame();
        var imgr = new ImageRenderer();

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
                imgr.Render(game, outfile);
            }
            i++;
        }
    }
}