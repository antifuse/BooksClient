using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BooksClient
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CredentialsDialog : Window
    {
        public CredentialsDialog()
        {
            InitializeComponent();
            pssPassword.Password = Properties.Settings.Default.blPassword;
            txtUsername.Text = Properties.Settings.Default.blUsername;
            txtApiURL.Text = Properties.Settings.Default.apiUrl;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (pssPassword.Password != "" && txtUsername.Text != "")
            {
                Properties.Settings.Default.blPassword = pssPassword.Password;
                Properties.Settings.Default.blUsername = txtUsername.Text;
                Properties.Settings.Default.apiUrl =  Regex.Replace(txtApiURL.Text, @"\/+$", "");
                Properties.Settings.Default.Save();
                this.DialogResult = true;
            } else
            {
                this.DialogResult = false;
            }
        }
    }
}
