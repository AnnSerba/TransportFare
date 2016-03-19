﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TransportFare
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainMenu : Page
    {
        public MainMenu()
        {
            this.InitializeComponent();
        }

        private void ButtonToMap_Click(object sender, RoutedEventArgs e)
        {
            Map map = Window.Current.Content as Map;
            // если не создан
            if (map == null)
            {
                // создание 
                map = new Map();

                // установка для текущего окна
                Window.Current.Content = map;
            }
        }

        private void ButtonToPay_Click(object sender, RoutedEventArgs e)
        {
            Pay map = Window.Current.Content as Pay;
            // если не создан
            if (map == null)
            {
                // создание 
                map = new Pay();

                // установка для текущего окна
                Window.Current.Content = map;
            }
        }
    }
}
