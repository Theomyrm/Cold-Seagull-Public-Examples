using SeagullEngine.DSL.SeagullNoLonger.Models;
using SeagullEngine.DSL.SeagullNoLonger.Commands;

namespace SeagullEngine.DSL.SeagullNoLonger.Core
{
    /// <summary>
    /// Generic command factory interface for DSL.
    /// Creates command instances from parsed tokens.
    /// Supports extensible command type registration.
    /// </summary>
    public interface ICommandFactory
    {
        /// <summary>
        /// Create command instance from parsed token.
        /// Factory resolves command type and instantiates appropriate command class.
        /// </summary>
        /// <param name="token">Parsed command token with type and arguments</param>
        /// <param name="context">DSL context for command configuration</param>
        /// <returns>Executable command instance or NullCommand if type unknown</returns>
        ICommand Create(CommandToken token, IDSLContext context);
    }
}
