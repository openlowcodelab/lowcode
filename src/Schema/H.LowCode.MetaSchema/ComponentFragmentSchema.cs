
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    public class ComponentFragmentSchema
    {
        [JsonPropertyName("idx")]
        public int Index { get; set; }

        [JsonPropertyName("frageenum")]
        public FragmentEnum FragmentEnum { get; set; }

        [JsonPropertyName("fragname")]
        public string ComponentFragmentName { get; set; }

        [JsonPropertyName("n")]
        public string Name { get; set; }

        [JsonPropertyName("valtype")]
        public ComponentValueType ValueType { get; set; }

        [JsonPropertyName("intval")]
        public int IntValue { get; set; }

        [JsonPropertyName("strval")]
        public string StringValue { get; set; }
    }

    public enum FragmentEnum
    {
        Component,
        Attribute,
        Parameter
    }
}
