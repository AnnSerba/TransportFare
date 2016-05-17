using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TransportFare.Models;


namespace TransportFare
{
    /// <summary>
    /// Страница оплаты методом PayOnline
    /// </summary>
    public sealed partial class Pay : Page
    {
        private HashSet<Guid> consumedTransactionIds = new HashSet<Guid>();
        String Metka { get; set; }
        double Account { get; set; }
        string Login { get; set; }
        string Password { get; set; }
        public List<Product> AllProducts { get; set; }
        public Pay()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += Pay_BackRequested;
            splitViewAutification.IsPaneOpen = true;
            AllProducts = new List<Product>();
            appBarButtonShop.IsEnabled = false;
            appBarButtonAccept.IsEnabled = false;
            sliderCurrency.Maximum = Account;
            sliderCurrency.Value = (int)((sliderCurrency.Minimum + sliderCurrency.Maximum) / 2);
            LoadProducts();
        }
        
        public async void LoadProducts()
        {
            StorageFile proxyFile = await Package.Current.InstalledLocation.GetFileAsync("data\\products.xml");
            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
            var Listing = await CurrentAppSimulator.LoadListingInformationAsync();

            foreach (var i in Listing.ProductListings.Values)
            {
                AllProducts.Add(new Product(i.ProductId, i.Name,
                    double.Parse(i.FormattedPrice.Substring(4).Split('.')[0]+","
                    + i.FormattedPrice.Substring(4).Split('.')[1]),
                    i.FormattedPrice.Substring(0, 4)));
            }
            AllProducts.ForEach(i => { gridViewAllProducts.Items.Add(i); });
        }
        private void Pay_BackRequested(object sender, BackRequestedEventArgs e)
        {
            MainMenu mainMenu = Window.Current.Content as MainMenu;
            if (mainMenu == null)
            {
                mainMenu = new MainMenu();
                Window.Current.Content = mainMenu;
            }
        }

        private async void appBarButtonShop_Click(object sender, RoutedEventArgs e)
        {
            NotifyUser("Покупка продукта... "+ ((Product)gridViewAllProducts.SelectedValue).Name, 
                NotifyType.StatusMessage);
            try
            {
                Product productSelected= (Product)gridViewAllProducts.SelectedValue;
                PurchaseResults purchaseResults = 
                    await CurrentAppSimulator.RequestProductPurchaseAsync(
                        (productSelected.Id));
                
                switch (purchaseResults.Status)
                {
                    case ProductPurchaseStatus.Succeeded:
                        GrantFeatureLocally(purchaseResults.TransactionId);
                        FulfillPay(productSelected.Id,
                            purchaseResults.TransactionId);
                        Account += productSelected.Price;
                        textBlockAccount.Text ="На счету: "+ Account+" "+productSelected.Currency;
                        sliderCurrency.Maximum = Account;

                        sliderCurrency.Value = (int)((sliderCurrency.Minimum + sliderCurrency.Maximum) / 2);
                        break;
                    case ProductPurchaseStatus.NotFulfilled:
                        if (!IsLocallyFulfilled(purchaseResults.TransactionId))
                        {
                            GrantFeatureLocally(purchaseResults.TransactionId);
                        }
                        FulfillPay(productSelected.Id,   purchaseResults.TransactionId);
                        break;
                    case ProductPurchaseStatus.NotPurchased:
                        NotifyUser("Покупка"+ productSelected.Name + 
                            " не была выполнена", NotifyType.StatusMessage);
                        break;
                    case ProductPurchaseStatus.AlreadyPurchased:
                        NotifyUser("Покупка" + productSelected.Name + 
                            " не была выполнена так как эта покупка уже была выполнена", NotifyType.StatusMessage);
                        break;
                }
            }
            catch (Exception exception)
            {
                NotifyUser("Покупка не была выполнена. Ошибка:"+ exception.Message, NotifyType.ErrorMessage);
            }
        }

        private async void FulfillPay(string productId,Guid transactionId)
        {
            try
            {
                FulfillmentResult result = await CurrentAppSimulator.ReportConsumableFulfillmentAsync(
                    productId, transactionId);
                
                switch (result)
                {
                    case FulfillmentResult.Succeeded:
                        NotifyUser("Покупка совершена.", NotifyType.StatusMessage);
                        break;
                    case FulfillmentResult.NothingToFulfill:
                        NotifyUser("Покупка уже была выполнена.", NotifyType.StatusMessage);
                        break;
                    case FulfillmentResult.PurchasePending:
                        
                        NotifyUser("Покупка ещё не очищена. Она может быть отменена из за збоев поставщика и проверок рисков", NotifyType.StatusMessage);
                        break;
                    case FulfillmentResult.PurchaseReverted:
                        NotifyUser("Запрос на покупку был отменён.", NotifyType.StatusMessage);
                        // Since the user's purchase was revoked, they got their money back.
                        // You may want to revoke the user's access to the consumable content that was granted.
                        break;
                    case FulfillmentResult.ServerError:
                        NotifyUser("Ошибка запроса", NotifyType.StatusMessage);
                        break;
                }
            }
            catch (Exception)
            {
                NotifyUser("Возникла ошибка выполнения", NotifyType.ErrorMessage);
            }
        }

        private void GrantFeatureLocally(Guid transactionId)
        {
            consumedTransactionIds.Add(transactionId);
            
        }

        private bool IsLocallyFulfilled(Guid transactionId)
        {
            return consumedTransactionIds.Contains(transactionId);
        }
        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };
        public void NotifyUser(string strMessage, NotifyType type)
        {
            if (frameMessage.Visibility == Visibility.Collapsed)
            {
                frameMessage.Visibility = Visibility.Visible;
            }
            switch (type)
            {
                case NotifyType.StatusMessage:
                    frameMessage.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    frameMessage.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }
            textBlockMessage.Text = strMessage;
        }
        

        private void buttonAccert_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonCansel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonFlyoutAutificationClose_Click(object sender, RoutedEventArgs e)
        {
            MainMenu mainMenu = Window.Current.Content as MainMenu;
            if (mainMenu == null)
            {
                mainMenu = new MainMenu();
                Window.Current.Content = mainMenu;
            }
        }

        private void sliderCurrency_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            textBlockSliderValueCurrency.Text = sliderCurrency.Value.ToString();
        }

        private void buttonFlyoutAutificationOk_Click(object sender, RoutedEventArgs e)
        {
            splitViewAutification.IsPaneOpen = false;
            appBarButtonAccept.IsEnabled = true;
            gridContent.Visibility = Visibility.Visible;
            
        }

        private void gridViewAllProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridViewAllProducts.SelectedItems.Count == 0)
            {
                appBarButtonShop.IsEnabled = false;
            }
            else
            {
                appBarButtonShop.IsEnabled = true;
            }
        }
    }
}
