using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Widget;
using Java.IO;
using Java.Lang;
using System;

namespace Accelerometer
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ISensorEventListener
    {
        static readonly object _syncLock = new object();
        SensorManager _sensorManager;
        TextView _sensorTextView;
        BatteryManager battery;


        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            GetDetailsResult();
        }

        public void OnSensorChanged(SensorEvent e)
        {
            GetDetailsResult();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource  
            SetContentView(Resource.Layout.activity_main);
            _sensorManager = (SensorManager)GetSystemService(SensorService);
            _sensorTextView = FindViewById<TextView>(Resource.Id.accelerometer_test);
            battery = (BatteryManager)GetSystemService(BatteryService);
        }

        protected override void OnResume()
        {
            base.OnResume();
            _sensorManager.RegisterListener(this,
                    _sensorManager.GetDefaultSensor(SensorType.AmbientTemperature),
                    SensorDelay.Normal);
            GetDetails();
        }

        private void GetDetails()
        {
            GetDetailsResult();
        }

        private void GetDetailsResult()
        {
            GetCPUDetails();
            GetCpuUsageStatistic();
            GetBatteryDetails();
        }

        private void GetBatteryDetails()
        {
            lock (_syncLock)
            {
                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                {
                    using (var batteryService = Application.Context.RegisterReceiver(null, filter))
                    {
                        //Intent receiver = context.RegisterReceiver(null, new IntentFilter(Intent.ActionBatteryChanged));
                        if (batteryService != null)
                        {
                            var tempExtra = batteryService.GetIntExtra(BatteryManager.ExtraTemperature, 0) / 10;
                            var level = batteryService.GetIntExtra(BatteryManager.ExtraLevel, 0);

                            _sensorTextView.Text += "Battery Temp: " + tempExtra.ToString() + "oC\nBattery level: " + level + "%";
                        }
                    }
                }
            }
        }



        protected override void OnPause()
        {
            base.OnPause();
            _sensorManager.UnregisterListener(this);
        }


        private void GetCPUDetails()
        {
            Java.Lang.Process p;

            try
            {
                p = Runtime.GetRuntime().Exec("cat sys/class/thermal/thermal_zone0/temp");
                p.WaitForAsync();

                BufferedReader reader = new BufferedReader(new InputStreamReader(p.InputStream));

                string line = reader.ReadLine();
                float temp = Float.ParseFloat(line) / 1000.0f;

                _sensorTextView.Text = "CPU Temp: " + temp.ToString() + "oC\n";

            }
            catch (System.Exception e)
            {
                //return 0.0f;
            }

        }

       


        private string ExecuteTop()
        {
            Java.Lang.Process p = null;
            BufferedReader reader;
            string returnString = null;
            try
            {
                p = Runtime.GetRuntime().Exec("top -n 1");
                reader = new BufferedReader(new InputStreamReader(p.InputStream));
                while (returnString == null || returnString.Equals(""))
                {
                    returnString = reader.ReadLine();
                }
            }
            catch (System.IO.IOException e)
            {
 
            }
            finally
            {
                try
                {
                    //reader.Close();
                    p.Destroy();
                }
                catch (System.IO.IOException e)
                {
                    //Log.e("executeTop",
                    //        "error in closing and destroying top process");
                    //e.printStackTrace();
                }
            }
            return returnString;
        }


        public void GetCpuUsageStatistic()
        {

            string tempString = ExecuteTop();

            tempString = tempString.Replace(",", "");
            tempString = tempString.Replace("User", "");
            tempString = tempString.Replace("System", "");
            tempString = tempString.Replace("IOW", "");
            tempString = tempString.Replace("IRQ", "");
            tempString = tempString.Replace("%", "");
            for (int i = 0; i < 10; i++)
            {
                tempString = tempString.Replace("  ", " ");
            }
            tempString = tempString.Trim();
            var  myString = tempString.Split(new string[] { " " }, StringSplitOptions.None);
            int[] cpuUsageAsInt = new int[myString.Length];
            for (int i = 0; i < myString.Length; i++)
            {
                myString[i] = myString[i].Trim();
                cpuUsageAsInt[i] = Integer.ParseInt(myString[i]);
            }
            _sensorTextView.Text += "CPU Usage - User: " + cpuUsageAsInt[0].ToString() + "%\n";
            _sensorTextView.Text += "CPU Usage - System: " + cpuUsageAsInt[1].ToString() + "%\n";
            _sensorTextView.Text += "CPU Usage - IOW: " + cpuUsageAsInt[2].ToString() + "%\n";
            _sensorTextView.Text += "CPU Usage - IRQ: " + cpuUsageAsInt[3].ToString() + "%\n";
        }


    }
}

