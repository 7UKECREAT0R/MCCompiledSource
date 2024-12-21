using System.Collections.Generic;
using mc_compiled.MCC.Compiler;
using mc_compiled.MCC.Functions.Types;

namespace mc_compiled.MCC.Attributes;

/// <summary>
///     Represents an attribute that can be placed on a RuntimeFunction or ScoreboardValue.
/// </summary>
public interface IAttribute
{
    /// <summary>
    ///     Returns the string used to represent this attribute in debugging contexts.
    /// </summary>
    /// <returns></returns>
    string GetDebugString();
    /// <summary>
    ///     Returns the code needed to define this attribute. It could just be the keyword.<br />
    ///     Returns null if this attribute cannot be defined with code.
    /// </summary>
    /// <returns></returns>
    string GetCodeRepresentation();

    /// <summary>
    ///     Called when this attribute is added to a value.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="causingStatement"></param>
    void OnAddedValue(ScoreboardValue value, Statement causingStatement);
    /// <summary>
    ///     Called when this attribute is added to a function.
    /// </summary>
    void OnAddedFunction(RuntimeFunction function, Statement causingStatement);

    /// <summary>
    ///     Called when the function this attribute is attached to is called. Modify the call command by writing to 'command'.
    /// </summary>
    /// <param name="function">The function being called.</param>
    /// <param name="commands">
    ///     The commands being run to call the function. Index 0 should ALWAYS be the initial command, no
    ///     matter what. It can be set to null.
    /// </param>
    /// <param name="executor">The executor in this context.</param>
    /// <param name="statement">The calling statement.</param>
    void OnCalledFunction(RuntimeFunction function,
        List<string> commands,
        Executor executor, Statement statement);
}