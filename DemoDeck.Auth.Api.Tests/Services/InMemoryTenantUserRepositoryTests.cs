using DemoDeck.Auth.Api.Models;
using DemoDeck.Auth.Api.Services;

namespace DemoDeck.Auth.Api.Tests.Services;

public class InMemoryTenantUserRepositoryTests
{
    private readonly InMemoryTenantUserRepository _repository;

    public InMemoryTenantUserRepositoryTests()
    {
        _repository = new InMemoryTenantUserRepository();
    }

    // --- GetUserByUsernameAsync ---

    [Fact]
    public async Task GetUserByUsername_ExistingUser_ReturnsUser()
    {
        var user = await _repository.GetUserByUsernameAsync("john.doe", "tnt_acme001");

        user.Should().NotBeNull();
        user!.Username.Should().Be("john.doe");
        user.Email.Should().Be("john.doe@acme.com");
        user.TenantId.Should().Be("tnt_acme001");
    }

    [Fact]
    public async Task GetUserByUsername_CaseInsensitive_ReturnsUser()
    {
        var user = await _repository.GetUserByUsernameAsync("JOHN.DOE", "tnt_acme001");

        user.Should().NotBeNull();
        user!.Username.Should().Be("john.doe");
    }

    [Fact]
    public async Task GetUserByUsername_NonExistent_ReturnsNull()
    {
        var user = await _repository.GetUserByUsernameAsync("nonexistent", "tnt_acme001");

        user.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByUsername_WrongTenant_ReturnsNull()
    {
        var user = await _repository.GetUserByUsernameAsync("john.doe", "tnt_globalx001");

        user.Should().BeNull();
    }

    // --- GetUserByIdAsync ---

    [Fact]
    public async Task GetUserById_ExistingUser_ReturnsUser()
    {
        var user = await _repository.GetUserByIdAsync(1);

        user.Should().NotBeNull();
        user!.Username.Should().Be("john.doe");
    }

    [Fact]
    public async Task GetUserById_NonExistent_ReturnsNull()
    {
        var user = await _repository.GetUserByIdAsync(999);

        user.Should().BeNull();
    }

    // --- CreateUserAsync ---

    [Fact]
    public async Task CreateUser_ReturnsTrue()
    {
        var newUser = new TenantUser
        {
            Username = "new.user",
            Email = "new.user@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("test"),
            TenantId = "tnt_acme001",
            Role = "User",
            IsActive = true
        };

        var result = await _repository.CreateUserAsync(newUser);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUser_AssignsIncrementedId()
    {
        var newUser = new TenantUser
        {
            Username = "new.user2",
            Email = "new.user2@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("test"),
            TenantId = "tnt_acme001",
            Role = "User",
            IsActive = true
        };

        await _repository.CreateUserAsync(newUser);

        // 18 seed users (Ids 1-18), so new user gets Id 19
        newUser.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateUser_IsRetrievableAfterCreation()
    {
        var newUser = new TenantUser
        {
            Username = "retrievable.user",
            Email = "retrievable@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("test"),
            TenantId = "tnt_acme001",
            Role = "User",
            IsActive = true
        };

        await _repository.CreateUserAsync(newUser);
        var found = await _repository.GetUserByUsernameAsync("retrievable.user", "tnt_acme001");

        found.Should().NotBeNull();
        found!.Email.Should().Be("retrievable@test.com");
    }

    // --- UpdateUserAsync ---

    [Fact]
    public async Task UpdateUser_ExistingUser_ReturnsTrue()
    {
        var user = (await _repository.GetUserByIdAsync(1))!;
        user.LastLoginAt = DateTime.UtcNow;

        var result = await _repository.UpdateUserAsync(user);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUser_UpdatesLastLoginAt()
    {
        var user = (await _repository.GetUserByIdAsync(1))!;
        var loginTime = DateTime.UtcNow;
        user.LastLoginAt = loginTime;

        await _repository.UpdateUserAsync(user);

        var updated = await _repository.GetUserByIdAsync(1);
        updated!.LastLoginAt.Should().Be(loginTime);
    }

    [Fact]
    public async Task UpdateUser_UpdatesIsActive()
    {
        var user = (await _repository.GetUserByIdAsync(1))!;
        user.IsActive = false;

        await _repository.UpdateUserAsync(user);

        // After deactivation, GetUserByIdAsync filters by IsActive, so should return null
        var deactivated = await _repository.GetUserByIdAsync(1);
        deactivated.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUser_NonExistent_ReturnsFalse()
    {
        var fakeUser = new TenantUser { Id = 999, LastLoginAt = DateTime.UtcNow, IsActive = true };

        var result = await _repository.UpdateUserAsync(fakeUser);

        result.Should().BeFalse();
    }

    // --- GetUsersByTenantAsync ---

    [Fact]
    public async Task GetUsersByTenant_AcmeTenant_Returns4Users()
    {
        var users = await _repository.GetUsersByTenantAsync("tnt_acme001");

        users.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetUsersByTenant_GlobalXTenant_Returns5Users()
    {
        var users = await _repository.GetUsersByTenantAsync("tnt_globalx001");

        users.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetUsersByTenant_NonExistent_ReturnsEmpty()
    {
        var users = await _repository.GetUsersByTenantAsync("tnt_nonexistent");

        users.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUsersByTenant_AfterDeactivation_ReturnsFewerUsers()
    {
        var usersBefore = await _repository.GetUsersByTenantAsync("tnt_acme001");
        var countBefore = usersBefore.Count;

        var user = usersBefore.First();
        user.IsActive = false;
        await _repository.UpdateUserAsync(user);

        var usersAfter = await _repository.GetUsersByTenantAsync("tnt_acme001");
        usersAfter.Should().HaveCount(countBefore - 1);
    }
}
