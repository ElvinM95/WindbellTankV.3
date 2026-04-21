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
                msg = (string?)null
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
                msg = (string?)null
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
                msg = (string?)null
            });
        }

        // ── TANK CƏDVƏLİ ──────────────────────────────────────────────
        [HttpPost("getTankVolData")]
        public IActionResult GetTankVolData([FromBody] JsonElement body)
        {
            string tankNo = "01";
            try { tankNo = body.GetProperty("data")
                               .GetProperty("tankNo").GetString() ?? "01"; } catch { }

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
                msg = (string?)null
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
                msg = (string?)null
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
                msg = (string?)null
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
                msg         = (string?)null
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
                msg = (string?)null
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
                msg         = (string?)null
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
                msg = (string?)null
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
                msg         = (string?)null
            });
        }

        // ── REAL-TIME DATA — Cihazdan məlumat gəlir ───────────────────
        [HttpPost("uploadAtgData")]
        public IActionResult UploadAtgData([FromBody] JsonElement body)
        {
            try
            {
                var data = body.GetProperty("data");

                // İdentifikasiya
                string iotDevID = GetStr(data, "iotDevID");
                string tankNo = GetStr(data, "tankNo");
                string oilCode = GetStr(data, "oilCode");
                string oilName = GetStr(data, "oilName");

                // Səviyyə məlumatları
                string totalH = GetStr(data, "totalH");
                string waterH = GetStr(data, "waterH");
                string oilVt = GetStr(data, "oilVt");
                string waterVt = GetStr(data, "waterVt");
                string ullage = GetStr(data, "ullage");

                // Temperatur
                string oilT = GetStr(data, "oilT");
                string t1 = GetStr(data, "t1");
                string t2 = GetStr(data, "t2");
                string t3 = GetStr(data, "t3");
                string t4 = GetStr(data, "t4");

                // Həcm Kompensasiyası
                string oilV20 = GetStr(data, "oilV20");
                string totalV20 = GetStr(data, "totalV20");

                // Status
                string probeValveCode = GetStr(data, "probeValve");
                string probeValveText = probeValveCode switch
                {
                    "1" => "Normal",
                    "4" => "Xəta",
                    "6" => "Siqnal kəsildi",
                    _ => $"Bilinmir ({probeValveCode})"
                };

                // Digər
                string density = GetStr(data, "density");
                string weight = GetStr(data, "weight");
                string rawTime = GetStr(data, "uploadTime");
                
                string formattedTime = rawTime;
                if (DateTime.TryParse(rawTime, out DateTime parsedTime))
                {
                    formattedTime = parsedTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else if (!string.IsNullOrEmpty(rawTime) && rawTime.Length == 14 && long.TryParse(rawTime, out _))
                {
                    if (DateTime.TryParseExact(rawTime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime exactParsed))
                    {
                        formattedTime = exactParsed.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("\n=========================================");
                sb.AppendLine($"[REAL-TIME DATA] Yüklənmə Vaxtı: {formattedTime}");
                
                sb.AppendLine("--- İdentifikasiya ---");
                sb.AppendLine($"Cihaz ID: {iotDevID} | Çən №: {tankNo} | Məhsul: {oilCode} ({oilName})");
                
                sb.AppendLine("--- Səviyyə Məlumatları ---");
                sb.AppendLine($"Ümumi hündürlük: {totalH} mm | Su səviyyəsi: {waterH} mm");
                sb.AppendLine($"Xalis yanacaq həcmi: {oilVt} L | Su həcmi: {waterVt} L | Boş qalan həcm (Ullage): {ullage} L");

                sb.AppendLine("--- Temperatur ---");
                sb.AppendLine($"Ortalama Temp: {oilT} °C (T1: {t1}, T2: {t2}, T3: {t3}, T4: {t4})");

                sb.AppendLine("--- Həcm Kompensasiyası ---");
                sb.AppendLine($"V20 Standart Həcm: {oilV20} L | Ümumi Standart Həcm: {totalV20} L");

                sb.AppendLine("--- Status ---");
                sb.AppendLine($"Zond Statusu: {probeValveText}");

                sb.AppendLine("--- Digər ---");
                sb.AppendLine($"Sıxlıq: {density} kg/m³ | Çəki: {weight} kq");
                sb.AppendLine("=========================================");

                Console.WriteLine(sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UploadAtgData parse edilərkən xəta: {ex.Message}");
            }

            return Ok(new { code = 200, result = 0, commandType = 1, msg = (string?)null });
        }

        // ── ALARM DATA ─────────────────────────────────────────────────
        [HttpPost("uploadAtgAlarmData")]
        public IActionResult UploadAlarmData([FromBody] JsonElement body)
        {
            Console.WriteLine("[⚠️ ATG ALARM] " + body.ToString());
            return Ok(new { code=200, result=0, commandType=3, msg=(string?)null });
        }

        [HttpPost("uploadDeviceAlarmData")]
        public IActionResult UploadDeviceAlarmData([FromBody] JsonElement body)
        {
            Console.WriteLine("[⚠️ DEVICE ALARM] " + body.ToString());
            return Ok(new { code=200, result=0, msg=(string?)null });
        }

        // Helpers
        private string GetIotDevId(JsonElement body)
        {
            try { return body.GetProperty("data").GetProperty("iotDevID").GetString() ?? "unknown"; }
            catch { return "unknown"; }
        }
        private string GetStr(JsonElement el, string key)
        {
            try { return el.GetProperty(key).GetString() ?? "-"; } catch { return "-"; }
        }

        // ── MODBUS CRC16 HESABLANMASI (Token Sign üçün) ────────────────
        // Cihaza parametr/ayar göndərərkən "token" bloku daxilində "sign" parametrini hesablamaq üçün
        [NonAction]
        public string CalculateModbusCrc16(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            
            // Cihaz sənədlərinə əsasən şifrələnəcək məlumat UTF-8 yaxud ASCII ola bilər
            byte[] data = System.Text.Encoding.UTF8.GetBytes(input);
            return CalculateModbusCrc16(data);
        }

        [NonAction]
        public string CalculateModbusCrc16(byte[] data)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            
            // Cihazın tələbindən asılı olaraq CRC baytlarının yeri (Little Endian / Big Endian) dəyişə bilər.
            // Bu kod nəticəni HEX formatında (məs: "A1B2") qaytarır.
            // Əgər cihaz "Little Endian" (kiçik bayt öndə) istəyirsə, aşağıdakı qaydada çevirə bilərsiniz:
            // byte[] crcBytes = BitConverter.GetBytes(crc);
            // return BitConverter.ToString(crcBytes).Replace("-", "");

            return crc.ToString("X4");
        }
    }
}
