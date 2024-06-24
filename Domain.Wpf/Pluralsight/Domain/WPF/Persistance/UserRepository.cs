using System;
using Pluralsight.Domain.Authentication;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain.WPF.Persistance;

public class UserRepository : IUserRepository
{
    private DatabaseConnectionManager connectionManager;

    private static User User { get; set; }

    public UserRepository(DatabaseConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public User Load()
    {
        if (User != null)
        {
            return User;
        }
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        string sql = "Select * from User limit 1";
        UserDto userDto = dbConnectionTs.QuerySingleOrDefault<UserDto>(sql);
        if (userDto == null)
        {
            return null;
        }
        User = new User
        {
            AuthToken = new AuthenticationToken
            {
                Expiration = userDto.JwtExpiration.ParseWithDefault(DateTimeOffset.MinValue),
                Jwt = userDto.Jwt
            },
            DeviceInfo = new RegisteredDevice
            {
                DeviceId = userDto.DeviceId,
                RefreshToken = userDto.RefreshToken
            },
            UserHandle = userDto.UserHandle
        };
        return User;
    }

    public void Save(User user)
    {
        User = user;
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        string sql = "INSERT INTO User (Jwt, JwtExpiration, DeviceId, RefreshToken, UserHandle) VALUES (@Jwt, @JwtExpiration, @DeviceId, @RefreshToken, @UserHandle)";
        string sql2 = "UPDATE User SET Jwt=@Jwt, JwtExpiration=@JwtExpiration, DeviceId=@DeviceId, RefreshToken=@RefreshToken, UserHandle=@UserHandle";
        var param = new
        {
            Jwt = user.AuthToken.Jwt,
            JwtExpiration = user.AuthToken.Expiration.ToDatabaseFormat(),
            DeviceId = user.DeviceInfo.DeviceId,
            RefreshToken = user.DeviceInfo.RefreshToken,
            UserHandle = user.UserHandle
        };
        if (dbConnectionTs.Execute(sql2, param) == 0)
        {
            dbConnectionTs.Execute(sql, param);
        }
    }

    public void Delete()
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        string sql = "DELETE FROM User";
        dbConnectionTs.Execute(sql);
        User = null;
    }
}
