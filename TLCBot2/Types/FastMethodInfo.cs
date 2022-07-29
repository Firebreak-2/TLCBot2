namespace TLCBot2.Types;

public record FastMethodInfo(string Name, FastParameterInfo[]? Parameters, Func<object?[]?, object?> Invoke);