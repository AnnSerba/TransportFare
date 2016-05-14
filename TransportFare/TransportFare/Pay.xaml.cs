using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Store;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace TransportFare
{
    /// <summary>
    /// Страница оплаты методом PayOnline
    /// </summary>
    public sealed partial class Pay : Page
    {
        private HashSet<Guid> consumedTransactionIds = new HashSet<Guid>();
        String Metka { get; set; }
        string Login { get; set; }
        string Password { get; set; }
        public Pay()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += Pay_BackRequested;
            
            LoadProducts();
        }
        public async void LoadProducts()
        {
            ListingInformation listing = await CurrentAppSimulator.LoadListingInformationAsync();
            var AllProducts = listing.ProductListings;

            foreach (var i in AllProducts)
            {
                gridViewAllProducts.Items.Add(i.Value);
            }
            var unfulfilledProducts = await CurrentAppSimulator.GetUnfulfilledConsumablesAsync();
            for (int i = 0; i < unfulfilledProducts.Count; i++)
            {
                gridViewNoUsedProducts.Items.Add(AllProducts[unfulfilledProducts[i].ProductId]);
            }
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

        private async void appBarButtonAccept_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            NotifyUser("Покупка продукта...", NotifyType.StatusMessage);
            try
            {
                PurchaseResults purchaseResults = await CurrentAppSimulator.RequestProductPurchaseAsync("product1");
                
                switch (purchaseResults.Status)
                {
                    case ProductPurchaseStatus.Succeeded:
                        GrantFeatureLocally(purchaseResults.TransactionId);
                        FulfillPay(purchaseResults.TransactionId);
                        
                        break;
                    case ProductPurchaseStatus.NotFulfilled:
                        if (!IsLocallyFulfilled(purchaseResults.TransactionId))
                        {
                            GrantFeatureLocally(purchaseResults.TransactionId);
                        }
                        FulfillPay(purchaseResults.TransactionId);
                        break;
                    case ProductPurchaseStatus.NotPurchased:
                        NotifyUser("Покупка не была выполнена", NotifyType.StatusMessage);
                        break;
                }
            }
            catch (Exception)
            {
                NotifyUser("Покупка не была выполнена", NotifyType.ErrorMessage);
            }
        }

        private async void FulfillPay(Guid transactionId)
        {
            try
            {
                FulfillmentResult result = await CurrentAppSimulator.ReportConsumableFulfillmentAsync("product1", transactionId);
                
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

        private void toggleSwitchTag_Toggled(object sender, RoutedEventArgs e)
        {
            if (toggleSwitchTag.IsOn == true)
            {
                appBarButtonAccept.IsEnabled = true;
            }
            else
            {
                appBarButtonAccept.IsEnabled = false;
            }
        }

        private void buttonAccert_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonCansel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonFlyoutClose_Click(object sender, RoutedEventArgs e)
        {
            flyoutAutification.Hide();
        }
    }
}
