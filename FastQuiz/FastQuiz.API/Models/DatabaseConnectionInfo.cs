using FastQuiz.Common.Models;

namespace FastQuiz.API.Models;

public record DatabaseConnectionInfo(DatabaseType Type, string ConnectionString);
