using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.Json;
using WindbellTank.Services;

namespace WindbellTank.Controllers
{
    [ApiController]
    [Route("deviceAPI")]
    public class DeviceApiController : ControllerBase
    {
        private readonly DeviceSettingsStore _store;

        public DeviceApiController(DeviceSettingsStore store)
            => _store = store;

        // ── HEARTBEAT — Cihaz hər 10 saniyədə çağırır ─────────────────
        // Cihaz versiyaları müqayisə edir, fərqli olanları çəkir
        [HttpPost("getSysVersionData")]
        public IActionResult GetSysVersionData([FromBody] JsonElement body)
        {
            string iotDevId = GetIotDevId(body);
            Console.WriteLine($"[HEARTBEAT] Cihaz: {iotDevId}");

            return Ok(new
            {
                code        = 200,
                result      = 0,
                commandType = 6,
                data        = new
                {
                    serverTime         = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    softVer            = 1,
                    sensorVer          = _store.SensorVer,
                    gasVer             = _store.GasVer,
                    deviceSettingType  = 0, // 0=Remote — cihaz uzaqdan idarə olunur
                    tankList           = BuildTankVersionList()
                },
                msg = (string)null
            });
        }

        private object BuildTankVersionList()
        {
            // Heç tank yoxdursa default göndər
            if (!_store.Tanks.Any())
            {
                return new[] { new
                {
                    tankNo       = "01",
                    tankVer      = _store.TankVer,
                    tankTableVer = _store.TableVer,
                    probeVer     = _store.ProbeVer,
                    densityVer   = _store.DensityVer
                }};
            }

            return _store.Tanks.Select(t => new
            {
                tankNo       = t.TankNo,
                tankVer      = int.Parse(t.Version),
                tankTableVer = _store.TableVer,
                probeVer     = _store.ProbeVer,
                densityVer   = _store.DensityVer
            });
        }

        // ── TANK AYARLARI — Cihaz versiya fərqində çağırır ────────────
        [HttpPost("getTankData")]
        public IActionResult GetTankData([FromBody] JsonElement body)
        {
            Console.WriteLine("[REQUEST] Cihaz tank ayarlarını tələb etdi");

            return Ok(new
            {
                code        = 200,
                result      = 0,
                commandType = 9,
                data        = new
                {
                    tankList = _store.Tanks.Select(t => new
                    {
                        tankNo        = t.TankNo,
                        oilCode       = t.OilCode,
                        oilName       = t.OilName,
                        diameter      = t.DiameterMm.ToString(),
                        volume        = t.VolumeLiters.ToString(),
                        oilRate       = t.ExpansionRate,   // "0.0012" benzin
                        used          = t.Enabled ? "1" : "0"
                    })
                },
                msg = (string)null
            });
        }

        // ── PROB AYARLARI ──────────────────────────────────────────────
        [HttpPost("getProbeData")]
        public IActionResult GetProbeData([FromBody] JsonElement body)
        {
            Console.WriteLine("[REQUEST] Cihaz prob ayarlarını tələb etdi");

            return Ok(new
            {
                code        = 200,
                result      = 0,
                commandType = 11,
                data        = new
                {
                    probeList = _store.Probes.Select(p => new
                    {
                        tankNo       = p.TankNo,
                        probeId      = p.ProbeId,
                        probeType    = p.IsDensityProbe ? "1" : "0",
                        oilOffset    = p.OilOffsetMm.ToString("F1"),
                        waterOffset  = p.WaterOffsetMm.ToString("F1"),
                        oilBlind     = p.OilBlindMm.ToString("F1"),
                        highWarning  = p.HighWarningMm.ToString("F1"),
                        highAlarm    = p.HighAlarmMm.ToString("F1"),
                        lowWarning   = p.LowWarningMm.ToString("F1"),
                        lowAlarm     = p.LowAlarmMm.ToString("F1"),
                        waterWarning = p.WaterWarningMm.ToString("F1"),
                        waterAlarm   = p.WaterAlarmMm.ToString("F1"),
                        highTemp     = p.HighTempC.ToString("F1"),
                        lowTemp      = p.LowTempC.ToString("F1")
                    })
                },
                msg = (string)null
            });
        }

        // ── TANK CƏDVƏLİ ──────────────────────────────────────────────
        [HttpPost("getTankVolData")]
        public IActionResult GetTankVolData([FromBody] JsonElement body)
        {
            string tankNo = "01";
            try { tankNo = body.GetProperty("data")
                               .GetProperty("tankNo").GetString(); } catch { }

            var entries = _store.TankTable
                .Where(e => e.TankNo == tankNo).ToList();

            Console.WriteLine($"[REQUEST] Tank {tankNo} cədvəli tələb edildi");

            return Ok(new
            {
                code        = 200,
                result      = 0,
                commandType = 13,
                data        = new
                {
                    tankNo  = tankNo,
                    volList = entries.Select(e => new
                    {
                        height = e.HeightMm.ToString(),
                        volume = e.VolumeLiters.ToString()
                    })
                },
                msg = (string)null
            });
        }

        // ── SİZINTI SENSORU AYARLARI ───────────────────────────────────
        [HttpPost("getSensorSetData")]
        public IActionResult GetSensorSetData([FromBody] JsonElement body)
        {
            Console.WriteLine("[REQUEST] Sensor ayarları tələb edildi");

            return Ok(new
            {
                code        = 200,
                result      = 0,
                commandType = 15,
                data        = new
                {
                    sensorList = _store.Sensors.Select(s => new
                    {
                        sensorNo    = s.SensorNo,
                        sensorType  = s.SensorType,
                        position    = s.Position,
                        positionNum = s.PositionNum,
                        used        = s.Enabled ? "1" : "0"
                    })
                },
                msg = (string)null
            });
        }

        // ── YAĞ MƏHSULU AYARLARI ─────────────────────────────────────────
        [HttpPost("getOilData")]
        public IActionResult GetOilData([FromBody] JsonElement body)
        {
            Console.WriteLine("[REQUEST] Yağ məhsulu ayarları tələb edildi");

            return Ok(new
            {
                code        = 200,
                result      = 0,
                commandType = 7,
                data        = new
                {
                    oilList = _store.OilProducts.Select(o => new
                    {
                        oilCode       = o.OilCode,
                        oilName       = o.OilName,
                        oilColor      = o.OilColor,
                        oilRate       = o.ExpansionRate,
                        temperature   = o.Temperature,
                        weightDensity = o.WeightDensity
                    })
                },
                msg = (string)null
            });
        }

        [HttpPost("uploadOilData")]
        public IActionResult UploadOilData([FromBody] JsonElement body)
        {
            Console.WriteLine("[UPLOAD] Cihaz yağ məhsulu ayarlarını yüklədi");
            return Ok(new
            {
                code        = 200,
                result      = 0,
                commandType = 8,
                msg         = (string)null
            });
        }

        // ── SIXLIQ AYARLARI ──────────────────────────────────────────────
        [HttpPost("getDensityData")]
        public IActionResult GetDensityData([FromBody] JsonElement body)
        {
            Console.WriteLine("[REQUEST] Sıxlıq ayarları tələb edildi");

            return Ok(new
            {
                code        = 200,
                result      = 0,
                commandType = 17,
                data        = new
                {
                    densityList = _store.Densities.Select(d => new
                    {
                        tankNo         = d.TankNo,
                        heightD        = d.HeightDiff,
                        fixRate        = d.FixRate,
                        initDensity    = d.InitDensity,
                        secondDensity  = d.SecondDensity,
                        densityFloatNo = d.DensityFloatNo
                    })
                },
                msg = (string)null
            });
        }

        [HttpPost("uploadDensityData")]
        public IActionResult UploadDensityData([FromBody] JsonElement body)
        {
            Console.WriteLine("[UPLOAD] Cihaz sıxlıq ayarlarını yüklədi");
            return Ok(new
            {
                code        = 200,
                result      = 0,
                commandType = 18,
                msg         = (string)null
            });
        }

        // ── YANACAQ QAZ SENSORU AYARLARI ─────────────────────────────────
        [HttpPost("getGasSetData")]
        public IActionResult GetGasSetData([FromBody] JsonElement body)
        {
            Console.WriteLine("[REQUEST] Qaz sensoru ayarları tələb edildi");

            return Ok(new
            {
                code        = 200,
                result      = 0,
                commandType = 21,
                data        = new
                {
                    gasList = _store.GasSensors.Select(g => new
                    {
                        sensorNo    = g.SensorNo,
                        position    = g.Position,
                        positionNum = g.PositionNum,
                        used        = g.Enabled ? "1" : "0"
                    })
                },
                msg = (string)null
            });
        }

        [HttpPost("uploadGasSetData")]
        public IActionResult UploadGasSetData([FromBody] JsonElement body)
        {
            Console.WriteLine("[UPLOAD] Cihaz qaz sensoru ayarlarını yüklədi");
            return Ok(new
            {
                code        = 200,
                result      = 0,
                commandType = 22,
                msg         = (string)null
            });
        }

        // ── REAL-TIME DATA — Cihazdan məlumat gəlir ───────────────────
        [HttpPost("uploadAtgData")]
        public IActionResult UploadAtgData([FromBody] JsonElement body)
        {
            var data = body.GetProperty("data");
            Console.WriteLine($"[DATA] Tank: {GetStr(data,"tankNo")} " +
                             $"Səviyyə: {GetStr(data,"totalH")}mm " +
                             $"Həcm: {GetStr(data,"oilVt")}L");
            return Ok(new { code=200, result=0, commandType=1, msg=(string)null });
        }

        // ── ALARM DATA ─────────────────────────────────────────────────
        [HttpPost("uploadAtgAlarmData")]
        public IActionResult UploadAlarmData([FromBody] JsonElement body)
        {
            Console.WriteLine("[⚠️ ATG ALARM] " + body.ToString());
            return Ok(new { code=200, result=0, commandType=3, msg=(string)null });
        }

        [HttpPost("uploadDeviceAlarmData")]
        public IActionResult UploadDeviceAlarmData([FromBody] JsonElement body)
        {
            Console.WriteLine("[⚠️ DEVICE ALARM] " + body.ToString());
            return Ok(new { code=200, result=0, msg=(string)null });
        }

        // Helpers
        private string GetIotDevId(JsonElement body)
        {
            try { return body.GetProperty("data").GetProperty("iotDevID").GetString(); }
            catch { return "unknown"; }
        }
        private string GetStr(JsonElement el, string key)
        {
            try { return el.GetProperty(key).GetString(); } catch { return "-"; }
        }
    }
}
