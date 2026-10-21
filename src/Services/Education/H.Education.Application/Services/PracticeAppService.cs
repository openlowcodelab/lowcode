using H.Abp.Application.Contracts;
using H.Education.Application.Contracts;
using H.Education.Application.Practice;
using H.Education.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Education.Application.Services;

/// <summary>
/// 趣味练习应用服务。
/// 题目在服务端生成并连答案一起落库，下发给前端的只有题干与选项，
/// 交卷时由服务端逐题判定，算出得分与最长连对，避免答案泄漏到浏览器。
/// </summary>
public class PracticeAppService : ApplicationService, IPracticeAppService
{
    private readonly IRepository<PracticeSessionEntity, Guid> _sessionRepository;
    private readonly IRepository<PracticeItemEntity, Guid> _itemRepository;
    private readonly IRepository<StudentEntity, Guid> _studentRepository;
    private readonly IEnumerable<IPracticeItemGenerator> _generators;

    public PracticeAppService(
        IRepository<PracticeSessionEntity, Guid> sessionRepository,
        IRepository<PracticeItemEntity, Guid> itemRepository,
        IRepository<StudentEntity, Guid> studentRepository,
        IEnumerable<IPracticeItemGenerator> generators)
    {
        _sessionRepository = sessionRepository;
        _itemRepository = itemRepository;
        _studentRepository = studentRepository;
        _generators = generators;
    }

    public Task<BaseOutput<List<PracticeGameDto>>> GetGamesAsync() =>
        Task.FromResult(new BaseOutput<List<PracticeGameDto>>(PracticeGameCatalog.All));

    public async Task<BaseOutput<PagedResultDto<PracticeRecordDto>>> GetListAsync(PracticeRecordQueryDto input)
    {
        var query = await _sessionRepository.GetQueryableAsync();
        // 只列已结算的局：中途刷新会留下 Status=0 的会话，它不是一次成绩
        query = query.Where(x => x.Status == 1);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x => x.StudentName.Contains(input.Filter) || x.Grade.Contains(input.Filter));
        }
        if (input.GameType.HasValue)
        {
            var gameType = (int)input.GameType.Value;
            query = query.Where(x => x.GameType == gameType);
        }
        else if (input.Category.HasValue)
        {
            var gameTypes = PracticeGameCatalog.OfCategory(input.Category.Value)
                .Select(x => (int)x.GameType)
                .ToArray();
            query = query.Where(x => gameTypes.Contains(x.GameType));
        }

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(maxResult));

        return new(new PagedResultDto<PracticeRecordDto>(totalCount, items.Select(x => x.ToDto()).ToList()));
    }

    public async Task<BaseOutput<PracticeResultDto>> GetResultAsync(Guid id)
    {
        var session = await _sessionRepository.GetAsync(id);
        var items = await GetItemsAsync(id);
        return new(BuildResult(session, items));
    }

    public async Task<BaseOutput<PracticeStartDto>> StartAsync(StartPracticeInputDto input)
    {
        var generator = _generators.FirstOrDefault(x => x.GameType == input.GameType)
                        ?? throw new UserFriendlyException("暂不支持该练习游戏");

        var grade = input.Grade?.Trim();
        if (string.IsNullOrWhiteSpace(grade))
        {
            throw new UserFriendlyException("请选择年级");
        }

        var student = input.StudentId is Guid studentId ? await _studentRepository.FindAsync(studentId) : null;
        var count = Math.Clamp(input.Count, 5, 20);
        var game = PracticeGameCatalog.Get(input.GameType);
        var level = Math.Clamp(input.Level, 1, game.MaxLevel);
        var keyword = input.Keyword?.Trim();
        if (game.NeedsKeyword && string.IsNullOrWhiteSpace(keyword))
        {
            throw new UserFriendlyException($"「{game.Name}」需要先输入一个关键字或词");
        }

        var generated = generator.Generate(new PracticeGenerateRequest(
            grade, level, count, keyword, ModeOf(game, input.Mode)));

        if (generated.Count == 0)
        {
            throw new UserFriendlyException("本期题目生成失败，请调整关卡或题目数量后重试");
        }

        var session = new PracticeSessionEntity
        {
            GameType = (int)input.GameType,
            Subject = (int)generator.Subject,
            Grade = grade,
            Level = level,
            StudentId = student?.Id,
            StudentName = student?.Name ?? string.Empty,
            Status = 0,
            TotalCount = generated.Count
        };
        session = await _sessionRepository.InsertAsync(session);

        var sort = 1;
        var items = generated.Select(item => new PracticeItemEntity
        {
            SessionId = session.Id,
            Sort = sort++,
            InputType = (int)item.InputType,
            Content = item.Content,
            OptionsJson = item.Options.Count == 0 ? null : EduJson.Write(item.Options),
            Answer = item.Answer,
            Hint = item.Hint,
            AudioText = item.AudioText,
            LooseMatch = item.LooseMatch,
            BlankCount = item.BlankCount
        }).ToList();

        await _itemRepository.InsertManyAsync(items);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return new(new PracticeStartDto
        {
            SessionId = session.Id,
            GameType = input.GameType,
            GameName = game.Name,
            Level = level,
            TotalCount = items.Count,
            Questions = items.Select(item => new PracticeQuestionDto
            {
                Id = item.Id,
                Sort = item.Sort,
                InputType = (PracticeInputTypeEnum)item.InputType,
                Content = item.Content,
                Options = EduJson.ReadStringList(item.OptionsJson),
                Hint = item.Hint,
                AudioText = item.AudioText,
                BlankCount = item.BlankCount
            }).ToList()
        });
    }

    public async Task<BaseOutput<PracticeItemResultDto>> SubmitItemAsync(SubmitPracticeItemInputDto input)
    {
        var session = await _sessionRepository.GetAsync(input.SessionId);
        if (session.Status == 1)
        {
            throw new UserFriendlyException("本局练习已结算，请重新开始一局");
        }

        var item = await _itemRepository.GetAsync(input.ItemId);
        if (item.SessionId != session.Id)
        {
            throw new UserFriendlyException("该题目不属于本局练习");
        }

        item.StudentAnswer = input.Answer?.Trim();
        item.IsCorrect = PracticeAnswerMatcher.IsCorrect(item.Answer, item.StudentAnswer, item.LooseMatch);
        await _itemRepository.UpdateAsync(item);
        await CurrentUnitOfWork!.SaveChangesAsync();

        var items = await GetItemsAsync(session.Id);
        // 连对数从最后一道已判定题往前数，跳过还没作答的尾部题目
        var combo = 0;
        for (var i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].IsCorrect == null) continue;
            if (items[i].IsCorrect == true) combo++;
            else break;
        }

        return new(new PracticeItemResultDto
        {
            ItemId = item.Id,
            IsCorrect = item.IsCorrect == true,
            Answer = DisplayAnswer(item.Answer),
            Combo = combo,
            AnsweredCount = items.Count(x => x.IsCorrect.HasValue),
            CorrectCount = items.Count(x => x.IsCorrect == true)
        });
    }

    public async Task<BaseOutput<PracticeResultDto>> SubmitAsync(Guid id, SubmitPracticeInputDto input)
    {
        var session = await _sessionRepository.GetAsync(id);
        var items = await GetItemsAsync(id);

        if (session.Status == 1)
        {
            return new(BuildResult(session, items));
        }

        var answers = new Dictionary<Guid, string?>();
        foreach (var answer in input.Items)
        {
            answers[answer.ItemId] = answer.Answer;
        }

        var combo = 0;
        var correctCount = 0;

        foreach (var item in items)
        {
            // 单题提交时已经存过作答，结算只覆盖本次显式提交的题目，避免把之前的作答清成未答
            var studentAnswer = answers.TryGetValue(item.Id, out var answer) && !string.IsNullOrWhiteSpace(answer)
                ? answer
                : item.StudentAnswer;
            item.StudentAnswer = studentAnswer?.Trim();
            item.IsCorrect = PracticeAnswerMatcher.IsCorrect(item.Answer, item.StudentAnswer, item.LooseMatch);

            if (item.IsCorrect == true)
            {
                combo++;
                correctCount++;
                session.MaxCombo = Math.Max(session.MaxCombo, combo);
            }
            else
            {
                combo = 0;
            }
        }

        var bonus = Math.Max(0, session.MaxCombo - 3) * 5;
        var score = correctCount * 10 + bonus;

        session.CorrectCount = correctCount;
        session.Score = score;
        session.DurationSeconds = Math.Max(0, input.DurationSeconds);
        session.Status = 1;

        await _itemRepository.UpdateManyAsync(items);
        await _sessionRepository.UpdateAsync(session);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return new(BuildResult(session, items));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        var session = await _sessionRepository.GetAsync(id);

        await _itemRepository.DeleteAsync(x => x.SessionId == id);
        await _sessionRepository.DeleteAsync(session);

        return new();
    }

    private async Task<List<PracticeItemEntity>> GetItemsAsync(Guid sessionId)
    {
        var items = await _itemRepository.GetListAsync(x => x.SessionId == sessionId);
        items.Sort((a, b) => a.Sort.CompareTo(b.Sort));
        return items;
    }

    private static PracticeResultDto BuildResult(PracticeSessionEntity session, List<PracticeItemEntity> items) => new()
    {
        SessionId = session.Id,
        GameType = (PracticeGameTypeEnum)session.GameType,
        GameName = ((PracticeGameTypeEnum)session.GameType).GetGameName(),
        Grade = session.Grade,
        Level = session.Level,
        StudentName = session.StudentName,
        TotalCount = session.TotalCount,
        CorrectCount = session.CorrectCount,
        Score = session.Score,
        MaxCombo = session.MaxCombo,
        AccuracyRate = session.TotalCount == 0 ? 0 : Math.Round(session.CorrectCount * 100m / session.TotalCount, 1),
        DurationSeconds = session.DurationSeconds,
        FinishTime = session.LastModificationTime ?? session.CreationTime,
        Items = items.Select(item => new PracticeResultItemDto
        {
            Id = item.Id,
            Sort = item.Sort,
            InputType = (PracticeInputTypeEnum)item.InputType,
            Content = item.Content,
            Options = EduJson.ReadStringList(item.OptionsJson),
            Hint = item.Hint,
            StudentAnswer = item.StudentAnswer,
            Answer = DisplayAnswer(item.Answer),
            IsCorrect = item.IsCorrect == true,
            AudioText = item.AudioText
        }).ToList()
    };

    /// <summary>参考答案可能有多种写法（用 | 分隔），讲解时只展示最规范的一条</summary>
    private static string DisplayAnswer(string answer) =>
        answer.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? answer;

    /// <summary>模式只认目录里登记过的取值，不传或传错时退回第一项，保证出题器拿到的一定是有效模式</summary>
    private static string? ModeOf(PracticeGameDto game, string? mode)
    {
        if (game.Modes.Count == 0) return null;

        var value = mode?.Trim();
        return game.Modes.Any(x => x.Value == value) ? value : game.Modes[0].Value;
    }
}
