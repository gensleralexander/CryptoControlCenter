using Newtonsoft.Json;

namespace Bitstamp.Net.Objects.Helper
{
    public class BitstampData<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}