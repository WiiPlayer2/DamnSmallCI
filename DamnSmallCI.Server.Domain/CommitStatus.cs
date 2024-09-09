using FunicularSwitch.Generators;
using LanguageExt;

namespace DamnSmallCI.Server.Domain;

[UnionType]
public abstract partial record CommitStatus(
    Option<string> Description = default)
{
    public record Failure_(Option<string> Description = default) : CommitStatus(Description);

    public record Pending_(Option<string> Description = default) : CommitStatus(Description);

    public record Success_(Option<string> Description = default) : CommitStatus(Description);
}
