using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace H.LowCode.DesignEngine.EntityFrameworkCore;

internal class DesignEngineModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        var lowCodeDbContext = (DesignEngineDbContext)context;
        return Tuple.Create(context.GetType(), lowCodeDbContext.AppId);
    }
}