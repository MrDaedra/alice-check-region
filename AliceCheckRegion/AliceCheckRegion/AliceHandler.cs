using System;
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
            LogMessage($"REQUEST: {aliceRequest.Request.Command}\n", aliceRequest);
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
                    response.Response.EndSession = aliceRequest.Session.MessageId == 0;
                }
                else
                {
                    double baseRegion = region % 100;
                    regionName = GetRegionName(baseRegion, regions);

                    response = regionName != null
                        ? new AliceResponse(aliceRequest, $"Я не знаю точно, но думаю, что {region} - это {regionName}")
                        : new AliceResponse(aliceRequest, $"Я думаю, что региона с таким кодом не существует");

                    response.Response.EndSession = aliceRequest.Session.MessageId == 0;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(aliceRequest?.Request.Command))
                {
                    response = new AliceResponse(aliceRequest,
                        $"Этот навык подскажет название региона России по его коду на автомобильном номере. Для использования спросите меня про код региона, например \"Чей регион 198?\"");
                }
#pragma warning disable IDE0045 // Convert to conditional expression
                else if (aliceRequest?.Request.Command.ToLowerInvariant() == "помощь")
                {
                    response = new AliceResponse(aliceRequest,
                        $"Чтобы узнать к какому региону относится код, спросите меня, например \"Что за регион 777?\" или \"Какой регион 69?\"");
                }
                else if (aliceRequest?.Request.Command.ToLowerInvariant() == "что ты умеешь")
                {
                    response = new AliceResponse(aliceRequest,
                        $"Я могу подсказать название региона России по его коду на автомобильном номере. Спросите меня, например \"Что за регион 777?\" или \"Какой регион 69?\"");
                }
                else
                {
                    response = new AliceResponse(aliceRequest, $"Извините, я не поняла какой регион вы имеете в виду");
                }
#pragma warning restore IDE0045 // Convert to conditional expression
            }

            LogMessage($"RESPONSE: {response.Response.Text}\n", aliceRequest!);

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

        private static void LogMessage(string message, RegionsAliceRequest aliceRequest)
        {
            if (aliceRequest?.Request?.Command.ToLowerInvariant() != "ping")
            {
                Console.WriteLine(message);
            }
        }
    }
}
