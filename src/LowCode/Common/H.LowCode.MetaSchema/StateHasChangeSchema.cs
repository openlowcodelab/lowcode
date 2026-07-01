using H.Util.Ids;
using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public abstract class StateHasChangeSchema
{
    [JsonIgnore]
    public string StateKey { get; internal set; } = ShortIdGenerator.Generate();

    public void ChangeStateKey()
    {
        StateKey = ShortIdGenerator.Generate();
    }
}
