using H.Abp.Application.Contracts;
using H.Education.Application.Contracts;
using H.Education.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Education.Application.Services;

/// <summary>
/// 学生应用服务：档案维护 + 学习概况（答题数、平均得分率、练习局数）
/// </summary>
public class StudentAppService : ApplicationService, IStudentAppService
{
    private readonly IRepository<StudentEntity, Guid> _studentRepository;
    private readonly IRepository<AnswerSheetEntity, Guid> _sheetRepository;
    private readonly IRepository<PracticeSessionEntity, Guid> _sessionRepository;

    public StudentAppService(
        IRepository<StudentEntity, Guid> studentRepository,
        IRepository<AnswerSheetEntity, Guid> sheetRepository,
        IRepository<PracticeSessionEntity, Guid> sessionRepository)
    {
        _studentRepository = studentRepository;
        _sheetRepository = sheetRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<BaseOutput<PagedResultDto<StudentDto>>> GetListAsync(StudentQueryDto input)
    {
        var query = await _studentRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.Trim();
            query = query.Where(x => x.Name.Contains(filter) || (x.StudentNo != null && x.StudentNo.Contains(filter)));
        }
        if (!string.IsNullOrWhiteSpace(input.Grade))
        {
            var grade = input.Grade.Trim();
            query = query.Where(x => x.Grade == grade);
        }
        if (input.Gender.HasValue)
        {
            var gender = (int)input.Gender.Value;
            query = query.Where(x => x.Gender == gender);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 20;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.Grade).ThenBy(x => x.Name).Skip(input.SkipCount).Take(maxResult));

        var dtos = entities.Select(x => x.ToDto()).ToList();
        await FillStudyStatsAsync(dtos);

        return new(new PagedResultDto<StudentDto>(totalCount, dtos));
    }

    public async Task<BaseOutput<StudentDto>> GetAsync(Guid id)
    {
        var student = await _studentRepository.GetAsync(id);
        return new(student.ToDto());
    }

    public async Task<BaseOutput<StudentDto>> CreateAsync(CreateStudentDto input)
    {
        var student = new StudentEntity
        {
            Name = input.Name.Trim(),
            StudentNo = NormalizeOptional(input.StudentNo),
            Gender = (int)input.Gender,
            Grade = input.Grade.Trim(),
            ClassName = NormalizeOptional(input.ClassName),
            GuardianName = NormalizeOptional(input.GuardianName),
            GuardianPhone = NormalizeOptional(input.GuardianPhone),
            Remark = NormalizeOptional(input.Remark)
        };

        await EnsureStudentNoUnusedAsync(student.StudentNo, Guid.Empty);

        student = await _studentRepository.InsertAsync(student);
        return new(student.ToDto());
    }

    public async Task<BaseOutput<StudentDto>> UpdateAsync(Guid id, UpdateStudentDto input)
    {
        var student = await _studentRepository.GetAsync(id);

        student.Name = input.Name.Trim();
        student.StudentNo = NormalizeOptional(input.StudentNo);
        student.Gender = (int)input.Gender;
        student.Grade = input.Grade.Trim();
        student.ClassName = NormalizeOptional(input.ClassName);
        student.GuardianName = NormalizeOptional(input.GuardianName);
        student.GuardianPhone = NormalizeOptional(input.GuardianPhone);
        student.Remark = NormalizeOptional(input.Remark);

        await EnsureStudentNoUnusedAsync(student.StudentNo, id);

        await _studentRepository.UpdateAsync(student);
        return new(student.ToDto());
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        var student = await _studentRepository.GetAsync(id);

        // 答卷与练习记录都存了姓名快照，删档案不会抹掉历史成绩
        await _studentRepository.DeleteAsync(student);

        return new();
    }

    private async Task EnsureStudentNoUnusedAsync(string? studentNo, Guid selfId)
    {
        if (string.IsNullOrWhiteSpace(studentNo)) return;

        var query = await _studentRepository.GetQueryableAsync();
        var duplicated = await AsyncExecuter.AnyAsync(
            query.Where(x => x.StudentNo == studentNo && x.Id != selfId));
        if (duplicated)
        {
            throw new UserFriendlyException($"学号 {studentNo} 已被其它学生使用");
        }
    }

    private async Task FillStudyStatsAsync(List<StudentDto> dtos)
    {
        if (dtos.Count == 0) return;

        var ids = dtos.Select(x => x.Id).ToList();
        var graded = (int)AnswerSheetStatusEnum.Graded;

        var sheetQuery = await _sheetRepository.GetQueryableAsync();
        var answers = await AsyncExecuter.ToListAsync(
            sheetQuery
                .Where(x => x.StudentId != null && ids.Contains(x.StudentId!.Value) && x.Status == graded)
                .GroupBy(x => x.StudentId!.Value)
                .Select(g => new { StudentId = g.Key, Count = g.Count(), Avg = g.Average(x => x.AccuracyRate) }));

        var sessionQuery = await _sessionRepository.GetQueryableAsync();
        var practices = await AsyncExecuter.ToListAsync(
            sessionQuery
                .Where(x => x.StudentId != null && ids.Contains(x.StudentId!.Value))
                .GroupBy(x => x.StudentId!.Value)
                .Select(g => new { StudentId = g.Key, Count = g.Count() }));

        var answerMap = answers.ToDictionary(x => x.StudentId, x => x);
        var practiceMap = practices.ToDictionary(x => x.StudentId, x => x.Count);

        foreach (var dto in dtos)
        {
            if (answerMap.TryGetValue(dto.Id, out var answer))
            {
                dto.AnswerCount = answer.Count;
                dto.AvgAccuracyRate = Math.Round(answer.Avg, 1);
            }
            dto.PracticeCount = practiceMap.TryGetValue(dto.Id, out var practice) ? practice : 0;
        }
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
