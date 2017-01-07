using System;
using System.Collections.Generic;
using System.Text;
using ImageSharp;
using PushFight;
using System.IO;
using ImageSharp.Drawing.Shapes;

namespace ImageRenderer
{
    public class ImageRenderer
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

        public MemoryStream Render(PushFightGame game)
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
                    if (cell.Contents.Team == Team.Black)
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

            var stream = new MemoryStream();
            output.SaveAsPng(stream);
            return stream;
        }
    }

}
