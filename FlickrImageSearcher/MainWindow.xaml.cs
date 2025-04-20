using Microsoft.Win32;
using Newtonsoft.Json.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoSearcherFlickrAPI
{
    public partial class MainWindow : Window
    {
        // NOTE: Only for proof of concept is this key temporarily available for usage. Will be removed and key will be revoked after 6/25/2024.
        private const string FlickrApiKey = "fa13543cf51b501b694b7b88d61d0d30"; // GET ONE HERE: https://www.flickr.com/services/apps/create/noncommercial/?
        private const string FlickrApiUrl = "https://api.flickr.com/services/rest/?method=flickr.photos.search&api_key={0}&text={1}&per_page={2}&page={3}&format=json&nojsoncallback=1";

        private int currentPage = 1;
        private int perPage = 15;
        private int totalImagesLoaded = 0;

        public ObservableCollection<ImageItem> Images { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Images = new ObservableCollection<ImageItem>();
            DataContext = this;
            ImagesWrapPanel.Background = (SolidColorBrush)Application.Current.Resources["DarkPanelBrush"];
            AttachScrollEvent();
        }

        /// <summary>
        /// core task of flickr search with button click or text field
        /// </summary>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        private async Task<JObject> SearchFlickrImages(string query, int page, int perPage)
        {
            string url = string.Format(FlickrApiUrl, FlickrApiKey, query, perPage, page);
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return JObject.Parse(responseBody);
            }
        }

        /// <summary>
        /// Begin search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await BeginFlickrImageSearch();           
        }

        /// <summary>
        /// Triggers beginning of flickr search through enter key pressed, or button clicked.
        /// </summary>
        /// <returns></returns>
        private async Task BeginFlickrImageSearch()
        {
            string query = SearchTextBox.Text;
            if (string.IsNullOrWhiteSpace(query))
            {
                MessageBox.Show("Please enter a search query.");
                return;
            }

            currentPage = 1; // Reset current page to fetch from the beginning
            totalImagesLoaded = 0; // Reset total images loaded
            JObject response = await SearchFlickrImages(query, currentPage, perPage);
            DisplayImages(response);
        }

        /// <summary>
        /// Reset the content of txt/images loaded.
        /// </summary>
        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ImagesWrapPanel.Children.Clear();
            SearchTextBox.Text = string.Empty;
            ImagesWrapPanel.Background = (SolidColorBrush)Application.Current.Resources["DarkPanelBrush"];

            totalImagesLoaded = 0;
            ImageCountTextBlock.Text = $"Images Loaded: {totalImagesLoaded}";
            //PaginationCountTextBlock.Text = "15"; // no longer setting to a default, assume the user just wants to clear results/search text
        }

        /// <summary>
        /// Creates dynamic controls to add to ImageControlWrap block.
        /// </summary>
        private void DisplayImages(JObject jsonResponse)
        {
            // If the currentPage is 1 then just clear existing images and reset total count
            if (currentPage == 1)
            {
                ImagesWrapPanel.Children.Clear();
                totalImagesLoaded = 0;
            }

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
                    Source = new BitmapImage(new Uri(photoUrl)),
                    Width = 150,
                    Height = 150,
                    Margin = new Thickness(5)
                };

                // code to support right click and save
                // functionality on returned images
                ContextMenu contextMenu = new ContextMenu();
                contextMenu.Background = (SolidColorBrush)Application.Current.Resources["DarkPanelBrush"];
                contextMenu.Foreground = (SolidColorBrush)Application.Current.Resources["DarkForegroundBrush"];
                contextMenu.BorderBrush = (SolidColorBrush)Application.Current.Resources["DarkBorderBrush"];

                MenuItem saveMenuItem = new MenuItem();
                saveMenuItem.Header = "Save Image";
                saveMenuItem.Foreground = (SolidColorBrush)Application.Current.Resources["DarkForegroundBrush"];
                saveMenuItem.Background = (SolidColorBrush)Application.Current.Resources["DarkPanelBrush"];
                saveMenuItem.Click += (sender, args) =>
                {
                    SaveImage(photoUrl);
                };
                contextMenu.Items.Add(saveMenuItem);

                image.ContextMenu = contextMenu;

                // Add to DisplayImages method, after creating the Image control:
                image.Cursor = Cursors.Hand;
                image.MouseLeftButtonDown += (s, args) => 
                {
                    // Get the original sized image URL by replacing "_m" with "_b" for larger size
                    string largePhotoUrl = photoUrl.Replace("_m.jpg", "_b.jpg");
                    
                    // Show the modal with zoom functionality
                    ImageViewerModal modal = new ImageViewerModal(largePhotoUrl, title);
                    modal.Owner = this;
                    modal.ShowDialog();
                };

                TextBlock titleTextBlock = new TextBlock
                {
                    Text = title,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Width = 150
                };

                StackPanel imagePanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(5),
                    Width = 150,
                    Background = (SolidColorBrush)Application.Current.Resources["DarkPanelBrush"]
                };

                imagePanel.Children.Add(image);
                imagePanel.Children.Add(titleTextBlock);

                ImagesWrapPanel.Children.Add(imagePanel);

                totalImagesLoaded++;
            }

            // updating our text block to show the count of total images loaded
            ImageCountTextBlock.Text = $"Images Loaded: {totalImagesLoaded}";

            currentPage++;
        }

        /// <summary>
        /// Added to every image found. Basically, when a user right clicks an image, save the file and download it to where they select.
        /// </summary>
        private void SaveImage(string imageUrl)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "image"; // Default file name
            saveFileDialog.DefaultExt = ".jpg"; // Default file extension
            saveFileDialog.Filter = "JPEG Image (.jpg)|*.jpg"; // Filter files by extension

            if (saveFileDialog.ShowDialog() == true)
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(imageUrl, saveFileDialog.FileName);
                }
            }
        }

        /// <summary>
        /// attach the scroll event to our found (see method code scroller below) scroll viewer control.
        /// </summary>
        private void AttachScrollEvent()
        {
            var scrollViewer = GetScrollViewer(ImagesScrollViewer);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += ImagesScrollViewer_ScrollChanged;
            }
        }

        /// <summary>
        /// find the scroll viewer child control
        /// </summary>
        private ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Event method to handle ImagesScrollViewer component/control for pagination.
        /// </summary>
        private void ImagesScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            // See if the user has scrolled to the bottom of the ScrollViewer WPF control
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                string query = SearchTextBox.Text;
                if (!string.IsNullOrWhiteSpace(query))
                {
                    SearchFlickrImages(query, currentPage, perPage)
                        .ContinueWith(task =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                DisplayImages(task.Result);
                            });
                        });
                }
            }
        }

        /// <summary>
        /// Allows the user to set the pagination page request amount 
        /// </summary>
        private void PaginationCountTextBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            int perPage = 15;
            try
            {
                perPage = int.Parse(PaginationCountTextBlock.Text);
            }
            catch (Exception)
            {
                //this.perPage = (int)perPage;
                PaginationCountTextBlock.Text = perPage.ToString();
            }
            finally
            {
                this.perPage = perPage;
            }
        }

        /// <summary>
        /// Listening to Key.Enter WND_Proc event/key press and searching.
        /// </summary>
        private async void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await BeginFlickrImageSearch();
            }
        }
    }
}
