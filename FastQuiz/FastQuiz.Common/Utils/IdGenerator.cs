using NUlid;
using NUlid.Rng;

namespace FastQuiz.Common.Utils;

public class IdGenerator : IIdGenerator
{
    private readonly MonotonicUlidRng _rng = new();

    public string GenerateUlId()
    {
        return Ulid.NewUlid(_rng).ToString();
    }
}
