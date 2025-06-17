using System;
using System.Collections.Concurrent;

namespace mc_compiled.MCC.Compiler;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class TokenFriendlyNameAttribute(string friendlyName) : Attribute
{
    public string FriendlyName { get; } = friendlyName;
}

public static class TokenExtensions
{
    private static readonly ConcurrentDictionary<Type, string> FriendlyNameCache = new();

    public static string GetFriendlyTokenName(this Type type)
    {
        return FriendlyNameCache.GetOrAdd(type, t =>
        {
            var attribute =
                (TokenFriendlyNameAttribute) Attribute.GetCustomAttribute(t, typeof(TokenFriendlyNameAttribute));
            return attribute?.FriendlyName ?? t.Name;
        });
    }
}