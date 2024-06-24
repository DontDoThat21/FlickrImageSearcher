using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoSearcherFlickrAPI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string FlickrApiKey = "[USE_YOUR_FLICKR_API_HERE]"; // GET ONE HERE: https://www.flickr.com/services/apps/create/noncommercial/?
        private const string FlickrApiUrl = "https://api.flickr.com/services/rest/?method=flickr.photos.search&api_key={0}&text={1}&format=json&nojsoncallback=1";

        public ObservableCollection<ImageItem> Images { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Images = new ObservableCollection<ImageItem>();
            DataContext = this;
            ImagesWrapPanel.Background = Brushes.LightGray;
        }

        private async Task<JObject> SearchFlickrImages(string query)
        {
            string url = string.Format(FlickrApiUrl, FlickrApiKey, query);
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return JObject.Parse(responseBody);
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchTextBox.Text;
            if (string.IsNullOrWhiteSpace(query))
            {
                MessageBox.Show("Please enter a search query.");
                return;
            }

            JObject response = await SearchFlickrImages(query);
            DisplayImages(response);
        }

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ImagesWrapPanel.Children.Clear();
            SearchTextBox.Text = string.Empty;
            ImagesWrapPanel.Background = Brushes.LightGray;
        }

        private void DisplayImages(JObject jsonResponse)
        {
            ImagesWrapPanel.Children.Clear();
            var photos = jsonResponse["photos"]["photo"];
            foreach (var photo in photos)
            {
                string photoId = photo["id"].ToString();
                string owner = photo["owner"].ToString();
                string secret = photo["secret"].ToString();
                string server = photo["server"].ToString();
                string farm = photo["farm"].ToString();
                string title = photo["title"].ToString();

                string photoUrl = $"https://farm{farm}.staticflickr.com/{server}/{photoId}_{secret}_m.jpg";

                Image image = new Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(photoUrl)),
                    Width = 150,
                    Height = 150,
                    Margin = new Thickness(5)
                };

                TextBlock titleTextBlock = new TextBlock
                {
                    Text = title,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Width = 150 // Adjust as needed
                };

                StackPanel imagePanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(5),
                    Width = 150 // Adjust as needed
                };

                imagePanel.Children.Add(image);
                imagePanel.Children.Add(titleTextBlock);

                ImagesWrapPanel.Children.Add(imagePanel);
            }
        }

    }
}
