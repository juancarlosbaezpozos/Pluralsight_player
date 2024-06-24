using System.Threading.Tasks;

namespace Pluralsight.Domain.Authentication;

public interface ILoginHelper
{
	Task<LoginResult> LogIn(string userName, string password);

	Task<UnregisteredDevice> StartUnauthenticatedDevice();

	Task<DeviceStatus> CheckDeviceStatus(string deviceId);

	Task<LoginResult> LoginDevice(RegisteredDevice deviceResponse);
}
