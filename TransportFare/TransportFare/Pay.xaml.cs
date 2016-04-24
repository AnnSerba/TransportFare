using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Store;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
        public Pay()
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += Pay_BackRequested;
            this.InitializeComponent();
            sliderSummPay.Minimum = 1;
            sliderSummPay.Maximum = 1000;
           
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
            NotifyUser("Buying Product 1...", NotifyType.StatusMessage);
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
                        // The purchase failed because we haven't confirmed fulfillment of a previous purchase.
                        // Fulfill it now.
                        if (!IsLocallyFulfilled(purchaseResults.TransactionId))
                        {
                            GrantFeatureLocally(purchaseResults.TransactionId);
                        }
                        FulfillPay(purchaseResults.TransactionId);
                        break;
                    case ProductPurchaseStatus.NotPurchased:
                        NotifyUser("Product 1 was not purchased.", NotifyType.StatusMessage);
                        break;
                }
            }
            catch (Exception)
            {
                NotifyUser("Unable to buy Product 1.", NotifyType.ErrorMessage);
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
                        NotifyUser("You bought and fulfilled product 1.", NotifyType.StatusMessage);
                        break;
                    case FulfillmentResult.NothingToFulfill:
                        NotifyUser("There is no purchased product 1 to fulfill.", NotifyType.StatusMessage);
                        break;
                    case FulfillmentResult.PurchasePending:
                        NotifyUser("You bought product 1. The purchase is pending so we cannot fulfill the product.", NotifyType.StatusMessage);
                        break;
                    case FulfillmentResult.PurchaseReverted:
                        NotifyUser("You bought product 1, but your purchase has been reverted.", NotifyType.StatusMessage);
                        // Since the user's purchase was revoked, they got their money back.
                        // You may want to revoke the user's access to the consumable content that was granted.
                        break;
                    case FulfillmentResult.ServerError:
                        NotifyUser("You bought product 1. There was an error when fulfilling.", NotifyType.StatusMessage);
                        break;
                }
            }
            catch (Exception)
            {
                NotifyUser("You bought Product 1. There was an error when fulfilling.", NotifyType.ErrorMessage);
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

        private void toggleSwitchTag_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }
        
    }
}
