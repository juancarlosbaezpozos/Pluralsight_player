using System;
using System.Net;
using System.Threading.Tasks;
using Pluralsight.Domain.Serialization;

namespace Pluralsight.Domain.Authentication;

public class LoginHelper : ILoginHelper
{
    private readonly IRestHelper restHelper;

    public LoginHelper(IRestHelper restHelper)
    {
        this.restHelper = restHelper;
    }

    public async Task<LoginResult> LogIn(string userName, string password)
    {
        RestResponse<RegisteredDevice> restResponse = await restHelper.BasicPost<RegisteredDevice, AuthenticatedRegisterRequest>("user/device/authenticated", new AuthenticatedRegisterRequest
        {
            Username = userName,
            Password = password,
            DeviceModel = "Windows Desktop"
        });
        if (restResponse.StatusCode == HttpStatusCode.OK)
        {
            return await LoginDevice(restResponse.Data);
        }
        return new LoginResult
        {
            Success = false,
            ErrorMessage = GetErrorMessage(restResponse.RawContent, restResponse.StatusCode)
        };
    }

    public async Task<LoginResult> LoginDevice(RegisteredDevice deviceResponse)
    {
        RestResponse<DeviceAuthorizationResponse> restResponse = await restHelper.BasicPost<DeviceAuthorizationResponse, RegisteredDevice>("user/authorization/" + deviceResponse.DeviceId, deviceResponse);
        if (restResponse.StatusCode == HttpStatusCode.OK)
        {
            return new LoginResult
            {
                User = new User
                {
                    AuthToken = new AuthenticationToken
                    {
                        Expiration = restResponse.Data.Expiration,
                        Jwt = restResponse.Data.Token
                    },
                    DeviceInfo = deviceResponse,
                    UserHandle = restResponse.Data.UserHandle
                },
                Success = (restResponse.StatusCode == HttpStatusCode.OK)
            };
        }
        return new LoginResult
        {
            Success = false,
            ErrorMessage = GetErrorMessage(restResponse.RawContent, restResponse.StatusCode)
        };
    }

    private string GetErrorMessage(string content, HttpStatusCode statusCode)
    {
        switch (statusCode)
        {
            case (HttpStatusCode)0:
                return "Unable to contact server.  Check your internet connection.";
            case HttpStatusCode.Unauthorized:
                {
                    string text = "Invalid username or password";
                    if (JsonExtensions.TryDeserializeObject<UnAuthorizedResponse>(content, out var typeObject))
                    {
                        return typeObject.Message ?? text;
                    }
                    return text;
                }
            default:
                return "An error occured";
        }
    }

    public async Task<UnregisteredDevice> StartUnauthenticatedDevice()
    {
        RestResponse<UnregisteredDevice> restResponse = await restHelper.BasicPost<UnregisteredDevice, UnauthenticatedRegisterRequest>("user/device/unauthenticated", new UnauthenticatedRegisterRequest
        {
            DeviceModel = "Windows Desktop"
        });
        if (restResponse.StatusCode == HttpStatusCode.OK)
        {
            return restResponse.Data;
        }
        return null;
    }

    public async Task<DeviceStatus> CheckDeviceStatus(string deviceId)
    {
        RestResponse<DeviceStatusResponse> restResponse = await restHelper.BasicGet<DeviceStatusResponse>("user/device/" + deviceId + "/status");
        if (restResponse.StatusCode == HttpStatusCode.OK)
        {
            return (DeviceStatus)Enum.Parse(typeof(DeviceStatus), restResponse.Data.Status);
        }
        if (restResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return DeviceStatus.Invalid;
        }
        return DeviceStatus.Unknown;
    }
}
