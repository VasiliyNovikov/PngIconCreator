using System;
using System.Windows;
using System.Windows.Controls;

namespace PngIconCreator
{
    public partial class UrlWindow : Window
    {

        public UrlWindow()
        {
            InitializeComponent();
        }

        private void OnUrlChanged(object sender, TextChangedEventArgs e)
        {
            OkBtn.IsEnabled = !String.IsNullOrWhiteSpace(UrlTxt.Text);
        }

        public static string GetUrl()
        {
            var wnd = new UrlWindow();
            return wnd.ShowDialog() == true ? wnd.UrlTxt.Text : null;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
