using Microsoft.Win32;
using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoSearcherFlickrAPI
{
    public partial class ImageViewerModal : Window
    {
        private string _imageUrl;
        private double _currentZoom = 1.0;
        private const double _zoomIncrement = 0.2;
        private const double _minZoom = 0.2;
        private const double _maxZoom = 5.0;

        public ImageViewerModal(string imageUrl, string title = null)
        {
            InitializeComponent();
            
            _imageUrl = imageUrl;
            
            if (!string.IsNullOrEmpty(title))
            {
                this.Title = title;
            }
            
            // Load the image
            ImageDisplay.Source = new BitmapImage(new Uri(imageUrl));
            
            UpdateZoomText();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            AdjustZoom(_zoomIncrement);
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            AdjustZoom(-_zoomIncrement);
        }

        private void ResetZoomButton_Click(object sender, RoutedEventArgs e)
        {
            _currentZoom = 1.0;
            ApplyZoom();
        }

        private void ImageDisplay_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                AdjustZoom(_zoomIncrement);
            }
            else
            {
                AdjustZoom(-_zoomIncrement);
            }
        }

        private void AdjustZoom(double increment)
        {
            _currentZoom += increment;
            
            // Restrict zoom level to min/max values
            _currentZoom = Math.Max(_minZoom, Math.Min(_maxZoom, _currentZoom));
            
            ApplyZoom();
        }

        private void ApplyZoom()
        {
            ImageScaleTransform.ScaleX = _currentZoom;
            ImageScaleTransform.ScaleY = _currentZoom;
            
            UpdateZoomText();
        }

        private void UpdateZoomText()
        {
            int zoomPercentage = (int)(_currentZoom * 100);
            ZoomLevelText.Text = $"Zoom: {zoomPercentage}%";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(_imageUrl);
        }

        private void SaveImage(string imageUrl)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "image"; // Default file name
            saveFileDialog.DefaultExt = ".jpg"; // Default file extension
            saveFileDialog.Filter = "JPEG Image (.jpg)|*.jpg"; // Filter files by extension

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.DownloadFile(imageUrl, saveFileDialog.FileName);
                        MessageBox.Show("Image saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
