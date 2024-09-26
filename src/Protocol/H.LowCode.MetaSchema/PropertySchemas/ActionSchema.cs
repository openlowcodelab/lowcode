using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public class ActionSchema
{
    public string Id { get; set; }

    public string Name { get; set; }

    public ActionTypeEnum ActionType { get; set; }
}

public enum ActionTypeEnum
{
    None,
    Modal,
    Self,
    Blank
}
