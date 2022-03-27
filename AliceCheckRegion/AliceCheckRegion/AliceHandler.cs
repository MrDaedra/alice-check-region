using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Yandex.Alice.Sdk.Models;

namespace AliceCheckRegion
{
    public class AliceHandler
    {
        public async Task<AliceResponse> FunctionHandler(RegionsAliceRequest aliceRequest)
        {
            AliceResponse response;
            if (aliceRequest?.Request?.Nlu?.Intents?.CheckRegion?.Slots?.Region != null)
            {
                double region = ((AliceEntityNumberModel)(aliceRequest.Request.Nlu.Intents.CheckRegion.Slots.Region!)).Value;
                if (region >= 1000 || region == 0)
                {
                    return new AliceResponse(aliceRequest, $"Региона с кодом {region} не существует");
                }

                using FileStream fileStream = File.OpenRead("./regions.json");
                using var streamReader = new StreamReader(fileStream);
                string? regionsString = await streamReader.ReadToEndAsync();
                List<RegionInfo> regions = JsonConvert.DeserializeObject<List<RegionInfo>>(regionsString);

                string? regionName = GetRegionName(region, regions);

                if (regionName != null)
                {
                    response = new AliceResponse(aliceRequest, $"Регион {region} - это {regionName}");
                    response.Response.EndSession = true;
                }
                else
                {
                    double baseRegion = region % 100;
                    regionName = GetRegionName(baseRegion, regions);

                    response = regionName != null
                        ? new AliceResponse(aliceRequest, $"Я не знаю точно, но думаю, что {region} - это {regionName}")
                        : new AliceResponse(aliceRequest, $"Я думаю, что региона с таким кодом не существует");

                    response.Response.EndSession = true;
                }
            }
            else
            {
                response = new AliceResponse(aliceRequest, $"Извините, я не поняла какой регион вы имеете в виду");
            }

            return response;
        }

        private static string? GetRegionName(double region, List<RegionInfo> regions)
        {
            string? stringRegion = region.ToString();
            if (region < 10)
            {
                stringRegion = "0" + stringRegion;
            }

            return regions.FirstOrDefault(r => r.Code == stringRegion)?.Region;
        }
    }
}
