using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace BooksClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class MutableKeyValuePair<TKey, TValue>
    {
        public MutableKeyValuePair(TKey key, TValue value)
        {
            this.Key = key;
            this.Value = value;
        } 
        public MutableKeyValuePair() { }
        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }

    public partial class ImportWindow : Window

    {
        static readonly HttpClient Client = new HttpClient();
        public ObservableCollection<Book> DataAll = new ObservableCollection<Book>();
        public Dictionary<string, string> CurrentData = new Dictionary<string, string>();
        ApiService _api = new ApiService();
        public ImportWindow()
        {
            InitializeComponent();
            txtRestUrl.Text = Properties.Settings.Default.apiUrl;
            isbnInput.Focus();
        }

        private void Isbn_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.AutomaticInput == true && isbnInput.Text != "")
            {
                this.SearchButton_Click(searchButton, null);
            }
        }

        public bool AutomaticInput { get; set; }

        public ComboBoxItem ApiSelection { get; set; } = new ComboBoxItem();

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"Search performed: {isbnInput.Text} searched on {ApiSelection.Content}");
            if (isbnInput.Text == "") AcceptRetrieved_Click(null, null);
            CurrentData = _api.SearchForIsbn(isbnInput.Text, (string)ApiSelection.Content);
            if (CurrentData == null) return;
            currentTable.ItemsSource = new ObservableCollection<MutableKeyValuePair<string,string>>(CurrentData.Select(i => new MutableKeyValuePair<string, string>(i.Key, i.Value)).ToList());
            isbnInput.Clear();
        }
        
        private async void Upload_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.apiUrl = Regex.Replace(txtRestUrl.Text, @"\/+$", "");
            Properties.Settings.Default.Save();
            var response = await Client.PostAsJsonAsync<IEnumerable<Book>>(Properties.Settings.Default.apiUrl + "/books/bulk", allEntries.Items.OfType<Book>());
            Console.WriteLine(await response.RequestMessage.Content.ReadAsStringAsync());
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            //Client.PostAsync(Properties.Settings.Default.apiUrl + "/books/bulk", new StringContent(JsonConvert.SerializeObject(allEntries.Items.Cast<Book>()), Encoding.UTF8, "application/json"));

        }

        private void AcceptRetrieved_Click(object sender, RoutedEventArgs e)
        {
            CurrentData = new Dictionary<string, string>();
            currentTable.ItemsSource.Cast<MutableKeyValuePair<string, string>>().ToList().ForEach(i => CurrentData.Add(i.Key, i.Value));
            string json = JsonConvert.SerializeObject(CurrentData);
            Book book = JsonConvert.DeserializeObject<Book>(json);
            DataAll.Add(book);
            allEntries.ItemsSource = DataAll;
            Console.WriteLine(book);
            CurrentData = new Dictionary<string, string>();
            currentTable.ItemsSource = CurrentData;
            isbnInput.Focus();
        }

        private void EditCredentials_Click(object sender, RoutedEventArgs e)
        {
            var d = new CredentialsDialog
            {
                Owner = this
            };
            d.ShowDialog();
        }
        
        
    }
}
