namespace H.LowCode.Metadata
{
    public class DropItemContainerSchema
    {
        public string Class { get; set; }

        public string Style { get; set; }

        public DropItemContainerSchema ParentDropItemContainerSchema { get; set; }

        public IList<ComponentSchema> ComponentSchemas { get; set; } = new List<ComponentSchema>();

        public IList<DropItemContainerSchema> ChildDropItemContainerSchema { get; set; } = new List<DropItemContainerSchema>();
    }
}
