namespace DamnSmallCI.Server.Application;

public interface IServiceRuntime<RT>
{
    RT Runtime { get; }
}
