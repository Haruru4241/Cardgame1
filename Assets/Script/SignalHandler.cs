
// SignalHandler.cs
using System;

public class SignalHandler
{
    public SignalType Signal { get; }
    public Func<object, object> Func { get; }

    public SignalHandler(SignalType signal, Func<object, object> func)
    {
        Signal = signal;
        Func = func;
    }

    public object Process(object input) => Func(input);
}