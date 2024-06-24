using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain.WPF.Persistance;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly DatabaseConnectionManager connectionManager;

    public UserProfileRepository(DatabaseConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public UserProfile Load()
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        UserProfileDto userProfileDto = dbConnectionTs.QueryFirstOrDefault<UserProfileDto>("SELECT * FROM UserProfile LIMIT 1");
        if (userProfileDto == null)
        {
            return null;
        }
        return new UserProfile
        {
            Name = userProfileDto.Name,
            Email = userProfileDto.Email
        };
    }

    public void Save(UserProfile userProfile)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        var param = new { userProfile.Name, userProfile.Email };
        if (dbConnectionTs.Execute("UPDATE UserProfile SET Name=@Name, Email=@Email", param) == 0)
        {
            dbConnectionTs.Execute("INSERT INTO UserProfile (Name, Email) VALUES (@Name, @Email)", param);
        }
    }

    public void Delete()
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        dbConnectionTs.Execute("DELETE FROM UserProfile");
    }
}
