
namespace H.LowCode.MetaSchema
{
    public class ComponentFragmentSchema
    {
        public int Index { get; set; }

        public FragmentEnum FragmentEnum { get; set; }

        public string ComponentFragmentName { get; set; }

        public string Name { get; set; }

        public ComponentValueType ValueType { get; set; }

        public int IntValue { get; set; }

        public string StringValue { get; set; }
    }

    public enum FragmentEnum
    {
        Component,
        Attribute,
        Parameter
    }
}
