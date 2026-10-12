using System.Collections.Concurrent;
using EasyCaching.Core;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Domain.Exceptions;
using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.Domain.Servcies;
using FastQuiz.Question.UseCase.Extensions;

namespace FastQuiz.Question.Infrastructure.Services;

public class QuestionRandomPicker(
    IQuestionRepository _questionRepository,
    IEasyCachingProviderFactory _easyCachingProviderFactory
) : IQuestionRandomPicker
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _cacheLocks = new();
    private readonly IEasyCachingProvider _cache =
        _easyCachingProviderFactory.GetQuestionCacheProvider();

    private static string QuestionIdListCacheKey(string groupId, QuestionType type, int level) =>
        $"QuestionIdList@{groupId}:{type}:{level}";

    public async Task<IReadOnlyList<Domain.Entities.Question>> PickAsync(
        string groupId,
        QuestionType questionType,
        int level,
        int count
    )
    {
        var cachingKey = QuestionIdListCacheKey(groupId, questionType, level);

        var cached = await _cache.GetAsync<IReadOnlyList<string>>(cachingKey).ConfigureAwait(false);

        if (cached.HasValue)
        {
            var resultIds = RandomPick(cached.Value, count);
            var result = await _questionRepository.GetByIdAsync(resultIds).ConfigureAwait(false);
            return result;
        }

        var lockObj = _cacheLocks.GetOrAdd(cachingKey, new SemaphoreSlim(1, 1));
        await lockObj.WaitAsync().ConfigureAwait(false);

        try
        {
            var doubleCheck = await _cache
                .GetAsync<IReadOnlyList<string>>(cachingKey)
                .ConfigureAwait(false);
            if (doubleCheck.HasValue)
            {
                var resultIdList = RandomPick(doubleCheck.Value, count);
                var resultQuestions = await _questionRepository
                    .GetByIdAsync(resultIdList)
                    .ConfigureAwait(false);
                return resultQuestions;
            }

            var idList = await _questionRepository
                .GetQuestionIdsAsync(groupId, questionType, level)
                .ConfigureAwait(false);

            var expires = GetRandomExpiration();
            await _cache.SetAsync(cachingKey, idList, expires).ConfigureAwait(false);

            var resultIds = RandomPick(idList, count);
            var result = await _questionRepository.GetByIdAsync(resultIds).ConfigureAwait(false);
            return result;
        }
        finally
        {
            lockObj.Release();
        }
    }

    private static IReadOnlyList<string> RandomPick(IReadOnlyList<string> idList, int count)
    {
        if (idList == null || idList.Count == 0 || count <= 0)
        {
            return [];
        }

        if (idList.Count < count)
        {
            throw new InsufficientQuestionException(idList.Count, count);
        }

        var list = idList.ToList();
        var random = new Random();

        for (int i = 0; i < count; i++)
        {
            int j = random.Next(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return [.. list.Take(count)];
    }

    private static TimeSpan GetRandomExpiration()
    {
        var baseExpiration = TimeSpan.FromHours(1);
        var offsetMinutes = Random.Shared.Next(-10, 10);
        return baseExpiration.Add(TimeSpan.FromMinutes(offsetMinutes));
    }
}
