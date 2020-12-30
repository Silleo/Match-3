using System.Windows;
using System.Windows.Controls;

namespace match_3
{
    /// <summary>
    /// </summary>
    public partial class GameOver : UserControl
    {

        public int Points { get; }

        public GameOver(int points)
        {
            InitializeComponent();
            Points = points;
            DataContext = this;
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new MainMenu());
        }
    }
}
