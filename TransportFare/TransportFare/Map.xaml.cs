using System;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using System.Threading.Tasks;
using Windows.Services.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace TransportFare
{
    public sealed partial class Map : Page
    {
        Geolocator geolocator;
        Geoposition geoposition;
        MapLocation mapLocation;
        public Map()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += Map_BackRequested;
            mapControl.MapServiceToken = "nD18JaojG92g4lqLyhzI~wo1ajtwOwbPAoUAUUxu6Sg~ArEdm5wChACupRSXm50wPw8v7drL6dNMRMWztobTJOj1rrcNoh0uIch5I_XHnOvp";
            geolocator = new Geolocator { ReportInterval = 2000 };
            geolocator.PositionChanged += OnPositionChanged;
            geolocator.StatusChanged += OnStatusChanged;
            geolocator.DesiredAccuracy = PositionAccuracy.High;
            comboBoxDesiredAccuracy.Items.Add("Особо точный");
            comboBoxDesiredAccuracy.SelectedIndex = 0;
            comboBoxDesiredAccuracy.Items.Add("Экономный");
            LoadCoordinates();
        }

        private void Map_BackRequested(object sender, BackRequestedEventArgs e)
        {
            MainMenu mainMenu = Window.Current.Content as MainMenu;
            if (mainMenu == null)
            {
                mainMenu = new MainMenu();
                Window.Current.Content = mainMenu;
            }
        }

        async void LoadCoordinates()
        {
            symbolIconState.Symbol = Symbol.Target;
            appBarButtonState.Label = "Получение";
            textBlockStatus.Text = DateTime.Now + "Получение координат местоположения";
            stackPanelStatus.Background = new SolidColorBrush(Colors.Blue);
            geoposition = await geolocator.GetGeopositionAsync();

            MapLocationFinderResult mapLocationFinderResult =
                await MapLocationFinder.FindLocationsAtAsync(geoposition.Coordinate.Point);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateLocationData();
                splitView.IsPaneOpen = true;
                progressRingMap.IsActive = false;
                mapControl.Visibility = Visibility.Visible;
                textBlockLocation.Visibility = Visibility.Visible;
                if (mapLocationFinderResult.Status == MapLocationFinderStatus.Success)
                {
                    mapLocation = mapLocationFinderResult.Locations[0];
                }
                textBlockLocation.Visibility = Visibility.Visible;
                SetState("Скачивание данных", "Скачивание данных", Symbol.Download, Colors.Gold);
                LoadRoutes(LoadSities());
                SetState("Успех", "Данные успешно получены", Symbol.Like, Colors.Green);
            });
        }
        private string LoadSities()
        {
            Load.Sities().ForEach(i => { comboBoxSity.Items.Add(i); });
            if (comboBoxSity.Items.Count != 0)
            {
                if (mapLocation != null && comboBoxSity.Items.Contains(mapLocation.Address.Town))
                {
                    comboBoxSity.SelectedIndex = comboBoxSity.Items.IndexOf(mapLocation.Address.Town);
                }
                else if (comboBoxSity.Items.Count != 0)
                {
                    comboBoxSity.SelectedIndex = 0;
                }
                progressRingSity.IsActive = false;
                comboBoxSity.Visibility = Visibility.Visible;
                progressRingRoute.IsActive = true;
                return comboBoxSity.SelectedValue.ToString();
            }
            return "";
        }
        private void LoadRoutes(string sity)
        {
            Load.Routes(sity).ForEach(i => { comboBoxRoute.Items.Add(i); });
            comboBoxRoute.SelectedIndex = 0;
            comboBoxRoute.Visibility = Visibility.Visible;
            progressRingRoute.IsActive = false;
        }
        async private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetState("Получение", "Получение координат местоположения", Symbol.Target, Colors.Blue);
                geoposition = e.Position;
            });
            MapLocationFinderResult mapLocationFinderResult =
                await MapLocationFinder.FindLocationsAtAsync(geoposition.Coordinate.Point);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (mapLocationFinderResult.Status == MapLocationFinderStatus.Success)
                {
                    mapLocation = mapLocationFinderResult.Locations[0];
                    textBlockLocation.Text = "Текущее местоположение: " +
                        mapLocation.Address.Country + " " +
                          mapLocation.Address.Town + " " +
                          mapLocation.Address.Street + " " +
                           mapLocation.Address.StreetNumber;
                    SetState("Успех", "Координаты местоположения успешно получены", Symbol.Like, Colors.Green);
                }
            });
        }

        async private void OnStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (e.Status)
                {
                    case PositionStatus.Ready:
                        SetState("Готов. Ожидание", "Готов к получению координат. Ожидание запроса на получение координат", Symbol.Clock, Colors.Green);
                        break;

                    case PositionStatus.Initializing:
                        SetState("Инициализация", "Инициализация координат местаположения", Symbol.Upload, Colors.Blue);
                        break;

                    case PositionStatus.NoData:
                        SetState("Не определено", "Не в состоянии определить координаты местоположения", Symbol.DisableUpdates, Colors.Red);
                        break;

                    case PositionStatus.Disabled:
                        SetState("Отказано", " Доступ к координатам местоположения отказано", Symbol.DisconnectDrive, Colors.Red);
                        break;

                    case PositionStatus.NotInitialized:
                        SetState("Нет заявок", "Ни одна заявка для размещения не производится пока", Symbol.Preview, Colors.Gold);
                        break;

                    case PositionStatus.NotAvailable:
                        SetState("Не доступен", "Расположение не доступен на этой версии ОС", Symbol.ProtectedDocument, Colors.Red);
                        break;

                    default:
                        SetState("Ошибка", " Неизвестная ошибка", Symbol.Important, Colors.Red);
                        break;
                }
            });
        }
        private void SetState(string shortState,string longState,Symbol symbol,Color color)
        {
            appBarButtonState.Label = shortState;
            symbolIconState.Symbol = symbol;
            textBlockStatus.Text = DateTime.Now + " " + longState;
            stackPanelStatus.Background = new SolidColorBrush(color);
        }
        private void UpdateLocationData()
        {
            if (geoposition != null)
            {
                mapControl.Center = geoposition.Coordinate.Point;
                mapControl.ZoomLevel = (double)geolocator.DesiredAccuracyInMeters;
                mapControl.LandmarksVisible = true;
            }
        }
        public void AddPoint(double latitude, double longitude, string name)
        {
            MapIcon mapIcon = new MapIcon();
            mapIcon.Location = new Geopoint(new BasicGeoposition() { Latitude = latitude, Longitude = longitude });
            mapIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
            mapIcon.Title = name;
            mapIcon.ZIndex = 0;
            mapControl.MapElements.Add(mapIcon);
        }

        private void comboBoxSity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private void comboBoxRoute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void appBarButtonMap_Click(object sender, RoutedEventArgs e)
        {
            UpdateLocationData();
        }

        private void appBarButtonSetting_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = !splitView.IsPaneOpen;
        }

        private void appBarButtonState_Click(object sender, RoutedEventArgs e)
        {
            stackPanelStatus.Visibility = Visibility.Visible;
        }

        private void comboBoxDesiredAccuracy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource != null)
            {
                if (e.OriginalSource.Equals("Особо точный"))
                {
                    geolocator.DesiredAccuracy = PositionAccuracy.High;
                }
                else if (e.OriginalSource.Equals("Экономный"))
                {
                    geolocator.DesiredAccuracy = PositionAccuracy.Default;
                }
            }
        }

        private void buttonStatus_Click(object sender, RoutedEventArgs e)
        {
            stackPanelStatus.Visibility = Visibility.Collapsed;
        }
    }
}
