using NUlid;
using NUlid.Rng;

namespace FastQuiz.Common.Utils;

public class IdGenerator : IIdGenerator
{
    private readonly MonotonicUlidRng _rng = new();
    private readonly object _sync = new();
    private DateTimeOffset _lastTimestamp = DateTimeOffset.MinValue;

    public string GenerateUlId()
    {
        lock (_sync)
        {
            var now = DateTimeOffset.UtcNow;
            if (now < _lastTimestamp)
            {
                now = _lastTimestamp;
            }
            else
            {
                _lastTimestamp = now;
            }

            return Ulid.NewUlid(now, _rng).ToString();
        }
    }
}
