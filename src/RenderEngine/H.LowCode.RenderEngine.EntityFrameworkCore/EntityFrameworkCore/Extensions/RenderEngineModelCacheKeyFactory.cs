using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace H.LowCode.RenderEngine.EntityFrameworkCore;

internal class RenderEngineModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        var lowCodeDbContext = (RenderEngineDbContext)context;
        return Tuple.Create(context.GetType(), lowCodeDbContext.AppId);
    }
}