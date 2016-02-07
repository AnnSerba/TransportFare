using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace TransportFare
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Map : Page
    {
        public Map()
        {
            this.InitializeComponent();
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

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            map.MapServiceToken = "mapToken";
            // получаем инструмент геолокации
            var geolocator = new Geolocator();
            //точность геолокации до 150 метров
            geolocator.DesiredAccuracyInMeters = 150;
            // получаем позицию
            var position = await geolocator.GetGeopositionAsync();
            // установка этой позиции на карте
            await map.TrySetViewAsync(position.Coordinate.Point, 19);
        }
    }
}
