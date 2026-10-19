using System.ComponentModel.DataAnnotations;
using H.Abp.Application.Contracts;

namespace H.Education.Application.Contracts;

/// <summary>
/// 学生 DTO
/// </summary>
public class StudentDto
{
    public Guid Id { get; set; }
    public string? StudentNo { get; set; }
    public string Name { get; set; } = string.Empty;
    public GenderEnum Gender { get; set; }
    public string Grade { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public string? GuardianName { get; set; }
    public string? GuardianPhone { get; set; }
    public string? Remark { get; set; }

    /// <summary>已交卷的答卷数</summary>
    public int AnswerCount { get; set; }

    /// <summary>已批改答卷的平均得分率</summary>
    public decimal AvgAccuracyRate { get; set; }

    /// <summary>趣味练习局数</summary>
    public int PracticeCount { get; set; }

    public DateTime CreationTime { get; set; }
}

/// <summary>
/// 学生分页查询条件
/// </summary>
public class StudentQueryDto : PagedAndSortedResultRequestDto
{
    /// <summary>姓名或学号模糊查询</summary>
    public string? Filter { get; set; }

    public string? Grade { get; set; }

    public GenderEnum? Gender { get; set; }
}

/// <summary>
/// 新增学生入参
/// </summary>
public class CreateStudentDto
{
    [StringLength(32, ErrorMessage = "姓名长度不能超过 32")]
    [Required(ErrorMessage = "请填写姓名")]
    public string Name { get; set; } = string.Empty;

    /// <summary>学号，可留空；填写后不可与其它学生重复</summary>
    [StringLength(32, ErrorMessage = "学号长度不能超过 32")]
    public string? StudentNo { get; set; }

    public GenderEnum Gender { get; set; }

    [Required(ErrorMessage = "请填写年级")]
    [StringLength(16, ErrorMessage = "年级长度不能超过 16")]
    public string Grade { get; set; } = string.Empty;

    [StringLength(32, ErrorMessage = "班级长度不能超过 32")]
    public string? ClassName { get; set; }

    [StringLength(32, ErrorMessage = "家长姓名长度不能超过 32")]
    public string? GuardianName { get; set; }

    [StringLength(24, ErrorMessage = "家长电话长度不能超过 24")]
    public string? GuardianPhone { get; set; }

    [StringLength(500, ErrorMessage = "备注长度不能超过 500")]
    public string? Remark { get; set; }
}

/// <summary>
/// 更新学生入参
/// </summary>
public class UpdateStudentDto : CreateStudentDto
{
}
