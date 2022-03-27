using System.Text.Json.Serialization;
using Yandex.Alice.Sdk.Models;

namespace AliceCheckRegion
{
    public class RegionsIntent
    {
        [JsonPropertyName("Check.region")]
        public AliceIntentModel<RegionsIntentSlots>? CheckRegion { get; set; }
    }
}
