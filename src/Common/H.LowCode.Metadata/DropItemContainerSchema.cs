namespace H.LowCode.Metadata
{
    public class DropItemContainerSchema
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Class { get; set; }

        public string Style { get; set; }

        public Action Refresh { get; set; }

        public DropItemContainerSchema ParentDropItemContainerSchema { get; set; }

        public IList<ComponentSchema> ComponentSchemas { get; set; } = new List<ComponentSchema>();

        public IList<DropItemContainerSchema> ChildDropItemContainerSchema { get; set; } = new List<DropItemContainerSchema>();

        public void Refresh_StateChange()
        {
            if (Refresh != null) Refresh();
        }

        public bool Equals(DropItemContainerSchema other)
        {
            if (!ReferenceEquals(other, null))
                return false;

            if (ParentDropItemContainerSchema != other.ParentDropItemContainerSchema)
                return false;

            if (ComponentSchemas != other.ComponentSchemas)
                return false;

            if (ChildDropItemContainerSchema != other.ChildDropItemContainerSchema)
                return false;

            if (Class != other.Class || Style != other.Style)
                return false;

            return true;
        }
    }
}
