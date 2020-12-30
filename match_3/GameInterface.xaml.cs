using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace match_3
{
    /// <summary>
    /// </summary>
    public partial class GameInterface : UserControl
    {
        private readonly Game game;

        public GameInterface()
        {
            InitializeComponent();
            game = new Game(RegisterTile, UnregisterTile, DropAnimation);
            DataContext = game;
        }

        private void RegisterTile(Tile tile)
        {
            tile.Shape.Height = GameCanvas.Height / 8;
            tile.Shape.Width = GameCanvas.Width / 8;
            tile.Shape.RenderTransform =
                new ScaleTransform(1.0, 1.0, tile.Shape.Height / 2, tile.Shape.Width / 2);
            GameCanvas.Children.Add(tile.Shape);
            Canvas.SetTop(tile.Shape, tile.Top * tile.Shape.Height);
            Canvas.SetLeft(tile.Shape, tile.Left * tile.Shape.Width);
        }


        private int dropAnimationRegister;

        private void DropAnimation(Tile tile)
        {
            dropAnimationRegister++;
            var animTop = new DoubleAnimation
            {
                To = tile.Top * tile.Shape.Height,
                Duration = TimeSpan.FromMilliseconds(200),
            };
            animTop.Completed += delegate
            {
                dropAnimationRegister--;
                if (dropAnimationRegister != 0) return;
                game.FillBoard(RegisterTile);
                game.RemoveMatches(DeleteAnimation);
            };
            tile.Shape.BeginAnimation(Canvas.TopProperty, animTop);
        }

        private int deleteAnimationRegister;

        private void DeleteAnimation(Tile tile)
        {
            deleteAnimationRegister += 2;
            var anim = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(200),
            };
            anim.Completed += delegate
            {
                deleteAnimationRegister--;
                if (deleteAnimationRegister == 0)
                {
                    game.DeleteAndDropTiles(
                        DropAnimation, RegisterTile, UnregisterTile);
                }
            };
            tile.Shape.RenderTransform.BeginAnimation(
                ScaleTransform.ScaleXProperty, anim);
            tile.Shape.RenderTransform.BeginAnimation(
                ScaleTransform.ScaleYProperty, anim);
        }

        private int successAnimationRegister;

        private void OnSuccessAnimationComplete(object o, EventArgs e)
        {
            successAnimationRegister--;
            if (successAnimationRegister == 0)
            {
                game.RemoveMatches(DeleteAnimation);
            }
        }

        private void SuccessAnimation(Tile first, Tile second)
        {
            successAnimationRegister += 2;
            AnimateSwap(first, second, OnSuccessAnimationComplete);
        }

        private void AnimateSwap(Tile first, Tile second, Action<object, EventArgs> onCompleted)
        {
            var dt = Math.Sign(Math.Abs(first.Top - second.Top));
            var dl = Math.Sign(Math.Abs(first.Left - second.Left));
            var animFirst = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(200),
            };
            var animSecond = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(200),
            };
            animFirst.Completed += (o, ea) => onCompleted(o, ea);
            animSecond.Completed += (o, ea) => onCompleted(o, ea);
            if (dt == 1)
            {
                animSecond.To = second.Top * second.Shape.Height;
                animFirst.To = first.Top * first.Shape.Height;
                first.Shape.BeginAnimation(Canvas.TopProperty, animFirst);
                second.Shape.BeginAnimation(Canvas.TopProperty, animSecond);
            }
            else if (dl == 1)
            {
                animSecond.To = second.Left * second.Shape.Width;
                animFirst.To = first.Left * first.Shape.Width;
                first.Shape.BeginAnimation(Canvas.LeftProperty, animFirst);
                second.Shape.BeginAnimation(Canvas.LeftProperty, animSecond);
            }
            else
            {
                throw new InvalidOperationException("По диагонали");
            }
        }

        private int failAnimationRegister;

        private void FailAnimation(Tile first, Tile second)
        {
            failAnimationRegister += 2;
            first.SwapCoordinates(ref second);
            AnimateSwap(
                first, second, (o1, e1) =>
                {
                    failAnimationRegister--;
                    if (failAnimationRegister != 0) return;
                    first.SwapCoordinates(ref second);
                    failAnimationRegister += 2;
                    AnimateSwap(first, second, (o2, e2) => { failAnimationRegister--; });
                });
        }

        private void UnregisterTile(Tile tile)
        {
            GameCanvas.Children.Remove(tile.Shape);
        }

        private Tile selected;

        private void GameCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (failAnimationRegister + deleteAnimationRegister +
                successAnimationRegister + dropAnimationRegister > 0)
            {
                return;
            }

            if (!(e.OriginalSource is TileShape ts)) return;
            var t = (Tile) ts.Tag;
            if (t.Selected)
            {
                t.Selected = false;
                selected = null;
            }
            else
            {
                if (selected != null)
                {
                    var tempTile = selected;
                    selected.Selected = false;
                    selected = null;
                    game.TrySwapTiles(t, tempTile, SuccessAnimation, FailAnimation);
                }
                else
                {
                    t.Selected = true;
                    selected = t;
                }
            }
        }
    }
}