using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FastQuiz.Common.Exceptions.Extensions;

public static class StandardErrorMetadata
{
    public static RouteGroupBuilder WithStandardErrorMetadata(this RouteGroupBuilder group)
    {
        return group.WithMetadata(
            new ProducesResponseTypeAttribute(
                typeof(ProblemDetails),
                StatusCodes.Status400BadRequest,
                "application/problem+json"
            ),
            new ProducesResponseTypeAttribute(
                typeof(ProblemDetails),
                StatusCodes.Status422UnprocessableEntity,
                "application/problem+json"
            ),
            new ProducesResponseTypeAttribute(
                typeof(ProblemDetails),
                StatusCodes.Status500InternalServerError,
                "application/problem+json"
            )
        );
    }
}
