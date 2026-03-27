using SeagullEngine.DSL.SeagullNoLonger.Models;

namespace SeagullEngine.DSL.SeagullNoLonger.Core
{
    /// <summary>
    /// Generic DSL executor interface.
    /// Executes resolved DSL tokens and returns execution results.
    /// </summary>
    public interface IDSLExecutor
    {
        /// <summary>
        /// Execute a resolved DSL token with given context.
        /// Processes commands, applies effects, and returns execution results.
        /// </summary>
        /// <param name="token">Resolved DSL token containing narrative and commands</param>
        /// <param name="context">Execution context with callbacks and state</param>
        /// <returns>Execution result with narrative text, applied effects, and command results</returns>
        ExecutionResult Execute(DSLToken token, IDSLContext context);
    }
}
