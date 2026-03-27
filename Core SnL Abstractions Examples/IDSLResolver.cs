using System.Collections.Generic;
using SeagullEngine.DSL.SeagullNoLonger.Models;
using SeagullEngine.DSL.SeagullNoLonger.Commands;

namespace SeagullEngine.DSL.SeagullNoLonger.Core
{
    /// <summary>
    /// Generic DSL resolver interface.
    /// Resolves narrative text, commands, and effects from parsed DSL tokens.
    /// </summary>
    public interface IDSLResolver
    {
        /// <summary>
        /// Resolve narrative text from DSL token.
        /// Replaces placeholders with concrete words using personality-driven selection.
        /// </summary>
        /// <param name="token">Parsed DSL token with narrative template</param>
        /// <param name="context">Resolution context with personality and word pools</param>
        /// <returns>Resolved narrative text with placeholders replaced</returns>
        string ResolveNarrative(DSLToken token, IDSLContext context);
        
        /// <summary>
        /// Resolve executable commands from DSL token.
        /// Creates command instances from command tokens.
        /// </summary>
        /// <param name="token">Parsed DSL token with command tokens</param>
        /// <param name="context">Resolution context</param>
        /// <returns>List of executable command instances</returns>
        List<ICommand> ResolveCommands(DSLToken token, IDSLContext context);
        
        /// <summary>
        /// Resolve effect tokens from DSL token.
        /// Prepares effects for application to game state.
        /// </summary>
        /// <param name="token">Parsed DSL token with effect specifications</param>
        /// <param name="context">Resolution context</param>
        /// <returns>List of effect tokens ready for application</returns>
        List<EffectToken> ResolveEffects(DSLToken token, IDSLContext context);
    }
}
