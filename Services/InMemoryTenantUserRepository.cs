using DemoDeck.Auth.Api.Models;

namespace DemoDeck.Auth.Api.Services
{
    public class InMemoryTenantUserRepository : ITenantUserRepository
    {
        private readonly List<TenantUser> _users = new();

        public InMemoryTenantUserRepository()
        {
            SeedTestUsers();
        }

        private void SeedTestUsers()
        {
            var users = new List<TenantUser>();
            int userId = 1;

            // ACME Corporation Users (tnt_acme001)
            users.AddRange(new[]
            {
                new TenantUser
                {
                    Id = userId++,
                    Username = "john.doe",
                    Email = "john.doe@acme.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_acme001",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-180)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "sarah.johnson",
                    Email = "sarah.johnson@acme.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_acme001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-150)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "mike.wilson",
                    Email = "mike.wilson@acme.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_acme001",
                    Role = "Manager",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-120)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "lisa.brown",
                    Email = "lisa.brown@acme.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_acme001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-90)
                }
            });

            // GlobalX Industries Users (tnt_globalx001)
            users.AddRange(new[]
            {
                new TenantUser
                {
                    Id = userId++,
                    Username = "alex.smith",
                    Email = "alex.smith@globalx.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_globalx001",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-160)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "emma.davis",
                    Email = "emma.davis@globalx.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_globalx001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-140)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "james.taylor",
                    Email = "james.taylor@globalx.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_globalx001",
                    Role = "Manager",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-100)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "maria.garcia",
                    Email = "maria.garcia@globalx.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_globalx001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-80)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "robert.lee",
                    Email = "robert.lee@globalx.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_globalx001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-60)
                }
            });

            // Initech LLC Users (tnt_initech001)
            users.AddRange(new[]
            {
                new TenantUser
                {
                    Id = userId++,
                    Username = "peter.gibbons",
                    Email = "peter.gibbons@initech.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_initech001",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-75)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "samir.nagheenanajar",
                    Email = "samir.n@initech.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_initech001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-65)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "michael.bolton",
                    Email = "michael.bolton@initech.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_initech001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-55)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "milton.waddams",
                    Email = "milton.waddams@initech.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_initech001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                }
            });

            // Umbrella Corp Users (tnt_umbrella001)
            users.AddRange(new[]
            {
                new TenantUser
                {
                    Id = userId++,
                    Username = "albert.wesker",
                    Email = "albert.wesker@umbrella.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_umbrella001",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-35)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "ada.wong",
                    Email = "ada.wong@umbrella.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_umbrella001",
                    Role = "Manager",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "william.birkin",
                    Email = "william.birkin@umbrella.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_umbrella001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "annette.birkin",
                    Email = "annette.birkin@umbrella.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_umbrella001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new TenantUser
                {
                    Id = userId++,
                    Username = "spencer.parks",
                    Email = "spencer.parks@umbrella.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    TenantId = "tnt_umbrella001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }
            });

            _users.AddRange(users);
        }

        public async Task<TenantUser?> GetUserByUsernameAsync(string username, string tenantId)
        {
            await Task.Delay(10); // Simulate async operation
            return _users.FirstOrDefault(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
                u.TenantId == tenantId && 
                u.IsActive);
        }

        public async Task<TenantUser?> GetUserByIdAsync(int userId)
        {
            await Task.Delay(10);
            return _users.FirstOrDefault(u => u.Id == userId && u.IsActive);
        }

        public async Task<bool> CreateUserAsync(TenantUser user)
        {
            await Task.Delay(10);
            user.Id = _users.Max(u => u.Id) + 1;
            _users.Add(user);
            return true;
        }

        public async Task<bool> UpdateUserAsync(TenantUser user)
        {
            await Task.Delay(10);
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser == null) return false;

            existingUser.LastLoginAt = user.LastLoginAt;
            existingUser.IsActive = user.IsActive;
            return true;
        }

        public async Task<List<TenantUser>> GetUsersByTenantAsync(string tenantId)
        {
            await Task.Delay(10);
            return _users.Where(u => u.TenantId == tenantId && u.IsActive).ToList();
        }
    }
}