using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace match_3
{
    /// <summary>
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Switcher.PageSwitcher = this;
            Switcher.Switch(new MainMenu());
        }

        public void Navigate(UserControl nextPage)
        {
            Content = nextPage;
        }
    }
}
