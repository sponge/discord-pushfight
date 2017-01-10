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

        private int cellSize = 90;
        private int cellInnerSize = 84;

        private Point ToPixels(Point cell)
        {
            return new Point((int)(cellSize * (cell.X - 1)) + 76, (int)(cellSize * (cell.Y - 1)) + 82);
        }

        private void DrawHighlight(PushFightGame game, Image output, Point cell)
        {
            var px = ToPixels(cell);
            output.Fill(new Color(255, 255, 0, 90), new RectangularPolygon(new Rectangle(px.X, px.Y, cellInnerSize, cellInnerSize)));
        }

        private void DrawPath(PushFightGame game, Image output, Point startCell, Point endCell) {
            var startpx = ToPixels(startCell);
            var endpx = ToPixels(endCell);
            output.DrawLines(new ImageSharp.Drawing.Pens.Pen(Color.Black, 2.0f), new[] {
                            new Vector2(startpx.X + WhiteSquare.Width/2, startpx.Y + WhiteSquare.Height/2),
                            new Vector2(endpx.X + WhiteSquare.Width/2, endpx.Y + WhiteSquare.Height/2)
                        });
        }

        private void DrawArrow(PushFightGame game, Image output, Point cell, Direction dir)
        {
            var px = ToPixels(cell);
            var arr = new Image(Arrow);
            arr.Rotate(dir == Direction.Right ? 0 : dir == Direction.Down ? 90 : dir == Direction.Left ? 180 : 270);
            output.DrawImage(arr, 100, new Size(arr.Width, arr.Height), px);
        }

        private void DrawPiece(PushFightGame game, Image output, Point cell, Pawn pawn) {
            Image img;
            if (pawn.Team == Team.None)
            {
                return;
            }
            else if (pawn.Team == Team.Black)
            {
                img = pawn.Type == PawnType.Round ? BlackRound : BlackSquare;
            }
            else
            {
                img = pawn.Type == PawnType.Round ? WhiteRound : WhiteSquare;
            }

            var px = ToPixels(cell);
            output.DrawImage(img, 100, new Size(img.Width, img.Height), new Point(px.X, px.Y));
        }


        public MemoryStream Render(PushFightGame game)
        {
            var output = new Image(Board.Width, Board.Height);

            output.DrawImage(Board, 100, new Size(Board.Width, Board.Height), new Point(0, 0));

            foreach (var ev in game.LastTurnEvents)
            {
                switch (ev.Type)
                {
                    case TurnEventType.Place:
                        DrawHighlight(game, output, new Point(ev.x1, ev.y1));
                        break;

                    case TurnEventType.Move:
                        DrawPath(game, output, new Point(ev.x1, ev.y1), new Point(ev.x2, ev.y2));
                        break;

                    case TurnEventType.Push:
                        DrawArrow(game, output, new Point(ev.x1, ev.y1), ev.Direction);
                        break;
                }
            }

            for (int x = 0; x < game.Board.GetLength(0); x++)
            {
                for (int y = 0; y < game.Board.GetLength(1); y++)
                {
                    var cell = game.Board[x, y];

                    if (cell.Highlight)
                    {
                        DrawHighlight(game, output, new Point(x, y));
                    }

                    DrawPiece(game, output, new Point(x, y), cell.Contents);

                    if (cell.Anchored)
                    {
                        var pos = ToPixels(new Point(x, y));
                        output.DrawImage(Anchor, 100, new Size(Anchor.Width, Anchor.Height), new Point(pos.X, pos.Y));
                    }
                }
            }

            var stream = new MemoryStream();
            output.SaveAsPng(stream);
            return stream;
        }
    }

}
