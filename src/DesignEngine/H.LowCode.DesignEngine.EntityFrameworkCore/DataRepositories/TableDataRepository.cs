using H.LowCode.DesignEngine.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.DesignEngine.EntityFrameworkCore;

public class TableDataRepository : ITableDataRepository
{
    public bool? IsChangeTrackingEnabled => true;

    public TableDataRepository(DesignEngineDbContext dbContext)
    {
    }
}
