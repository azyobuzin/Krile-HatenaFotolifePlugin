using System.Windows;

namespace HatenaFotolifePlugin
{
    /// <summary>
    /// ConfigWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Setting.Instance.Save();
            this.Close();
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            (this.DataContext as ConfigWindowViewModel).Setting.Password = this.Password.Password;   
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Password.Password = (this.DataContext as ConfigWindowViewModel).Setting.Password;
        }
    }
}
