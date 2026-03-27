using System.Collections.Generic;
using SeagullEngine.DSL.SeagullNoLonger.Models;

namespace SeagullEngine.DSL.SeagullNoLonger.Core
{
    public interface IDSLParser
    {
        DSLToken Parse(string dslText);
        bool Validate(string dslText);
        List<string> GetErrors(string dslText);
    }
}
