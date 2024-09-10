using LanguageExt.Common;
using LanguageExt.Effects.Traits;

namespace DamnSmallCi.Utils;

public static class EffExtensions
{
    public static Aff<RT, A> OnError<RT, A>(this Aff<RT, A> aff, Func<Error, Aff<RT, Unit>> fn)
        where RT : struct, HasCancel<RT>
        => aff.IfFailAff(error => fn(error)
            .Bind(x => FailAff<A>(error)));
}
