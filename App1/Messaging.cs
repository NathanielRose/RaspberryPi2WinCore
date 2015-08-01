using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;



namespace App1
{
   //Troll Bridge Registered Request
   /*Customer Info
        Customer ID = 22  | Company = NateRose Trolllolol | Address1 = 555 California St | Address 2 = Floor 3
        City = San Francisco | State = California | ZipCode = 94123 | Country = USA
    Site Info
        Customer ID = 22 | SiteID = 15 | SiteName = Satyas Place |Address1 = 123 Market St | 
        Address 2 = 200 Suite | City = Seattle | State = Washington | Zipcode = 10101 | Country = USA
    Event Hub Policy Table
        Customer ID = 22 | SiteID = 15 | Policy ID = 20 | PolicyIdentifier = Sender | EventHubConnectionStr = *****
        Manage = 0 | Send = 1 | Receive = 0|
    Collector
        Customer ID = 22 | SiteID = 15 (16 for Manage)` | CollectorID = 1 | MinuteToLate = 5 | ResendAfterXMin = 1440 | TimeNotified = NULL
        Active = 1 | MacAddress1 = 00155D274N00 // 00155D27N01 for MAnage | MacAddress2 = "" | LastUpdateTimestamp = CURRENT_TIMESTAMP
    Azure Event Hub Policy Map
        Customer ID = 22 | SiteID = 15 | CollectorID = 1| PolicyID = 20 & 21

        08:57



    */
    class Messaging
    {
        
        private string tbCustID = "22";
        private string tbSiteID = "15";
        private string tbMacAddress = "00155D274N00";
        public string errorMessage;


        private void GetTokens()
        {
            string machineIdentifier = (tbCustID + "_" + tbSiteID + "_" + tbMacAddress);
            HttpClient messageClient = new HttpClient();
            var endpointUri = string.Format("http://trollbridge-staging.azurewebsites.net/KeyMaster.svc/soap,");

            var sasToken = messageClient.GetStringAsync(endpointUri);

            if(sasToken == null)
            {
                errorMessage = "Sorry, no data returned";
                return;                
            }

           
        }
       
        public Task<HttpResponseMessage> PostMessageAsync(string sensorData)
        {
            var sas = "SharedAccessSignature sr=Your TOKEN HERE";

            var serviceNamespace = "trollbridgeNate";
            var hubName = "tbnatehub";
            var url = string.Format("{0}/publishers/{1}/messages", hubName, sensorData);

            // Create a Client
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(string.Format("https://{0}.servicebus.windows.net/", serviceNamespace))
            };

            var payLoad = JsonConvert.SerializeObject(sensorData);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", sas);

            var content = new StringContent(payLoad, Encoding.UTF8, "application/json");

            content.Headers.Add("ContentType", sensorData); //Needs TO BE CHANGED

            return httpClient.PostAsync(url, content);
          
        }
    }
}
