using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transmittal.Requesters;
public interface IParameterGuidRequester
{
    void ParameterComplete(string variableName, string parameterGuid);
}
