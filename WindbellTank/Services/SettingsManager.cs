using System;
using WindbellTank.Models;

namespace WindbellTank.Services
{
    // Ayar Göndərmə — istənilən yerdən çağır (UI, Console, vs.)
    public class SettingsManager
    {
        private readonly DeviceSettingsStore _store;

        public SettingsManager(DeviceSettingsStore store) => _store = store;

        // Bu metodu çağıran kimi cihaz növbəti heartbeat-də yeni ayarı çəkəcək
        public void SetTank1(int diameterMm, int volumeLiters, string oilName)
        {
            _store.UpdateTank(new TankSetting
            {
                TankNo       = "01",
                OilCode      = "1020",
                OilName      = oilName,
                DiameterMm   = diameterMm,
                VolumeLiters = volumeLiters,
                Enabled      = true
            });
            Console.WriteLine($"✅ Tank 01 ayarı növbəyə alındı. " +
                             $"Cihaz ~10 san içində çəkəcək.");
        }

        public void SetProbeAlarms(string tankNo,
            double highAlarm, double lowAlarm,
            double highWarning, double lowWarning)
        {
            _store.UpdateProbe(new ProbeSetting
            {
                TankNo       = tankNo,
                HighAlarmMm  = highAlarm,
                LowAlarmMm   = lowAlarm,
                HighWarningMm = highWarning,
                LowWarningMm  = lowWarning,
                WaterAlarmMm  = 50.0,
                WaterWarningMm = 30.0
            });
        }

        public void AddSampleSettings()
        {
            // Yağ məhsulu əlavə et
            _store.UpdateOilProduct(new OilProductSetting
            {
                OilCode       = "1020",
                OilName       = "92#",
                OilColor      = "green",
                ExpansionRate = "0.0012",   // benzin
                Temperature   = "20",
                WeightDensity = "0.725"
            });

            // Tank 1 sıxlıq ayarı
            _store.UpdateDensity(new DensitySetting
            {
                TankNo         = "01",
                InitDensity    = "0.725",
                SecondDensity  = "0.720",
                FixRate        = "1.0",
                HeightDiff     = "0",
                DensityFloatNo = "1"
            });

            // Qaz sensoru ayarı
            _store.UpdateGasSensor(new GasSensorSetting
            {
                SensorNo    = "01",
                Position    = "0",    // yanacaq adası
                PositionNum = "01",
                Enabled     = true
            });
        }
    }
}
