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
using Microsoft.Band;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private App viewModel;
        IBandClient bandClient;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                // Get the list of Microsoft Bands paired to the phone.
                IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands.Length < 1)
                {
                    textBox.Text = "This sample app requires a Microsoft Band paired to your device. Also make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                    return;
                }

                bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]);
                
                    //while (true)
                    //{
                        int samplesReceived = 0; // the number of Accelerometer samples received

                    // Subscribe to Accelerometer data.
                    //bandClient.SensorManager.Accelerometer.ReadingChanged += (s, args) => { samplesReceived++; };
                    bandClient.SensorManager.Accelerometer.ReadingChanged += async (s, args) => {
                        await textBox.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            textBox.Text = "Accelerator = " + args.SensorReading.AccelerationX;

                        });
                    };
                    await bandClient.SensorManager.Accelerometer.StartReadingsAsync();

                    //    // Receive Accelerometer data for a while, then stop the subscription.
                    //    await Task.Delay(TimeSpan.FromSeconds(5));
                    //    await bandClient.SensorManager.Accelerometer.StopReadingsAsync();
                    //    string message = "Accelerator =" + samplesReceived;
                    //    textBox.Text = message;
                    //}
                
            }
            catch (Exception ex)
            {
                textBox.Text = ex.ToString();
            }
        }
    }
}
