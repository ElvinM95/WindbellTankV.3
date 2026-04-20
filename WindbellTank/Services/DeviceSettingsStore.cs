using System;
using System.Collections.Generic;
using System.Linq;
using WindbellTank.Models;

namespace WindbellTank.Services
{
    public class DeviceSettingsStore
    {
        // Versiyalar — artırıldıqda cihaz yeni ayarları çəkir
        public int TankVer    { get; private set; } = 1;
        public int ProbeVer   { get; private set; } = 1;
        public int SensorVer  { get; private set; } = 1;
        public int TableVer   { get; private set; } = 1;
        public int DensityVer { get; private set; } = 1;
        public int OilProductVer { get; private set; } = 1;
        public int GasVer { get; private set; } = 1;

        // Ayarlar
        public List<TankSetting>   Tanks   { get; } = new();
        public List<ProbeSetting>  Probes  { get; } = new();
        public List<SensorSetting> Sensors { get; } = new();
        public List<TankTableEntry> TankTable { get; } = new();
        public List<OilProductSetting> OilProducts { get; } = new();
        public List<DensitySetting> Densities { get; } = new();
        public List<GasSensorSetting> GasSensors { get; } = new();

        // Tank ayarını yenilə — versiyanı artır ki cihaz çəksin
        public void UpdateTank(TankSetting setting)
        {
            var existing = Tanks.FirstOrDefault(t => t.TankNo == setting.TankNo);
            if (existing != null) Tanks.Remove(existing);
            setting.Version = (TankVer + 1).ToString();
            Tanks.Add(setting);
            TankVer++;
            Console.WriteLine($"[STORE] Tank {setting.TankNo} yeniləndi. Yeni ver: {TankVer}");
        }

        public void UpdateProbe(ProbeSetting setting)
        {
            var existing = Probes.FirstOrDefault(p => p.TankNo == setting.TankNo);
            if (existing != null) Probes.Remove(existing);
            setting.Version = (ProbeVer + 1).ToString();
            Probes.Add(setting);
            ProbeVer++;
        }

        public void UpdateSensor(SensorSetting setting)
        {
            var existing = Sensors.FirstOrDefault(s => s.SensorNo == setting.SensorNo);
            if (existing != null) Sensors.Remove(existing);
            Sensors.Add(setting);
            SensorVer++;
        }

        public void UpdateTankTable(string tankNo, List<TankTableEntry> entries)
        {
            TankTable.RemoveAll(t => t.TankNo == tankNo);
            TankTable.AddRange(entries);
            TableVer++;
        }

        public void UpdateOilProduct(OilProductSetting setting)
        {
            var existing = OilProducts.FirstOrDefault(o => o.OilCode == setting.OilCode);
            if (existing != null) OilProducts.Remove(existing);
            OilProducts.Add(setting);
            OilProductVer++;
            Console.WriteLine($"[STORE] Yağ məhsulu '{setting.OilName}' yeniləndi. Ver: {OilProductVer}");
        }

        public void UpdateDensity(DensitySetting setting)
        {
            var existing = Densities.FirstOrDefault(d => d.TankNo == setting.TankNo);
            if (existing != null) Densities.Remove(existing);
            setting.Version = (DensityVer + 1).ToString();
            Densities.Add(setting);
            DensityVer++;
            Console.WriteLine($"[STORE] Tank {setting.TankNo} sıxlıq ayarı yeniləndi. Ver: {DensityVer}");
        }

        public void UpdateGasSensor(GasSensorSetting setting)
        {
            var existing = GasSensors.FirstOrDefault(g => g.SensorNo == setting.SensorNo);
            if (existing != null) GasSensors.Remove(existing);
            GasSensors.Add(setting);
            GasVer++;
            Console.WriteLine($"[STORE] Qaz sensoru {setting.SensorNo} yeniləndi. Ver: {GasVer}");
        }
    }
}
