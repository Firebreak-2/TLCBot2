namespace TLCBot2.Types;

public struct DiscordActionResult<T>
{
    public T Value;
    public Exception? Error = null;
    public bool Success => Error is null;
    public string[] Arguments;

    public DiscordActionResult(T value, params string[] arguments)
    {
        Value = value;
        Arguments = arguments;
    }

    public DiscordActionResult(Exception error)
    {
        Error = error;
        Value = default!;
        Arguments = Array.Empty<string>();
    }

    public static DiscordActionResult<T> From(Func<T> action)
    {
        try
        {
            return new DiscordActionResult<T>(action());
        }
        catch (Exception e)
        {
            return new DiscordActionResult<T>(e);
        }
    }
}