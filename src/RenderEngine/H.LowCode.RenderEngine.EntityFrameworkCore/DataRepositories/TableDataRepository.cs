using H.LowCode.RenderEngine.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.EntityFrameworkCore;

public class TableDataRepository : ITableDataRepository
{
    public bool? IsChangeTrackingEnabled => true;

    public TableDataRepository(RenderEngineDbContext dbContext)
    {
    }
}
