using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using match_3.Annotations;

namespace match_3
{
    public sealed class Game : INotifyPropertyChanged
    {
        public int Points
        {
            get => points;
            set
            {
                points = value;
                OnPropertyChanged();
            }
        }

        public int Countdown
        {
            get => countdown;
            set
            {
                countdown = value;
                OnPropertyChanged();
            }
        }

        public Game(Action<Tile> registerTile, Action<Tile> unregisterTile, Action<Tile> dropAnimation)
        {
            FillBoard(registerTile);
            DeleteAndDropTiles(dropAnimation, registerTile, unregisterTile);
            gameTimer = new DispatcherTimer(
                new TimeSpan(0, 0, 1), DispatcherPriority.Normal,
                delegate
                {
                    Countdown -= 1;
                    if (Countdown == 0)
                    {
                        Switcher.Switch(new GameOver(Points));
                    }
                }, Application.Current.Dispatcher);
        }

        public void RemoveMatches(Action<Tile> deleteAnimation)
        {
            lastMatches = CheckMatches();
            Points += lastMatches.Count;
            foreach (var match in lastMatches)
            {
                deleteAnimation(match);
            }
        }

        private readonly Tile[,] board = new Tile[16, 8];

        private readonly Color[] colors = {Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Purple};

        public void FillBoard(Action<Tile> registerTileCallback)
        {
            var r = new Random();
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    if (board[i, j] != null) continue;
                    board[i, j] = new Tile(i - 8, j, colors[r.Next(colors.Length)]);
                    registerTileCallback(board[i, j]);
                }
            }
        }

        private List<Tile> lastMatches = new List<Tile>();
        private int points;
        private int countdown = 60;
        private readonly DispatcherTimer gameTimer;

        public void TrySwapTiles(Tile first, Tile second, Action<Tile, Tile> successAnimCallback, Action<Tile, Tile> failAnimCallback)
        {
            if (Math.Abs(first.Top - second.Top) + Math.Abs(first.Left - second.Left) > 1)
            {
                return;
            }

            Utility.Swap(
                ref board[first.Top + 8, first.Left],
                ref board[second.Top + 8, second.Left]);
            lastMatches = CheckMatches();
            if (lastMatches.Count > 0)
            {
                first.SwapCoordinates(ref second);
                successAnimCallback(first, second);
            }
            else
            {
                Utility.Swap(
                    ref board[first.Top + 8, first.Left],
                    ref board[second.Top + 8, second.Left]);
                failAnimCallback(first, second);
            }
        }

        private void DeleteMatches(Action<Tile> unregisterTile)
        {
            foreach (var match in lastMatches)
            {
                unregisterTile(board[match.Top + 8, match.Left]);
                board[match.Top + 8, match.Left] = null;
            }
        }

        public void DeleteAndDropTiles(Action<Tile> tileDropAnimation, Action<Tile> registerTile, Action<Tile> unregisterTile)
        {
            DeleteMatches(unregisterTile);
            var dropLengths = new int[8];
            for (int i = 16 - 1; i >= 0; i--)
            {
                for (var j = 0; j < 8; j++)
                {
                    if (board[i, j] == null)
                    {
                        dropLengths[j]++;
                    }
                    else if (dropLengths[j] != 0)
                    {
                        if (board[i + dropLengths[j], j] != null)
                        {
                            throw new InvalidOperationException("Здесь не ноль");
                        }

                        Utility.Swap(ref board[i, j], ref board[i + dropLengths[j], j]);
                        board[i + dropLengths[j], j].Top = i + dropLengths[j] - 8;
                        tileDropAnimation(board[i + dropLengths[j], j]);
                    }
                }
            }

            FillBoard(registerTile);
        }

        private List<Tile> CheckMatches()
        {
            var delete = new bool[16, 8];
            for (var i = 8; i < 16; i++)
            {
                var matches = 1;
                var color = board[i, 0].Color;
                for (var j = 1; j < 8; j++)
                {
                    if (board[i, j].Color == color)
                    {
                        ++matches;
                    }
                    else
                    {
                        if (matches >= 3)
                        {
                            for (var k = 1; k < matches + 1; k++)
                            {
                                delete[i, j - k] = true;
                            }
                        }

                        color = board[i, j].Color;
                        matches = 1;
                    }
                }

                if (matches < 3) continue;
                for (var k = 1; k < matches + 1; k++)
                {
                    delete[i, 8 - k] = true;
                }
            }

            for (var i = 0; i < 8; i++)
            {
                var matches = 1;
                var color = board[8, i].Color;
                for (var j = 9; j < 16; j++)
                {
                    if (board[j, i].Color == color)
                    {
                        ++matches;
                    }
                    else
                    {
                        if (matches >= 3)
                        {
                            for (var k = 1; k < matches + 1; k++)
                            {
                                delete[j - k, i] = true;
                            }
                        }

                        color = board[j, i].Color;
                        matches = 1;
                    }
                }
                
                if (matches < 3) continue;
                for (var k = 1; k < matches + 1; k++)
                {
                    delete[16 - k, i] = true;
                }
            }

            var result = new List<Tile>();
            for (var i = 8; i < 16; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    if (delete[i, j])
                    {
                        result.Add(board[i, j]);
                    }
                }
            }

            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}