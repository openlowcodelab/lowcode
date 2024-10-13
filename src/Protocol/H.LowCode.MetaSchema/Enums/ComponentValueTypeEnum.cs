using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public enum ComponentValueTypeEnum
{
    None = 0,
    String = 1,
    Textarea = 2,
    Text = 3,
    Integer = 6,
    Double = 7,
    Decimal = 8,
    Boolean = 13,
    Date = 18,
    Array = 25,
    Option = 30,
    Table = 35,
    List = 36,
    Tree = 42
}
