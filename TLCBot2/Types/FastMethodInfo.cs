namespace TLCBot2.Types;

public record FastMethodInfo(string Name, FastParameterInfo[]? Parameters, Action<object?[]?> Invoke);