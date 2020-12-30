using System.Windows;
using System.Windows.Controls;

namespace match_3
{
    /// <summary>
    /// </summary>
    public partial class MainMenu : UserControl
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new GameInterface());
        }
    }
}
