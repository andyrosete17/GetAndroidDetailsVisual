using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Hardware;
using Android.Runtime;
using Android.Content;
using System.Runtime.Remoting.Contexts;
using Java.Lang;

namespace Accelerometer
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ISensorEventListener
    {
        static readonly object _syncLock = new object();
        SensorManager _sensorManager;
        TextView _sensorTextView;
        BatteryManager battery;
        ProcessBuilder processBuilder;


        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
            throw new System.NotImplementedException();
        }

        public void OnSensorChanged(SensorEvent e)
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

                            _sensorTextView.Text = "Temp: " + tempExtra.ToString() + "oC\nBattery level: " + level + "%";
                        }
                    }
                }
            }
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
                    _sensorManager.GetDefaultSensor(SensorType.Accelerometer),
                    SensorDelay.Fastest);
        }

        protected override void OnPause()
        {
            base.OnPause();
            _sensorManager.UnregisterListener(this);
        }
    }
}

