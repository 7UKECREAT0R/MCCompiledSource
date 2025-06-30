using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace mc_compiled.MCC.Compiler;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class TokenFriendlyNameAttribute(string friendlyName) : Attribute
{
    public string FriendlyName { get; } = friendlyName;
}

public static class TokenExtensions
{
    private static readonly ConcurrentDictionary<Guid, string> FriendlyNameCache = new();

    /// <summary>
    ///     Retrieves the friendly name of a token based on the <see cref="TokenFriendlyNameAttribute" />
    ///     applied to the specified <see cref="Type" />. If the attribute is not present, the method defaults
    ///     to returning the name of the type.
    /// </summary>
    /// <param name="type">
    ///     The <see cref="Type" /> for which the friendly name is requested. This type may or may not have the
    ///     <see cref="TokenFriendlyNameAttribute" /> applied. Must not be <see langword="null" />.
    /// </param>
    /// <returns>
    ///     A <see cref="string" /> representing the friendly name of the token if the <paramref name="type" />
    ///     has an associated <see cref="TokenFriendlyNameAttribute" />. If the attribute is not present, the
    ///     method returns the default name of the type.
    /// </returns>
    public static string GetFriendlyTokenName(this Type type)
    {
        return FriendlyNameCache.GetOrAdd(type.GUID, _ =>
        {
            var attribute =
                (TokenFriendlyNameAttribute) Attribute.GetCustomAttribute(type, typeof(TokenFriendlyNameAttribute));
            return attribute?.FriendlyName ?? type.Name;
        });
    }

    /// <summary>
    ///     Retrieves the friendly name of a token as defined by the <see cref="TokenFriendlyNameAttribute" />
    ///     applied to the specified <see cref="Type" />. If no friendly name is defined, the method returns
    ///     <see langword="null" />.
    /// </summary>
    /// <param name="type">
    ///     The <see cref="Type" /> for which the friendly name is requested. It is expected to have a
    ///     <see cref="TokenFriendlyNameAttribute" /> applied to it. Must not be <see langword="null" />.
    /// </param>
    /// <returns>
    ///     A <see cref="string" /> representing the friendly name of the token if the <paramref name="type" />
    ///     has an associated <see cref="TokenFriendlyNameAttribute" />. Otherwise, returns <see langword="null" />.
    /// </returns>
    [CanBeNull]
    public static string GetFriendlyTokenNameOrDefault(this Type type)
    {
        if (FriendlyNameCache.TryGetValue(type.GUID, out string typeName))
            return typeName;

        var attribute =
            (TokenFriendlyNameAttribute) Attribute.GetCustomAttribute(type, typeof(TokenFriendlyNameAttribute));
        if (attribute == null)
            return null;

        FriendlyNameCache[type.GUID] = attribute.FriendlyName;
        return attribute.FriendlyName;
    }
}