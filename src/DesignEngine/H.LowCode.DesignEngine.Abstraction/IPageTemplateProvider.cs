using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.Abstraction
{
    public interface IPageTemplateProvider
    {
        public string Title { get; set; }

        IEnumerable<ComponentSchema> LoadPageTemplate();
    }
}
