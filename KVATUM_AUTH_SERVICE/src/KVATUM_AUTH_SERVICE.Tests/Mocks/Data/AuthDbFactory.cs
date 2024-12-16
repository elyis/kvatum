using KVATUM_AUTH_SERVICE.Core.Entities.Models;
using KVATUM_AUTH_SERVICE.Core.Enums;
using KVATUM_AUTH_SERVICE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KVATUM_AUTH_SERVICE.Tests.Mocks
{
    public class AuthDbFactory
    {
        public static AuthDbContext Create(string name)
        {
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            var context = new AuthDbContext(options);
            var accounts = GenerateAccounts();
            var unverifiedAccounts = GenerateUnverifiedAccounts();
            var sessions = GenerateAccountSessions(accounts);

            context.Accounts.AddRange(accounts);
            context.UnverifiedAccounts.AddRange(unverifiedAccounts);
            context.AccountSessions.AddRange(sessions);

            context.SaveChanges();
            return context;
        }

        private static IEnumerable<Account> GenerateAccounts()
        {
            return new List<Account>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "test@test.com",
                    Nickname = "test",
                    PasswordHash = "hash",
                    Role = AccountRole.User.ToString(),
                    CreatedAt = DateTime.UtcNow,
                },

                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "test2@test.com",
                    Nickname = "test2",
                    PasswordHash = "hash",
                    Role = AccountRole.User.ToString(),
                    CreatedAt = DateTime.UtcNow,
                },

                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "test3@test.com",
                    Nickname = "test3",
                    PasswordHash = "hash",
                    Role = AccountRole.User.ToString(),
                    CreatedAt = DateTime.UtcNow,
                },

                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "test4@test.com",
                    Nickname = "test4",
                    PasswordHash = "hash",
                    Role = AccountRole.User.ToString(),
                    CreatedAt = DateTime.UtcNow,
                },
            };
        }

        private static IEnumerable<AccountSession> GenerateAccountSessions(IEnumerable<Account> accounts)
        {
            return accounts.Select(account => new AccountSession
            {
                Id = Guid.NewGuid(),
                Ip = "localhost",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
                Token = Guid.NewGuid().ToString(),
                TokenValidBefore = DateTime.UtcNow.AddHours(1),
                Account = account
            });
        }

        private static IEnumerable<UnverifiedAccount> GenerateUnverifiedAccounts()
        {
            return new List<UnverifiedAccount>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "unverified@gmail.com",
                    Nickname = "unverified",
                    Password = "password",
                    VerificationCode = "123",
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "unverified2@gmail.com",
                    Nickname = "unverified2",
                    Password = "password2",
                    VerificationCode = "123",
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "unverified3@gmail.com",
                    Nickname = "unverified3",
                    Password = "password3",
                    VerificationCode = "123",
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "unverified4@gmail.com",
                    Nickname = "unverified4",
                    Password = "password4",
                    VerificationCode = "123",
                    CreatedAt = DateTime.UtcNow
                },

                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "test4@test.com",
                    Nickname = "test4",
                    Password = "test4",
                    VerificationCode = "123",
                    CreatedAt = DateTime.UtcNow,
                }
            };
        }
    }
}
