using System;
using System.Threading;
using System.Threading.Tasks;
using Pluralsight.Domain.Authentication;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain;

public class LoginController
{
    private readonly ILoginHelper loginHelper;

    private readonly IUserRepository userRepository;

    private User loggedInUser;

    private UnregisteredDevice unregisteredDevice;

    private DateTimeOffset unregisteredDeviceTime;

    private Timer unregisteredDeviceRefreshTimer;

    public event Action<string> LoginFailed;

    public event Action<User> LoginSucceeded;

    public event Action<int> WaitForRegistrationTick;

    public LoginController(ILoginHelper loginHelper, IUserRepository userRepository)
    {
        this.loginHelper = loginHelper;
        this.userRepository = userRepository;
    }

    public async Task AttemptLogin(string username, string password)
    {
        LoginResult loginResult = await loginHelper.LogIn(username, password);
        if (loginResult.Success)
        {
            loggedInUser = loginResult.User;
            userRepository.Save(loggedInUser);
            RaiseLoginSuccess(loginResult.User);
        }
        else
        {
            RaiseLoginFailure(loginResult.ErrorMessage);
        }
    }

    public async Task<string> StartDeviceRegistration()
    {
        unregisteredDevice = await loginHelper.StartUnauthenticatedDevice();
        if (unregisteredDevice == null)
        {
            return null;
        }
        CancelDeviceRegistrationTimer();
        unregisteredDeviceTime = DateTimeOffset.UtcNow;
        StartDeviceRegistrationTimer();
        return unregisteredDevice.Pin;
    }

    private void StartDeviceRegistrationTimer()
    {
        unregisteredDeviceRefreshTimer = new Timer(UpdateDeviceStatus, null, TimeSpan.FromSeconds(15.0), Timeout.InfiniteTimeSpan);
    }

    public void CancelDeviceRegistrationTimer()
    {
        unregisteredDeviceRefreshTimer?.Dispose();
        unregisteredDeviceRefreshTimer = null;
    }

    private async void UpdateDeviceStatus(object state)
    {
        CancelDeviceRegistrationTimer();
        DeviceStatus status = await loginHelper.CheckDeviceStatus(unregisteredDevice.DeviceId);
        if (status == DeviceStatus.Valid)
        {
            LoginResult loginResult = await loginHelper.LoginDevice(unregisteredDevice);
            if (loginResult.Success)
            {
                userRepository.Save(loginResult.User);
                RaiseLoginSuccess(loginResult.User);
                return;
            }
            RaiseLoginFailure(loginResult.ErrorMessage);
        }
        TimeSpan timeSpan = unregisteredDevice.ValidUntil - DateTimeOffset.UtcNow;
        TimeSpan timeSpan2 = unregisteredDevice.ServerTime - unregisteredDeviceTime;
        double num = Math.Round((timeSpan - timeSpan2).TotalMinutes);
        if (status != DeviceStatus.Invalid)
        {
            RaiseRegistrationTick((int)num);
            StartDeviceRegistrationTimer();
        }
        else
        {
            RaiseRegistrationTick(-1);
        }
    }

    private void RaiseLoginSuccess(User user)
    {
        this.LoginSucceeded?.Invoke(user);
    }

    private void RaiseLoginFailure(string errorMessage)
    {
        this.LoginFailed?.Invoke(errorMessage);
    }

    private void RaiseRegistrationTick(int minutesRemaining)
    {
        this.WaitForRegistrationTick?.Invoke(minutesRemaining);
    }
}
