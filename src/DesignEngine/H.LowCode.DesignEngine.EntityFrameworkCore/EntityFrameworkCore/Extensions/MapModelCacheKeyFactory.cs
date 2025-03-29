using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.DesignEngine.EntityFrameworkCore;

internal class MapModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        var lowCodeDbContext = (DesignEngineDbContext)context;
        return Tuple.Create(context.GetType(), lowCodeDbContext.AppId);
    }
}