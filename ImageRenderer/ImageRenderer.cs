using System;
using System.Collections.Generic;
using System.Text;
using ImageSharp;
using PushFight;
using System.IO;
using ImageSharp.Drawing.Shapes;
using System.Numerics;

namespace ImageRenderer
{
    public class ImageRenderer
    {
        Image Board = new Image(File.ReadAllBytes("img/board.png"));
        Image BlackRound = new Image(File.ReadAllBytes("img/br.png"));
        Image BlackSquare = new Image(File.ReadAllBytes("img/bs.png"));
        Image WhiteRound = new Image(File.ReadAllBytes("img/wr.png"));
        Image WhiteSquare = new Image(File.ReadAllBytes("img/ws.png"));
        Image Anchor = new Image(File.ReadAllBytes("img/anchor.png"));
        Image Arrow = new Image(File.ReadAllBytes("img/arrow.png"));


        public MemoryStream Render(PushFightGame game)
        {
            var output = new Image(Board.Width, Board.Height);

            output.DrawImage(Board, 100, new Size(Board.Width, Board.Height), new Point(0, 0));

            foreach (var ev in game.LastTurnEvents)
            {
                var pos1 = new Point((int)(88.5 * ev.x1), (int)(91 * ev.y1) - 23);
                var pos2 = new Point((int)(88.5 * ev.x2), (int)(91 * ev.y2) - 23);

                switch (ev.Type)
                {
                    case TurnEventType.Place:
                        output.Fill(new Color(255, 255, 0, 90), new RectangularPolygon(new Rectangle(pos1.X, pos1.Y, 88, 91)));
                        break;

                    case TurnEventType.Move:
                        output.DrawLines(new ImageSharp.Drawing.Pens.Pen(Color.Black, 2.0f), new[] {
                            new Vector2(pos1.X + WhiteSquare.Width/2, pos1.Y + WhiteSquare.Height/2),
                            new Vector2(pos2.X + WhiteSquare.Width/2, pos2.Y + WhiteSquare.Height/2)
                        });
                        break;

                    case TurnEventType.Push:
                        var arr = new Image(Arrow);
                        arr.Rotate(ev.Direction == Direction.Right ? 0 : ev.Direction == Direction.Down ? 90 : ev.Direction == Direction.Left ? 180 : 270);
                        output.DrawImage(arr, 100, new Size(arr.Width, arr.Height), pos1);
                        break;
                }
            }

            for (int x = 0; x < game.Board.GetLength(0); x++)
            {
                for (int y = 0; y < game.Board.GetLength(1); y++)
                {
                    var cell = game.Board[x, y];

                    var pos = new Point((int)(88.5 * x), (int)(91 * y) - 23);

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
