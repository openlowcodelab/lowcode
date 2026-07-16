using System;
using Volo.Abp.Domain.Entities;

namespace H.Approval.EntityFrameworkCore;

/// <summary>
/// 审批分类(分组)实体
/// </summary>
public class ApprovalCategory : Entity<string>
{
    protected ApprovalCategory()
    {
    }

    public ApprovalCategory(string id)
    {
        Id = id;
    }

    /// <summary>分类名称</summary>
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>排序值(升序)</summary>
    public virtual int Sort { get; set; }

    /// <summary>创建时间</summary>
    public virtual DateTime CreationTime { get; set; }
}
