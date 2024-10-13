using H.Util.Ids;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class MenuSchema : MetaSchemaBase
{
    public MenuSchema(bool isNewId)
    {
        if (isNewId)
        {
            Id = ShortIdGenerator.Generate();
        }
    }

    public MenuSchema()
    {

    }

    [JsonPropertyName("aid")]
    public string AppId {  get; set; }

    public string Id { get; set; }

    [JsonPropertyName("pid")]
    public string ParentId { get; set; }

    [JsonPropertyName("t")]
    public string Title { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    /// <remarks>0-菜单 1-目录</remarks>
    [JsonPropertyName("type")]
    public int MenuType { get; set; }

    public string Icon {  get; set; }

    /// <summary>
    /// 菜单地址
    /// </summary>
    [JsonPropertyName("path")]
    public string MenuUrl { get; set; }

    public int Order { get; set; }

    [JsonPropertyName("childs")]
    public IList<MenuSchema> Childrens { get; set; } = [];
}
