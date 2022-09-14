using System;
using CSharpFunctionalExtensions;

namespace ZohoCRM.SDK_2_1.Extender.BaseTypes.Everything;

public static class ResultExtensions
{
    public static Result<TSuccess> Switch<T, TSuccess>(this Result<T> resultT, Func<T, TSuccess> successFunc, Func<string, string> failureFunc) =>
        resultT.IsSuccess switch
        {
            true => successFunc(resultT.Value),
            _ => resultT.MapError(failureFunc)
                .ConvertFailure<TSuccess>()
        };
}