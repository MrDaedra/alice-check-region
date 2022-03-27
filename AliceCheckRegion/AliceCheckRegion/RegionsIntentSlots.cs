using System.Text.Json.Serialization;
using Yandex.Alice.Sdk.Converters;
using Yandex.Alice.Sdk.Models;

namespace AliceCheckRegion
{
    public class RegionsIntentSlots
    {
        [JsonPropertyName("question")]
        [JsonConverter(typeof(AliceEntityModelConverter))]
        public AliceEntityModel? Question { get; set; }

        [JsonPropertyName("region")]
        [JsonConverter(typeof(AliceEntityModelConverter))]
        public AliceEntityModel? Region { get; set; }
    }
}
