
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Notifications;
using Windows.Foundation;
using Windows.Web.Http;

namespace TransportFare
{
    public class Load
    {
        static Uri url = new Uri("http://www.eway.in.ua/");
        static HttpClient httpClient = new HttpClient();
        public static List<string> Sities()
        {
            
            var result= Task.Run(async () => 
            {
                return await httpClient.GetAsync(url);
            }).Result.Content;
            List<string> list = new List<string>();
            list.Add("Симферополь");
            list.Add("Севастополь");
            list.Add("Керч");
            return list;
        }
        public static List<string> Routes(string sity)
        {
            List<string> list = new List<string>();
            list.Add("9");
            list.Add("94");
            return list;
        }
        public static List<Geopoint> Transport(string route)
        {
            var list = new List<Geopoint>();
            list.Add(new Geopoint(new BasicGeoposition() { Latitude = 44.539125, Longitude = 33.543051 }));
            list.Add(new Geopoint(new BasicGeoposition() { Latitude = 44.529095, Longitude = 33.554922 }));
            list.Add(new Geopoint(new BasicGeoposition() { Latitude = 44.501472, Longitude = 33.600006 }));
            list.Add(new Geopoint(new BasicGeoposition() { Latitude = 44.51011, Longitude = 33.620895 }));
            return list;
        }
        public static List<Geopoint> Way(string route)
        {
            var list = new List<Geopoint>();
            if (route == "9")
            {
                list.Add(new Geopoint(new BasicGeoposition() { Latitude = 44.539125, Longitude = 33.543051 }));
                list.Add(new Geopoint(new BasicGeoposition() { Latitude = 44.529095, Longitude = 33.554922 }));
                list.Add(new Geopoint(new BasicGeoposition() { Latitude = 44.501472, Longitude = 33.600006 }));
                list.Add(new Geopoint(new BasicGeoposition() { Latitude = 44.51011, Longitude = 33.620895 }));
            }
            else
            {
                list.Add(new Geopoint(new BasicGeoposition() { Latitude = 44.513453, Longitude = 33.50234345 }));
                list.Add(new Geopoint(new BasicGeoposition() { Latitude = 44.5534234, Longitude = 33.552342345 }));
                list.Add(new Geopoint(new BasicGeoposition() { Latitude = 44.5834234, Longitude = 33.582342345 }));
            }
            return list;
        }
        public static double GetAccount(string login, string password)
        {
            return 100;
        }
        public static bool ChangeAccount(string login, string password,double account)
        {
            return true;
        }
        public static bool Autification(string login, string password)
        {
            return true;
        }
        public static bool Registration(string login,string email,string number, string password)
        {
            return true;
        }
        public static void DoToast(int numEventsOfInterest, string eventName)
        {
            // pop a toast for each geofence event
            ToastNotifier ToastNotifier = ToastNotificationManager.CreateToastNotifier();

            // Create a two line toast and add audio reminder

            // Here the xml that will be passed to the 
            // ToastNotification for the toast is retrieved
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            // Set both lines of text
            Windows.Data.Xml.Dom.XmlNodeList toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0).AppendChild(toastXml.CreateTextNode("Geolocation Sample"));

            if (1 == numEventsOfInterest)
            {
                toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(eventName));
            }
            else
            {
                string secondLine = "There are " + numEventsOfInterest + " new geofence events";
                toastNodeList.Item(1).AppendChild(toastXml.CreateTextNode(secondLine));
            }

            // now create a xml node for the audio source
            Windows.Data.Xml.Dom.IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            Windows.Data.Xml.Dom.XmlElement audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotifier.Show(toast);
        }
    }
}
