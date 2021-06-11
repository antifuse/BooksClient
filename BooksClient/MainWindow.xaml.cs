using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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
    public partial class MainWindow : Window
    {
        private Book SelectedBook;
        static readonly HttpClient Client = new HttpClient();
        private ObservableCollection<Book> books = new ObservableCollection<Book>();

        public MainWindow()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => { return true; };
            
            InitializeComponent();
            SearchBar.KeyDown += SearchBar_KeyDown;
            this.RefreshTable_Click(null, null);
        }

        private void SearchBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return) SearchButton_Click(sender, e);
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var res = await Client.GetAsync(Properties.Settings.Default.apiUrl + "/books?q=" + SearchBar.Text);
            Console.WriteLine(await res.Content.ReadAsStringAsync());
            books = await res.Content.ReadAsAsync<ObservableCollection<Book>>();
            AllGrid.ItemsSource = books;
        }

        private async void RefreshTable_Click(object sender, RoutedEventArgs e)
        {
            var res = await Client.GetAsync(Properties.Settings.Default.apiUrl + "/books");
            Console.WriteLine(await res.Content.ReadAsStringAsync());
            books = await res.Content.ReadAsAsync<ObservableCollection<Book>>();
            AllGrid.ItemsSource = books;
        }
       

        private async void SaveFormData_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedBook == null) return;
            SelectedBook.title = string.IsNullOrWhiteSpace(TitleBox.Text) ? null : TitleBox.Text;
            SelectedBook.author = string.IsNullOrWhiteSpace(AuthorBox.Text) ? null : AuthorBox.Text;
            SelectedBook.publisher = string.IsNullOrWhiteSpace(PublisherBox.Text) ? null : PublisherBox.Text;
            SelectedBook.year = string.IsNullOrWhiteSpace(YearBox.Text) ? null : YearBox.Text;
            SelectedBook.subtitle = string.IsNullOrWhiteSpace(SubtitleBox.Text) ? null : SubtitleBox.Text;
            SelectedBook.isbn = string.IsNullOrWhiteSpace(ISBNBox.Text) ? null : ISBNBox.Text;

            var result = await Client.PutAsJsonAsync<Book>(Properties.Settings.Default.apiUrl + "/book/" + SelectedBook.id, SelectedBook);
            Console.WriteLine(await result.Content.ReadAsStringAsync());
            SelectedBook = await result.Content.ReadAsAsync<Book>();
            FillInFields();
            AllGrid.ItemsSource = null;
            AllGrid.ItemsSource = books;
        }

        private void ResetFormData_Click(object sender, RoutedEventArgs e)
        {
            FillInFields();
        }

        private void EditCredentials_Click(object sender, RoutedEventArgs e)
        {
            var d = new CredentialsDialog
            {
                Owner = this
            };
            d.ShowDialog();
        }

        private void AllGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) SelectedBook = null;
            else SelectedBook = (Book)e.AddedItems[0];
            FillInFields();
        }

        private void FillInFields()
        {
            TitleBox.Text = SelectedBook?.title ?? "";
            AuthorBox.Text = SelectedBook?.author ?? "";
            PublisherBox.Text = SelectedBook?.publisher ?? "";
            ISBNBox.Text = SelectedBook?.isbn ?? "";
            YearBox.Text = SelectedBook?.year ?? "";
            SubtitleBox.Text = SelectedBook?.subtitle ?? "";
            LabelID.Content = "ID: " + SelectedBook?.id ?? "";
        }

        private void MenuImportBtn_Click(object sender, RoutedEventArgs e)
        {
            var d = new ImportWindow
            {
                Owner = this
                
            };
            d.Closed += (sendObj, args) => {
                this.Show();
                this.Focus();
                this.RefreshTable_Click(null, null);
            };
            d.Show();
        }

        private void DeleteTheThing_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult confirmation = MessageBox.Show($"Buch {SelectedBook.id} löschen?", "Bestätigen", MessageBoxButton.YesNo);
            if (confirmation == MessageBoxResult.Yes)
            {
                Client.DeleteAsync(Properties.Settings.Default.apiUrl + "/book/" + SelectedBook.id);
                books.Remove(SelectedBook);
                //((ObservableCollection<Book>)AllGrid.ItemsSource).Remove(SelectedBook);

            }
        }
    }
}
