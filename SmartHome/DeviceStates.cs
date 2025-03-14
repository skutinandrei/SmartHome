using Newtonsoft.Json;

namespace SmartHome
{
    public class DeviceStates
    {
        public bool AcIsOn { get; set; }
        public int AcTemperature { get; set; }
        public int CurrentTemperature { get; set; }
        public bool LightIsOn { get; set; }
        public int LightTemperature { get; set; } // 2000-6500K (теплый-холодный)
    }

    public class DeviceStateResponse
    {
        [JsonProperty("capabilities")]
        public List<CapabilityState>? Capabilities { get; set; }
    }

    public class YandexDeviceListResponse
    {
        [JsonProperty("devices")]
        public List<YandexDevice> Devices { get; set; } = new List<YandexDevice>();
    }

    public class YandexDevice
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("capabilities")]
        public List<DeviceCapability> Capabilities { get; set; } = new List<DeviceCapability>();

        [JsonProperty("properties")]
        public List<DeviceProperty> Properties { get; set; } = new List<DeviceProperty>();
    }

    public class DeviceCapability
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("state")]
        public CapabilityState State { get; set; } = new CapabilityState();
    }

    public class CapabilityState
    {
        [JsonProperty("instance")]
        public string Instance { get; set; } = string.Empty;

        [JsonProperty("value")]
        public object Value { get; set; } = default!;
    }

    public class DeviceProperty
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("state")]
        public PropertyState State { get; set; } = new PropertyState();
    }

    public class PropertyState
    {
        [JsonProperty("instance")]
        public string Instance { get; set; } = string.Empty;

        [JsonProperty("value")]
        public object Value { get; set; } = default!;
    }
}
