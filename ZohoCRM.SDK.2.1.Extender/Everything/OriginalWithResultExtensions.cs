using CSharpFunctionalExtensions;

namespace ZohoCRM.SDK_2_1.Extender.BaseTypes.Everything;

public static class OriginalWithResultExtensions
{
    public static OriginalWithSameResult<TOriginal> Create<TOriginal>(this TOriginal original, Result<TOriginal> result) => new(original, result);
    public static OriginalWithSameResult<TOriginal> CreateSuccess<TOriginal>(this TOriginal original) => new(original, Result.Success(original));
    public static OriginalWithSameResult<TOriginal> CreateFailure<TOriginal>(this TOriginal original, string error) => new(original, Result.Failure<TOriginal>(error));
}

public class OriginalWithSameResult<TOriginal> : OriginalWithResult<TOriginal, TOriginal>
{
    public OriginalWithSameResult(TOriginal original, Result<TOriginal> result) : base(original, result)
    {
    }
}

public class OriginalWithResult<TOriginal, TResult>
{
    protected OriginalWithResult(TOriginal original, Result<TResult> result)
    {
        Original = original;
        Result = result;
    }

    public TOriginal Original { get; }
    public Result<TResult> Result { get; }
}

