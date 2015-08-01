using System;
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
using Windows.Devices.Gpio;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Runtime;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;





// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409


namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer timer;
        private const int SPI_CHIP_SELECT_LINE = 0; //Line 0 maps to physical pin number 24 on the Rpi2
        private const string SPI_CONTROLLER_NAME = "SPI0"; //For Raspberry Pi 2 use SPI0
        private SpiDevice SpiDisplay;
        int res;
        string tbCustID = "22";
        string tbSiteID = "15";
        string tbMacAddress = "00155D274N00";
        JObject returnJson = new JObject();
        bool buttonClicked = false;


        const string tbURL = "http://trollbridge.azurewebsites.net/KeyMaster.svc/json/GetTokensForMeIoT/22_15_00155D274N00";
        const string testUrl = "http://104.43.142.33//KeyMaster.svc/json/GetTokensForMeIoT/22_15_00155D274N00";

        HttpClient MessageClient = new HttpClient();
        EventHubToken tokenCollector = new EventHubToken();

        
        
        

        byte[] readBuffer = new byte[3]; //This is defined to hold the output data
        byte[] writeBuffer = new byte[3] { 0x06, 0x00, 0x00 }; //00000110 00; It is SPI port serial input pin and is used to load channel configuration data into the device

        public MainPage()
        {
            this.InitializeComponent();

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(500);
            this.timer.Tick += Timer_Tick;
            this.timer.Start();

            //MessageClient.GetStringAsync(tbURL);

        }

        private void Timer_Tick(object sender, object e)
        {
            //Do SOmething
            DisplayTextBoxContents();
            if (buttonClicked)
            {

            }

                
        }

        public async Task<string> GetDataAsync()
        {
            string sasToken = null;

            try
            {
                sasToken = await MessageClient.GetStringAsync(tbURL).ConfigureAwait(false);
                //var temp = await MessageClient.GetAsync(new Uri("http://bing.com/"));
                this.Dispatcher.RunAsync( Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
               {
                  // this.StartMessage.Text = sasToken;

               });
                //this.StartMessage.Text = sasToken;


            }
            catch ( Exception e )
            {
                Debug.Assert(false);
            }
          //  var thisToken = await EventHubToken.GetEventHubTokens(sasToken);

            return sasToken;      
              
        }
       public async Task<HttpResponseMessage> PostDataAsync(EventHubToken sasTokens, string currentTemp)
       {
            piData tempy = new piData();
            string url = "https://trollbridgenate.servicebus.windows.net/tbnatehub/messages";
            tempy.temperature = "100";
            var payload = JsonConvert.SerializeObject(tempy);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            MessageClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", sasTokens.SasToken);
            return await MessageClient.PostAsync(url, content);
          
            

       }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            this.StartMessage.Text = "Program Initiated";
            StartButton.Content = "Stop Sending Data";
            buttonClicked = !buttonClicked;
            //var sas = GetDataAsync().Result;
            //this.StartMessage.Text = sas;
            var tokens = EventHubToken.GetEventHubTokens(tbURL).Result;
            this.StartMessage.Text = tokens[0].Uri;
           var errorResponse = PostDataAsync(tokens[0], "test");

            errorResponse.Wait();
           // Debug.WriteLine(errorResponse.Result.StatusCode);
           // Debugger.Break();

        }

        public void DisplayTextBoxContents()
        {
            //SpiDisplay.TransferFullDuplex(writeBuffer, readBuffer);
            //res = convertToInt(readBuffer);
            //var sensorData = StartMessage.Text = res.ToString();//Take this and use for sending string as a message
           

            //var payLoad = JsonConvert.SerializeObject(sensorData);


            //var content = new StringContent(sensorData, Encoding.UTF8, "application/json");

            //MessageClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "SharedAccessSignature sr=sb%3a%2f%2ftrollbridgenate.servicebus.windows.net%2ftbnatehub%2fpublishers%2fsite15&sig=C%2f6lH%2bezMAg02dmv125Ddtn53ltKV%2b9U7erfBwMW%2fPU%3d&se=1434572626&skn=sender");

            //content.Headers.Add("ContentType", "application/xml"); //Needs TO BE CHANGED
            //var url = string.Format("tbnatehub/publishers/sender/messages");

            //MessageClient.PostAsync(url, content);

        }

        public int convertToInt(byte[] data)
        {
            //The code below is currated for the MCP 3008 ATD Chip
            int result = data[1] & 0x0F;
            result <<= 8;
            result += data[2];
            return result;
        }

        private async void InitSPI()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 500000;//10 million
                settings.Mode = SpiMode.Mode0; // Mode3

                string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
                var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
                SpiDisplay = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);
            }

            //If initialization fails, display the exception and stop running
            catch(Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }

        }

        public class EventHubToken
        {
            public DateTime ExpirationDate { get; set; }
            public string EventHubName { get; set; }
            public string SasToken { get; set; }
            public string Uri { get; set; }

            public static async Task<List<EventHubToken>> GetEventHubTokens(string url)
            {
                var tokens = new List<EventHubToken>();

                var httpClient = new HttpClient();
                var json = await httpClient.GetStringAsync(url).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(json))
                {
                    var o = JObject.Parse(json);
                    var a = (JArray)o["AzureEventHubs"];

                    tokens = new List<EventHubToken>();

                    for (int i = 0; i < a.Count; i++)
                    {
                        var eventHubToken = new EventHubToken();

                        eventHubToken.ExpirationDate = (DateTime)a[i]["DateExpires"];
                        eventHubToken.EventHubName = (string)a[i]["Name"];
                        eventHubToken.SasToken = (string)a[i]["SasToken"];
                        eventHubToken.Uri = (string)a[i]["Uri"];

                        tokens.Add(eventHubToken);
                    }
                }

                return tokens;
            }
        }

        public class piData
        {
            public string temperature { get; set; }
        }

    }
}

  
    

