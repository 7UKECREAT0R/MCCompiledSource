namespace mc_compiled.Modding.Behaviors;

/// <summary>
///     A subject to run an event on.
/// </summary>
public enum EventSubject
{
    /// <summary>
    ///     The block involved with the interaction.
    /// </summary>
    block,
    /// <summary>
    ///     The damaging actor involved with the interaction.
    /// </summary>
    damager,
    /// <summary>
    ///     The other member of an interaction, not the caller.
    /// </summary>
    other,
    /// <summary>
    ///     The caller's current parent.
    /// </summary>
    parent,
    /// <summary>
    ///     The player involved with the interaction.
    /// </summary>
    player,
    /// <summary>
    ///     The entity or object calling the test.
    /// </summary>
    self,
    /// <summary>
    ///     The caller's current target.
    /// </summary>
    target
}