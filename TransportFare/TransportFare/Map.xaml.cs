using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Globalization;
using System.Threading;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace TransportFare
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Map : Page
    {
        Geolocator geolocator;
        Geoposition position;
        public Map()
        {
            this.InitializeComponent();
            map.MapServiceToken = "nD18JaojG92g4lqLyhzI~wo1ajtwOwbPAoUAUUxu6Sg~ArEdm5wChACupRSXm50wPw8v7drL6dNMRMWztobTJOj1rrcNoh0uIch5I_XHnOvp";
            geolocator = new Geolocator { ReportInterval = 2000 };
            geolocator.PositionChanged += OnPositionChanged;
            geolocator.StatusChanged += OnStatusChanged;
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
                        messagePositionStatus.Text = "Координаты местоположения готовы";
                        toMyPosition.IsEnabled = true;
                        break;

                    case PositionStatus.Initializing:
                        messagePositionStatus.Text = "Инициализация координат местаположения";
                        toMyPosition.IsEnabled = false;
                        break;

                    case PositionStatus.NoData:
                        messagePositionStatus.Text = "Не в состоянии определить координаты местоположения";
                        toMyPosition.IsEnabled = false;
                        break;

                    case PositionStatus.Disabled:
                        messagePositionStatus.Text = "Доступ к координатам местоположения отказано";
                        toMyPosition.IsEnabled = false;

                        break;

                    case PositionStatus.NotInitialized:
                        messagePositionStatus.Text = "Ни одна заявка для размещения не производится пока";
                        toMyPosition.IsEnabled = false;
                        break;

                    case PositionStatus.NotAvailable:
                        messagePositionStatus.Text = "Расположение не доступен на этой версии ОС";
                        toMyPosition.IsEnabled = false;
                        break;

                    default:
                        messagePositionStatus.Text = "Неизвестная ошибка";
                        toMyPosition.IsEnabled = false;
                        break;
                }
            });
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            MainMenu mainMenu = Window.Current.Content as MainMenu;
            // если не создан
            if (mainMenu == null)
            {
                // создание 
                mainMenu = new MainMenu();

                // установка для текущего окна
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
    }
}
