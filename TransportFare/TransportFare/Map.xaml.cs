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
using Windows.UI.Xaml.Controls.Primitives;
using Windows.Storage.Streams;
using System.Collections.Generic;

namespace TransportFare
{
    public sealed partial class Map : Page
    {
        Geolocator Geolocator { get; set; }
        Geoposition Geoposition { get; set; }
        MapLocation MapLocation { get; set; }
        MapIcon MapIcon { get; set; }
        SortedList<string,List<Geopoint>> Stops { get; set; }
    List<MapIcon> ListMapIcon { get; set; }
        string Sity { get; set; }
        public Map()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += Map_BackRequested;
            mapControl.MapServiceToken = "nD18JaojG92g4lqLyhzI~wo1ajtwOwbPAoUAUUxu6Sg~ArEdm5wChACupRSXm50wPw8v7drL6dNMRMWztobTJOj1rrcNoh0uIch5I_XHnOvp";
            ListMapIcon = new List<MapIcon>();
            Stops = new SortedList<string, List<Geopoint>>();
            Geolocator = new Geolocator { ReportInterval = 3000 };
            sliderFrequency.Value = 2000;
            Geolocator.PositionChanged += OnPositionChanged;
            Geolocator.StatusChanged += OnStatusChanged;
            Geolocator.DesiredAccuracy = PositionAccuracy.High;
            comboBoxDesiredAccuracy.Items.Add("Особо точный");
            comboBoxDesiredAccuracy.SelectedIndex = 0;
            comboBoxDesiredAccuracy.Items.Add("Экономный");
            sliderZoom.Minimum = mapControl.MinZoomLevel;
            sliderZoom.Maximum = mapControl.MaxZoomLevel;
            mapControl.PedestrianFeaturesVisible = true;
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
        private void LoadSities()
        {
            try
            {
                SetState("Скачивание данных", null, "Скачивание данных", null, null, Symbol.Download, Colors.Gold);
                Load.Sities().ForEach(i => { comboBoxSity.Items.Add(i); });

                progressRingSity.IsActive = false;
                progressRingRoute.IsActive = true;

                comboBoxSity.Visibility = Visibility.Visible;
                if (Sity != null && comboBoxSity.Items.Contains(Sity))
                {
                    comboBoxSity.SelectedIndex = comboBoxSity.Items.IndexOf(Sity);
                }
                else if (comboBoxSity.Items.Count != 0)
                {
                    comboBoxSity.SelectedIndex = 0;
                }
                else
                {
                    throw new Exception("Ошибка списка данных городов");
                }
                UpdateLocationData();
                SetState("Успех", null, "Данные успешно получены", null, null, Symbol.Like, Colors.Green);
            }
            catch (Exception e)
            {
                SetState("Ошибка", null, e.Message, null, null, Symbol.Like, Colors.Green);
            }
        }
        private void comboBoxSity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetState("Скачивание данных", null, null, "Скачивание данных о маршрутах", null, Symbol.Download, Colors.Gold);
                if (comboBoxSity.SelectedValue.ToString() != "")
                {
                    listViewRoutes.Items.Clear();
                    Load.Routes(comboBoxSity.SelectedValue.ToString()).ForEach(i => {
                        listViewRoutes.Items.Add(i);
                    });
                    listViewRoutes.Visibility = Visibility.Visible;
                    progressRingRoute.IsActive = false;
                }
                else
                {
                    throw new Exception("Город не может быть пустым");
                }
                SetState("Успех", null, null, "Данные о маршрутах успешно получены", null, Symbol.Like, Colors.Green);
            }
            catch (Exception ex)
            {
                SetState("Ошибка", null, null, ex.Message, null, Symbol.Like, Colors.Green);
            }
        }
        private void listBoxRoutes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetState("Обновление данных", null, null, null, "Скачивание данных о транспорте", Symbol.Download, Colors.Gold);
                ListMapIcon.ForEach(j =>
                {
                    RemoveMapIcon(j);
                    mapControl.Routes.Clear();
                });
                ListMapIcon.Clear();
                foreach (var i in listViewRoutes.SelectedItems)
                {
                    Load.Transport(i.ToString()).ForEach(j =>
                    {
                        ListMapIcon.Add(AddGeopoint(j, i.ToString()));
                    });
                    if (Stops.Keys.Contains(i.ToString()) == false)
                    {
                        Stops.Add(i.ToString(), new List<Geopoint>());
                        Stops[i.ToString()] = Load.Way(i.ToString());
                    }
                    ShowRouteOnMap(Stops[i.ToString()],Colors.LightSkyBlue);
                }
                SetState("Успех", null, null, null, "Данные о транспорте успешно получены", Symbol.Like, Colors.Green);
            }
            catch (Exception ex)
            {
                SetState("Ошибка", null, null, ex.Message, null, Symbol.Like, Colors.Green);
            }
        }
        
        async private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetState("Получение", "Получение координат местоположения", null, null, null, Symbol.Target, Colors.Blue);
                 if (Geoposition==null|| e.Position.Coordinate.Accuracy<=Geoposition.Coordinate.Accuracy )
                {
                    if (MapIcon != null)
                    {
                        RemoveMapIcon(MapIcon);
                    }
                    MapIcon = AddGeopoint(e.Position.Coordinate.Point, "Вы");
                }
            });
            MapLocationFinderResult mapLocationFinderResult =
                await MapLocationFinder.FindLocationsAtAsync(e.Position.Coordinate.Point);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    if (Geoposition == null || e.Position.Coordinate.Accuracy <= Geoposition.Coordinate.Accuracy)
                    {
                        MapLocation = mapLocationFinderResult.Locations[0];
                        if (MapLocation == null)
                        {
                            throw new Exception("Адресс не определён");
                        }
                        Sity = MapLocation.Address.Town;
                        if (Geoposition == null)
                        {
                            LoadSities();
                            Geoposition = e.Position;
                            UpdateLocationData();
                        }
                        else
                        {
                            Geoposition = e.Position;
                        }
                        SetAddress();
                        
                    }
                }
                catch (Exception ex)
                {
                    SetState("Ошибка", ex.Message, null, null, null, Symbol.Target, Colors.Blue);
                    return;
                }
            });
        }

        string SetAddress()
        {
            if (MapLocation != null)
            {
                var stringAddress = DateTime.Now+" "+  MapLocation.Address.Country + " " +
                              MapLocation.Address.Town;
                if (Geoposition.Coordinate.Accuracy < 500)
                {
                    stringAddress += " " + MapLocation.Address.District + " ";
                }
                if (Geoposition.Coordinate.Accuracy < 300)
                {
                    stringAddress += " " + MapLocation.Address.Street + " ";
                }
                if (Geoposition.Coordinate.Accuracy < 10)
                {
                    stringAddress += " " + MapLocation.Address.StreetNumber;
                }
                return stringAddress +". Координаты: L: " + 
                    Geoposition.Coordinate.Latitude + " B: " +
                      Geoposition.Coordinate.Longitude + " Точность: " + 
                      Geoposition.Coordinate.Accuracy + "м";
            }
            else
            {
                throw new Exception("Нет данных о координатах");
            }
        }

        async private void OnStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (e.Status)
                {
                    case PositionStatus.Ready:
                        SetState("Готов. Ожидание", "Готов к получению координат. Ожидание запроса на получение координат", null, null, null, Symbol.Clock, Colors.Green);
                        break;

                    case PositionStatus.Initializing:
                        SetState("Инициализация", "Инициализация координат местаположения", null, null, null, Symbol.Upload, Colors.Blue);
                        break;

                    case PositionStatus.NoData:
                        SetState("Не определено", "Не в состоянии определить координаты местоположения", null, null, null, Symbol.DisableUpdates, Colors.Red);
                        break;

                    case PositionStatus.Disabled:
                        SetState("Отказано", " Доступ к координатам местоположения отказано", null, null, null, Symbol.DisconnectDrive, Colors.Red);
                        break;

                    case PositionStatus.NotInitialized:
                        SetState("Нет заявок", "Ни одна заявка для размещения не производится пока", null, null, null, Symbol.Preview, Colors.Gold);
                        break;

                    case PositionStatus.NotAvailable:
                        SetState("Не доступен", "Расположение не доступен на этой версии ОС", null, null, null, Symbol.ProtectedDocument, Colors.Red);
                        break;

                    default:
                        SetState("Ошибка", " Неизвестная ошибка", null, null, null, Symbol.Important, Colors.Red);
                        break;
                }
            });
        }

        private void SetState(string shortGeoState, string longGeoState,
            string longSityState, string longRouteState, string longTransportState,
            Symbol symbol, Color color)
        {
            appBarButtonState.Label = shortGeoState;
            symbolIconState.Symbol = symbol;
            if (longGeoState != null)
                textBlockGeoStatus.Text = DateTime.Now + " " + longGeoState;
            if (longSityState != null)
                textBlockSity.Text = DateTime.Now + " " + longSityState;
            if (longRouteState != null)
                textBlockRoute.Text = DateTime.Now + " " + longRouteState;
            if (longTransportState != null)
                textBlockTransport.Text = DateTime.Now + " " + longTransportState;
            stackPanelStatus.Background = new SolidColorBrush(color);
        }

        private void UpdateLocationData()
        {
            if (Geoposition != null)
            {
                mapControl.Center = Geoposition.Coordinate.Point;
                mapControl.ZoomLevel = sliderZoom.Value;
                mapControl.LandmarksVisible = true;
            }
        }

        public MapIcon AddGeopoint(Geopoint geopoint, string name)
        {
            MapIcon mapIcon = new MapIcon();
            mapIcon.Location = geopoint;
            mapIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
            mapIcon.Title = name;
            mapIcon.ZIndex = 0;
            mapControl.MapElements.Add(mapIcon);
            mapIcon.Visible = true;
            return mapIcon;
        }

        public void RemoveMapIcon(MapIcon mapIcon)
        {
            mapControl.MapElements.Remove(mapIcon);
        }

        private async void ShowRouteOnMap(List<Geopoint> listGeopoint,Color color)
        {
            if (listGeopoint.Count > 1)
            {
                MapRouteFinderResult routeResult = 
                    await MapRouteFinder.GetDrivingRouteFromWaypointsAsync(listGeopoint);
                if (routeResult.Status == MapRouteFinderStatus.Success)
                {
                    // Use the route to initialize a MapRouteView.
                    MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                    viewOfRoute.RouteColor = color;
                    viewOfRoute.OutlineColor =Colors.Black;
                    
                    
                    // Add the new MapRouteView to the Routes collection
                    // of the MapControl.
                    mapControl.Routes.Add(viewOfRoute);

                    // Fit the MapControl to the route.
                    await mapControl.TrySetViewBoundsAsync(
                          routeResult.Route.BoundingBox,
                          null,
                          MapAnimationKind.None);
                    
                }
            }
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
                    Geolocator.DesiredAccuracy = PositionAccuracy.High;
                }
                else if (e.OriginalSource.Equals("Экономный"))
                {
                    Geolocator.DesiredAccuracy = PositionAccuracy.Default;
                }
            }
        }

        private void buttonStatus_Click(object sender, RoutedEventArgs e)
        {
            stackPanelStatus.Visibility = Visibility.Collapsed;
        }
        private void sliderFrequency_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Geolocator != null)
            {
                Geolocator.ReportInterval = (uint)sliderFrequency.Value;
            }
        }
        
        private void mapControl_Loaded(object sender, RoutedEventArgs e)
        {
            progressRingMap.IsActive = false;
        }

        private void buttonZoopPlus_Click(object sender, RoutedEventArgs e)
        {
            mapControl.ZoomLevel++;
            sliderZoom.Value++;
        }

        private void buttonZoomMinus_Click(object sender, RoutedEventArgs e)
        {
            mapControl.ZoomLevel--;
            sliderZoom.Value--;
        }

        private void sliderZoom_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            mapControl.ZoomLevel = sliderZoom.Value;
        }

        private void mapControl_ZoomLevelChanged(MapControl sender, object args)
        {
            sliderZoom.Value = mapControl.ZoomLevel;
        }
        
        private void buttonFind_Click(object sender, RoutedEventArgs e)
        {
        }

        private void buttonSettingSityRoute_Click(object sender, RoutedEventArgs e)
        {
            splitViewSettingSityRoute.IsPaneOpen = !splitViewSettingSityRoute.IsPaneOpen;
        }
    }
}
