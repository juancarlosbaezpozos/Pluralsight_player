using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Shell;

public interface ISingleInstanceApp
{
	Task<bool> SignalExternalCommandLineArgs(IList<string> args);
}
