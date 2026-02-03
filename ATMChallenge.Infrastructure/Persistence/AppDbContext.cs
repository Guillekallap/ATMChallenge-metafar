using ATMChallenge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATMChallenge.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Card> Cards => Set<Card>();
        public DbSet<Operation> Operations => Set<Operation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.FullName).IsRequired();
                b.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Account>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.AccountNumber).IsRequired();
                // Especifica precisión para evitar truncamiento en SQL Server
                b.Property(x => x.Balance).HasPrecision(18, 2);
                b.HasMany(x => x.Cards).WithOne(x => x.Account).HasForeignKey(x => x.AccountId);
                b.HasMany(x => x.Operations).WithOne(x => x.Account).HasForeignKey(x => x.AccountId);

                // Concurrency token
                b.Property(x => x.RowVersion).IsRowVersion();

                // Auditoría
                b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                b.Property(x => x.UpdatedAt);
            });

            modelBuilder.Entity<Card>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.CardNumber).IsRequired();
                b.Property(x => x.PinHash).IsRequired();

                b.HasIndex(x => x.CardNumber).IsUnique();

                b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                b.Property(x => x.UpdatedAt);
            });

            modelBuilder.Entity<Operation>(b =>
            {
                b.HasKey(x => x.Id);
                // Especifica precisión para los montos
                b.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
                b.Property(x => x.Timestamp).IsRequired();

                b.Property(x => x.CreatedBy);
                b.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Seed data - 5 usuarios con cuentas, tarjetas y operaciones históricas
            var baseDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Account 1 - Juan Pérez
            modelBuilder.Entity<Account>().HasData(new Account
            {
                Id = 1,
                AccountNumber = "AR12345678",
                Balance = 10000m,
                LastWithdrawal = baseDate.AddDays(29).AddHours(14),
                CreatedAt = baseDate
            });

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                FullName = "Juan Pérez",
                AccountId = 1
            });

            modelBuilder.Entity<Card>().HasData(new Card
            {
                Id = 1,
                CardNumber = "4000000000000001",
                PinHash = BCrypt.Net.BCrypt.HashPassword("1234"),
                IsBlocked = false,
                FailedPinAttempts = 0,
                AccountId = 1,
                CreatedAt = baseDate
            });

            // Account 2 - María García
            modelBuilder.Entity<Account>().HasData(new Account
            {
                Id = 2,
                AccountNumber = "AR87654321",
                Balance = 25000m,
                LastWithdrawal = baseDate.AddDays(28).AddHours(10),
                CreatedAt = baseDate
            });

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 2,
                FullName = "María García",
                AccountId = 2
            });

            modelBuilder.Entity<Card>().HasData(new Card
            {
                Id = 2,
                CardNumber = "4000000000000002",
                PinHash = BCrypt.Net.BCrypt.HashPassword("5678"),
                IsBlocked = false,
                FailedPinAttempts = 0,
                AccountId = 2,
                CreatedAt = baseDate
            });

            // Account 3 - Carlos López
            modelBuilder.Entity<Account>().HasData(new Account
            {
                Id = 3,
                AccountNumber = "AR11223344",
                Balance = 5500m,
                LastWithdrawal = baseDate.AddDays(25).AddHours(16),
                CreatedAt = baseDate
            });

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 3,
                FullName = "Carlos López",
                AccountId = 3
            });

            modelBuilder.Entity<Card>().HasData(new Card
            {
                Id = 3,
                CardNumber = "4000000000000003",
                PinHash = BCrypt.Net.BCrypt.HashPassword("9999"),
                IsBlocked = false,
                FailedPinAttempts = 0,
                AccountId = 3,
                CreatedAt = baseDate
            });

            // Account 4 - Ana Martínez
            modelBuilder.Entity<Account>().HasData(new Account
            {
                Id = 4,
                AccountNumber = "AR55667788",
                Balance = 50000m,
                LastWithdrawal = baseDate.AddDays(30).AddHours(9),
                CreatedAt = baseDate
            });

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 4,
                FullName = "Ana Martínez",
                AccountId = 4
            });

            modelBuilder.Entity<Card>().HasData(new Card
            {
                Id = 4,
                CardNumber = "4000000000000004",
                PinHash = BCrypt.Net.BCrypt.HashPassword("1111"),
                IsBlocked = false,
                FailedPinAttempts = 0,
                AccountId = 4,
                CreatedAt = baseDate
            });

            // Account 5 - Pedro Rodríguez
            modelBuilder.Entity<Account>().HasData(new Account
            {
                Id = 5,
                AccountNumber = "AR99887766",
                Balance = 15750m,
                LastWithdrawal = baseDate.AddDays(27).AddHours(11),
                CreatedAt = baseDate
            });

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 5,
                FullName = "Pedro Rodríguez",
                AccountId = 5
            });

            modelBuilder.Entity<Card>().HasData(new Card
            {
                Id = 5,
                CardNumber = "4000000000000005",
                PinHash = BCrypt.Net.BCrypt.HashPassword("2222"),
                IsBlocked = false,
                FailedPinAttempts = 0,
                AccountId = 5,
                CreatedAt = baseDate
            });

            // Seed operaciones históricas para testing de paginación
            var operations = new List<Operation>();
            int operationId = 1;

            // Juan Pérez - 15 operaciones
            var juanOperations = new[]
            {
                (baseDate.AddDays(1), 500m),
                (baseDate.AddDays(3), 1000m),
                (baseDate.AddDays(5), 250m),
                (baseDate.AddDays(7), 750m),
                (baseDate.AddDays(9), 300m),
                (baseDate.AddDays(11), 1500m),
                (baseDate.AddDays(13), 200m),
                (baseDate.AddDays(15), 800m),
                (baseDate.AddDays(17), 450m),
                (baseDate.AddDays(19), 1200m),
                (baseDate.AddDays(21), 350m),
                (baseDate.AddDays(23), 600m),
                (baseDate.AddDays(25), 900m),
                (baseDate.AddDays(27), 400m),
                (baseDate.AddDays(29), 550m)
            };

            foreach (var (date, amount) in juanOperations)
            {
                operations.Add(new Operation
                {
                    Id = operationId++,
                    AccountId = 1,
                    Type = OperationType.Withdrawal,
                    Amount = amount,
                    Timestamp = date.AddHours(10),
                    CreatedBy = "4000000000000001",
                    CreatedAt = date.AddHours(10)
                });
            }

            // María García - 12 operaciones
            var mariaOperations = new[]
            {
                (baseDate.AddDays(2), 2000m),
                (baseDate.AddDays(4), 1500m),
                (baseDate.AddDays(6), 3000m),
                (baseDate.AddDays(8), 500m),
                (baseDate.AddDays(10), 2500m),
                (baseDate.AddDays(12), 1000m),
                (baseDate.AddDays(14), 1800m),
                (baseDate.AddDays(16), 700m),
                (baseDate.AddDays(18), 2200m),
                (baseDate.AddDays(22), 1300m),
                (baseDate.AddDays(26), 900m),
                (baseDate.AddDays(28), 1100m)
            };

            foreach (var (date, amount) in mariaOperations)
            {
                operations.Add(new Operation
                {
                    Id = operationId++,
                    AccountId = 2,
                    Type = OperationType.Withdrawal,
                    Amount = amount,
                    Timestamp = date.AddHours(14),
                    CreatedBy = "4000000000000002",
                    CreatedAt = date.AddHours(14)
                });
            }

            // Carlos López - 8 operaciones
            var carlosOperations = new[]
            {
                (baseDate.AddDays(3), 300m),
                (baseDate.AddDays(6), 450m),
                (baseDate.AddDays(9), 600m),
                (baseDate.AddDays(12), 200m),
                (baseDate.AddDays(15), 800m),
                (baseDate.AddDays(18), 350m),
                (baseDate.AddDays(22), 500m),
                (baseDate.AddDays(25), 400m)
            };

            foreach (var (date, amount) in carlosOperations)
            {
                operations.Add(new Operation
                {
                    Id = operationId++,
                    AccountId = 3,
                    Type = OperationType.Withdrawal,
                    Amount = amount,
                    Timestamp = date.AddHours(16),
                    CreatedBy = "4000000000000003",
                    CreatedAt = date.AddHours(16)
                });
            }

            // Ana Martínez - 10 operaciones
            var anaOperations = new[]
            {
                (baseDate.AddDays(2), 5000m),
                (baseDate.AddDays(5), 3500m),
                (baseDate.AddDays(8), 4200m),
                (baseDate.AddDays(11), 2800m),
                (baseDate.AddDays(14), 6000m),
                (baseDate.AddDays(17), 1500m),
                (baseDate.AddDays(20), 3000m),
                (baseDate.AddDays(23), 4500m),
                (baseDate.AddDays(26), 2000m),
                (baseDate.AddDays(30), 3800m)
            };

            foreach (var (date, amount) in anaOperations)
            {
                operations.Add(new Operation
                {
                    Id = operationId++,
                    AccountId = 4,
                    Type = OperationType.Withdrawal,
                    Amount = amount,
                    Timestamp = date.AddHours(9),
                    CreatedBy = "4000000000000004",
                    CreatedAt = date.AddHours(9)
                });
            }

            // Pedro Rodríguez - 7 operaciones
            var pedroOperations = new[]
            {
                (baseDate.AddDays(4), 1000m),
                (baseDate.AddDays(8), 850m),
                (baseDate.AddDays(12), 1200m),
                (baseDate.AddDays(16), 950m),
                (baseDate.AddDays(20), 1100m),
                (baseDate.AddDays(24), 750m),
                (baseDate.AddDays(27), 900m)
            };

            foreach (var (date, amount) in pedroOperations)
            {
                operations.Add(new Operation
                {
                    Id = operationId++,
                    AccountId = 5,
                    Type = OperationType.Withdrawal,
                    Amount = amount,
                    Timestamp = date.AddHours(11),
                    CreatedBy = "4000000000000005",
                    CreatedAt = date.AddHours(11)
                });
            }

            modelBuilder.Entity<Operation>().HasData(operations);
        }
    }
}