using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;

namespace IoTSensorSimulator
{
    public partial class Form1 : Form
    {
        //Connection string for device to cloud messaging
        private static readonly string connectionString_IoTHub = "";

        //Device Client
        static DeviceClient truckDeviceClient;

        //Random Generator
        static Random random = new Random();

        //truck sensor details
        const double truckTemperature_min = 20;
        const double truckTemperature_max = 40;
        static double truckTemperature = 20;
        const double truckHumidity_min = 50;
        const double truckHumidity_max = 90;
        static double truckHumidity = 50;
        const double truckLattitude_min = 80;
        const double truckLattitude_max = 120;
        static double truckLattitude = 80;
        const double truckLongitude_min = 80;
        const double truckLongitude_max = 120;
        static double truckLongitude = 80;

        static int duration = 5000;

        static string output_messages = string.Empty;

        static CancellationTokenSource cts;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            btnstart.Enabled = true;
            btnstop.Enabled = false;
            txtConnectionString.Text = connectionString_IoTHub;
            txtLogView.ReadOnly = true;
            txtduration.Text = duration.ToString();
        }

        private void btnstart_Click(object sender, EventArgs e)
        {
            try
            {
                cts = new CancellationTokenSource();
                truckDeviceClient = DeviceClient.CreateFromConnectionString(txtConnectionString.Text);

                SendMessagesToIoTHub(cts.Token);
                btnstart.Enabled = false;
                btnstop.Enabled = true;
                txtLogView.Text = txtLogView.Text + "IoT Simulator Started!!" + Environment.NewLine;
            }
            catch (Exception ex)
            {
                txtLogView.Text = txtLogView.Text + Environment.NewLine + ex.Message;
            }
         
        }
        private void btnstop_Click(object sender, EventArgs e)
        {
            cts.Cancel();
            btnstart.Enabled = true;
            btnstop.Enabled = false;
            txtLogView.Text = txtLogView.Text + "IoT Simulator Stopped!!" + Environment.NewLine;
        }

        private async void SendMessagesToIoTHub(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    truckLattitude = GenerateSensorReading(truckLattitude, truckLattitude_min, truckLattitude_max);
                    truckLongitude = GenerateSensorReading(truckLongitude, truckLongitude_min, truckLongitude_max);
                    truckTemperature = GenerateSensorReading(truckTemperature, truckTemperature_min, truckTemperature_max);
                    truckHumidity = GenerateSensorReading(truckHumidity, truckHumidity_min, truckHumidity_max);

                    var json = CreateJSON(truckTemperature, truckHumidity, truckLattitude, truckLongitude);
                    var message = CreateMessage(json);
                    await truckDeviceClient.SendEventAsync(message);
                    txtLogView.Text = txtLogView.Text +$"Sending message at {DateTime.Now} and Message : "+ Environment.NewLine+$"{json}";
                    txtLogView.Text = txtLogView.Text + Environment.NewLine;
                    //Console.WriteLine($"Sending message at {DateTime.Now} and Message : {json}"); 
                    await Task.Delay(Convert.ToInt32(txtduration.Text));
                }
            }
            catch (Exception ex)
            {
                txtLogView.Text = txtLogView.Text + Environment.NewLine + ex.Message;
                btnstart.Enabled = true;
                btnstop.Enabled = false;
            }
          
        }

        private double GenerateSensorReading(double currentValue, double min, double max)
        {
            double percentage = 5; // 5%

            // generate a new value based on the previous supplied value
            // The new value will be calculated to be within the threshold specified by the "percentage" variable from the original number.
            // The value will also always be within the the specified "min" and "max" values.
            double value = currentValue * (1 + ((percentage / 100) * (2 * random.NextDouble() - 1)));

            value = Math.Max(value, min);
            value = Math.Min(value, max);

            return value;
        }

        private string CreateJSON(double tempdata,double humiddata, double lattitudedata, double longitude)
        {
            var data = new
            {
                temp = tempdata,
                humid = humiddata,
                lat = lattitudedata,
                lngt = longitude
            };
            return JsonConvert.SerializeObject(data);
        }

        private Microsoft.Azure.Devices.Client.Message CreateMessage(string jsonObject)
        {
            var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(jsonObject));

            // MESSAGE CONTENT TYPE
            message.ContentType = "application/json";
            message.ContentEncoding = "UTF-8";

            return message;
        }
    }
}
