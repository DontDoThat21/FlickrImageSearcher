using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoSearcherFlickrAPI
{
    /// <summary>
    /// used in an observable collection to deliver images to Wrap Scroll View WPF control and dynamically create it's UI when the API returns.
    /// </summary>
    public class ImageItem
    {
        public string ImageUrl { get; set; }
        public string Title { get; set; }
    }
}
