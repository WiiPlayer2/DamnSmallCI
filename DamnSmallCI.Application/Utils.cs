namespace DamnSmallCI.Application;

public static class Utils
{
    public static DisposableWrap<T> WrapSync<T>(this T asyncDisposable)
        where T : IAsyncDisposable =>
        new(asyncDisposable, x => x.DisposeAsync().AsTask().GetAwaiter().GetResult());
}

public class DisposableWrap<T>(T value, Action<T> onDispose) : IDisposable
{
    public T Value => value;

    public void Dispose() => onDispose(value);
}
