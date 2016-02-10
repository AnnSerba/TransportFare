using System;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.System.Threading;

namespace TransportFare
{
    public sealed partial class Map : Page
    {
        Geolocator geolocator;
        Geoposition position;
        ThreadPoolTimer timer;
        public Map()
        {
            this.InitializeComponent();
            map.MapServiceToken = "nD18JaojG92g4lqLyhzI~wo1ajtwOwbPAoUAUUxu6Sg~ArEdm5wChACupRSXm50wPw8v7drL6dNMRMWztobTJOj1rrcNoh0uIch5I_XHnOvp";
            geolocator = new Geolocator { ReportInterval = 2000 };
            geolocator.PositionChanged += OnPositionChanged;
            geolocator.StatusChanged += OnStatusChanged;
            timer = ThreadPoolTimer.CreateTimer(new TimerElapsedHandler(Update),new TimeSpan(2000));
        }
        private void Update(ThreadPoolTimer e)
        {
            
        }
        private async void UpdateTracking()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    messageGeolocationAccessStatus.Visibility=Visibility.Collapsed;
                    map.Visibility = Visibility.Visible;
                    settings.Visibility = Visibility.Collapsed;
                    break;

                case GeolocationAccessStatus.Denied:
                    messageGeolocationAccessStatus.Text = "В доступе к получении текущего положения отказано";
                    messageGeolocationAccessStatus.Visibility = Visibility.Visible;
                    settings.Visibility = Visibility.Visible;
                    map.Visibility = Visibility.Collapsed;
                    break;

                case GeolocationAccessStatus.Unspecified:
                    messageGeolocationAccessStatus.Text = "В доступе к получении текущего положения неизвестная ошибка";
                    messageGeolocationAccessStatus.Visibility = Visibility.Visible;
                    settings.Visibility = Visibility.Visible;
                    map.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        
        async private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                position = e.Position;
            });
        }
        async private void OnStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateTracking();
                switch (e.Status)
                {
                    case PositionStatus.Ready:
                        messagePositionStatus.Text =DateTime.Now+ " Координаты местоположения готовы";
                        toMyPosition.IsEnabled = true;
                        break;

                    case PositionStatus.Initializing:
                        messagePositionStatus.Text = DateTime.Now + " Инициализация координат местаположения";
                        toMyPosition.IsEnabled = false;
                        break;

                    case PositionStatus.NoData:
                        messagePositionStatus.Text = DateTime.Now + " Не в состоянии определить координаты местоположения";
                        toMyPosition.IsEnabled = false;
                        break;

                    case PositionStatus.Disabled:
                        messagePositionStatus.Text = DateTime.Now + " Доступ к координатам местоположения отказано";
                        toMyPosition.IsEnabled = false;

                        break;

                    case PositionStatus.NotInitialized:
                        messagePositionStatus.Text = DateTime.Now + " Ни одна заявка для размещения не производится пока";
                        toMyPosition.IsEnabled = false;
                        break;

                    case PositionStatus.NotAvailable:
                        messagePositionStatus.Text = DateTime.Now + " Расположение не доступен на этой версии ОС";
                        toMyPosition.IsEnabled = false;
                        break;

                    default:
                        messagePositionStatus.Text = DateTime.Now + " Неизвестная ошибка";
                        toMyPosition.IsEnabled = false;
                        break;
                }
            });
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            MainMenu mainMenu = Window.Current.Content as MainMenu;
            if (mainMenu == null)
            {
                mainMenu = new MainMenu();
                Window.Current.Content = mainMenu;
            }
        }
        private void UpdateLocationData(Geoposition position)
        {
            if (position != null)
            {
                map.Center = position.Coordinate.Point;
                map.ZoomLevel = (double)geolocator.DesiredAccuracyInMeters;
                map.LandmarksVisible = true;
            }
        }
        private void ToMyPosition_Click(object sender, RoutedEventArgs e)
        {
            UpdateLocationData(position);
        }
        public void AddPoint(double latitude,double longitude,string name)
        {
            MapIcon mapIcon = new MapIcon();
            mapIcon.Location = new Geopoint(new BasicGeoposition() { Latitude = latitude, Longitude = longitude });
            mapIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
            mapIcon.Title = name;
            mapIcon.ZIndex = 0;
            map.MapElements.Add(mapIcon);
        }
    }
}
