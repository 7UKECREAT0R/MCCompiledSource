using System.Collections.Generic;
using System.Linq;

namespace mc_compiled.MCC.Functions.Types;

/// <summary>
///     Represents a function that can be called and fully evaluated at compile-time.
/// </summary>
public abstract class CompiletimeFunction : Function
{
    public readonly string aliasedName; // user-facing name (keyword)
    public readonly string documentation;
    public readonly string name; // name used internally.
    private readonly List<CompiletimeFunctionParameter> parameters;
    public readonly string returnType; // the type-keyword of the value this returns, or null.

    /// <summary>
    ///     Creates a new compile-time function.
    /// </summary>
    /// <param name="aliasedName">The user-facing name of the function.</param>
    /// <param name="name">The internal name of the function.</param>
    /// <param name="returnType">The type-keyword of the value this returns, or null.</param>
    /// <param name="documentation">The documentation of this function.</param>
    public CompiletimeFunction(string aliasedName, string name, string returnType, string documentation)
    {
        this.aliasedName = aliasedName;
        this.name = name;
        this.returnType = returnType;
        this.documentation = documentation;
        this.parameters = [];
    }

    public override string Keyword => this.aliasedName;
    public override string Returns => this.returnType;
    public override string Documentation => this.documentation;
    public override FunctionParameter[] Parameters => this.parameters.Cast<FunctionParameter>().ToArray();
    public override int ParameterCount => this.parameters.Count;
    public override string[] Aliases => null;
    public override int Importance => 2; // most important. always prefer compile-time.
    public override bool ImplicitCall => false;
    public override bool AdvertiseOverLSP => true;
    /// <summary>
    ///     Adds a compile-time parameter to this function.
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns>This object for chaining.</returns>
    public CompiletimeFunction AddParameter(CompiletimeFunctionParameter parameter)
    {
        this.parameters.Add(parameter);
        return this;
    }
    /// <summary>
    ///     Adds multiple compile-time parameters to this function.
    /// </summary>
    /// <param name="newParameters"></param>
    /// <returns>This object for chaining.</returns>
    public CompiletimeFunction AddParameters(IEnumerable<CompiletimeFunctionParameter> newParameters)
    {
        this.parameters.AddRange(newParameters);
        return this;
    }
    /// <summary>
    ///     Adds multiple compile-time parameters to this function.
    /// </summary>
    /// <param name="newParameters"></param>
    /// <returns>This object for chaining.</returns>
    public CompiletimeFunction AddParameters(params CompiletimeFunctionParameter[] newParameters)
    {
        this.parameters.AddRange(newParameters);
        return this;
    }

    public override int GetHashCode()
    {
        return (this.name ?? this.aliasedName).GetHashCode();
    }
}