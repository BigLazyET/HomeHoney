using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HomeHoney.Api.Contracts.Common;

public static class ProblemDetailsExtensions
{
    public static ProblemHttpResult ToProblem(this string detail, int statusCode, string title)
        => TypedResults.Problem(new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
        });
}
