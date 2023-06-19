using MapControl;
using MapControl.Caching;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace FrontLine.Visualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static MainWindow()
        {
            try
            {
                ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "RurouniJones.Dcs.FrontLine.Visualizer");

                TileImageLoader.Cache = new ImageFileCache(TileImageLoader.DefaultCacheFolder);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            MainMap.MapLayer = new MapTileLayer
            {
                TileSource = new TileSource { UriFormat = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" },
                SourceName = "OpenStreetMap",
                Description = "© [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)"
            };

            if (TileImageLoader.Cache is ImageFileCache cache)
            {
                Loaded += async (s, e) =>
                {
                    await Task.Delay(2000);
                    await cache.Clean();
                };
            }
        }
    }
}
