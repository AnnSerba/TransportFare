﻿
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace TransportFare
{
    public class Load
    {
        public static List<string> Sities()
        {
            Task<List<string>> task = new Task<List<string>>(() =>
            {
                List<string> list = new List<string>();
                list.Add("Симферополь");
                list.Add("Севастополь");
                list.Add("Керч");
                return list;
            });
            task.Start();
            return task.Result;
        }
        public static List<string> Routes(string sity)
        {
            Task<List<string>> task = new Task<List<string>>(() =>
            {
                List<string> list = new List<string>();
                list.Add("9");
                return list;
            });
            task.Start();
            return task.Result;
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