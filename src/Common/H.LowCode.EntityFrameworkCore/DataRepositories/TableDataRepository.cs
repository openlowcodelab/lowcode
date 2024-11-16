using H.LowCode.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.EntityFrameworkCore;

public class TableDataRepository : ITableDataRepository
{
    public bool? IsChangeTrackingEnabled => true;

    public TableDataRepository(LowCodeDbContext dbContext)
    {
    }
}
