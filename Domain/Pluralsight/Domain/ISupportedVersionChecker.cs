using System.Threading.Tasks;

namespace Pluralsight.Domain;

public interface ISupportedVersionChecker
{
    Task<ApiStatus> CheckApiVersionStatus();
}
