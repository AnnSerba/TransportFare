using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using TransportFare.Models;
using Windows.UI.Xaml.Controls.Primitives;
using NdefLibrary;
using NdefLibraryUwp;
using Windows.Networking.Proximity;

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
        private ProximityDevice Device { get; set; }
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
            listBoxMethodBuy.Items.Add("Microsoft");
            LoadProducts();
            Device = ProximityDevice.GetDefault();
        }

        #region Back
        public void Back()
        {
            MainMenu mainMenu = Window.Current.Content as MainMenu;
            if (mainMenu == null)
            {
                mainMenu = new MainMenu();
                Window.Current.Content = mainMenu;
            }
        }
        private void Pay_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Back();   
        }

        #endregion
        #region Notify
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
        #endregion
        #region Registration and Autification
        private void buttonFlyoutOkAutification_Click(object sender, RoutedEventArgs e)
        {
            if (Load.Login(textBoxLoginAutification.Text, passwordBoxPasswordAutification.Password))
            {
                Login = textBoxLoginAutification.Text;
                Password = passwordBoxPasswordAutification.Password;
                splitViewAutification.IsPaneOpen = false;
                SetUIAccount(Load.Account(Login, Password), "руб.");
                gridContent.Visibility = Visibility.Visible;
            }
            else
            {
                NotifyUser("Ошибка авторизации", NotifyType.ErrorMessage);
            }
        }
        private void buttonFlyoutCloseAutification_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }
        private void buttonFlyoutOkRegistration_Click(object sender, RoutedEventArgs e)
        {
            if (passwordBoxPasswordRegistration1.Password!= passwordBoxPasswordRegistration2.Password)
            {
                NotifyUser("Пароли не совпадают", NotifyType.ErrorMessage);
            }
            else
            if (Load.Registration(textBoxLoginRegistration.Text, textBoxEmail.Text, 
                textBoxPhoneNumber.Text, passwordBoxPasswordRegistration1.Password))
            {
                Login = textBoxLoginRegistration.Text;
                Password = passwordBoxPasswordRegistration1.Password;
                splitViewAutification.IsPaneOpen = false;
                Account = Load.Account(Login,Password);
                SetUIAccount(Load.Account(Login, Password), "руб.");
                gridContent.Visibility = Visibility.Visible;
            }
            else
            {
                NotifyUser("Ошибка авторизации", NotifyType.ErrorMessage);
            }

        }
        private void buttonFlyoutCloseRegistration_Click(object sender, RoutedEventArgs e)
        {
            Back();
        }
        #endregion
        #region Buy
        public async void LoadProducts()
        {
            StorageFile proxyFile = await Package.Current.InstalledLocation.GetFileAsync("data\\products.xml");
            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
            var Listing = await CurrentAppSimulator.LoadListingInformationAsync();

            foreach (var i in Listing.ProductListings.Values)
            {
                AllProducts.Add(new Product(i.ProductId, i.Name,
                    double.Parse(i.FormattedPrice.Substring(4).Split('.')[0] + ","
                    + i.FormattedPrice.Substring(4).Split('.')[1]),
                    i.FormattedPrice.Substring(0, 4)));
            }
            AllProducts.ForEach(i => { gridViewAllProducts.Items.Add(i); });
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
        public void SetUIAccount(double account,string currency)
        {
            Account += account;
            if (Account != 0)
            {
                appBarButtonAccept.IsEnabled = true;
            }
            textBlockAccount.Text = "На счету: " + Account + " " + currency;
            sliderCurrency.Maximum = Account;
            sliderCurrency.Value = (int)((sliderCurrency.Minimum + sliderCurrency.Maximum) / 2);
        }
        public async void MicrosoftBuy()
        {
            NotifyUser("Покупка продукта... " + ((Product)gridViewAllProducts.SelectedValue).Name,
                NotifyType.StatusMessage);
            try
            {
                Product productSelected = (Product)gridViewAllProducts.SelectedValue;
                PurchaseResults purchaseResults =
                    await CurrentAppSimulator.RequestProductPurchaseAsync(
                        (productSelected.Id));

                switch (purchaseResults.Status)
                {
                    case ProductPurchaseStatus.Succeeded:
                        GrantFeatureLocally(purchaseResults.TransactionId);
                        FulfillPay(productSelected.Id,
                            purchaseResults.TransactionId);
                        SetUIAccount(productSelected.Price, productSelected.Currency);
                        break;
                    case ProductPurchaseStatus.NotFulfilled:
                        if (!IsLocallyFulfilled(purchaseResults.TransactionId))
                        {
                            GrantFeatureLocally(purchaseResults.TransactionId);
                        }
                        FulfillPay(productSelected.Id, purchaseResults.TransactionId);
                        break;
                    case ProductPurchaseStatus.NotPurchased:
                        NotifyUser("Покупка" + productSelected.Name +
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
                NotifyUser("Покупка не была выполнена. Ошибка:" + exception.Message, NotifyType.ErrorMessage);
            }
        }
        private async void FulfillPay(string productId, Guid transactionId)
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
        private void listBoxMethodBuy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (listBoxMethodBuy.SelectedValue!=null&& 
                listBoxMethodBuy.SelectedValue.ToString() == "Microsoft")
            {
                MicrosoftBuy();
            }
            listBoxMethodBuy.SelectedIndex = -1;
            flyoutMethodBuy.Hide();
        }
        private void GrantFeatureLocally(Guid transactionId)
        {
            consumedTransactionIds.Add(transactionId);

        }
        private bool IsLocallyFulfilled(Guid transactionId)
        {
            return consumedTransactionIds.Contains(transactionId);
        }
        private void sliderCurrency_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            textBlockSliderValueCurrency.Text = sliderCurrency.Value.ToString();
        }
        #endregion
    }
}
