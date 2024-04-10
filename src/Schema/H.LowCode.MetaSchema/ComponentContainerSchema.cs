using static System.Runtime.InteropServices.JavaScript.JSType;

namespace H.LowCode.MetaSchema
{
    public class ComponentContainerSchema : MetaSchema
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Class { get; set; }

        public string Style { get; set; }

        public Action Refresh { get; set; }

        public string ParentId { get; set; }

        public IList<ComponentSchema> Components { get; set; } = new List<ComponentSchema>();

        public IList<ComponentContainerSchema> Childrens { get; set; } = new List<ComponentContainerSchema>();

        public void Refresh_StateChange()
        {
            if (Refresh != null) Refresh();
        }

        public bool Equals(ComponentContainerSchema other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return false;
        }
    }
}
