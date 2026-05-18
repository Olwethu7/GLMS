using Microsoft.EntityFrameworkCore;
using GLMS.Models;

namespace GLMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Client Configuration
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Contract Configuration
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Client)
                .WithMany(c => c.Contracts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasIndex(c => c.ContractReference)
                .IsUnique();

            modelBuilder.Entity<Contract>()
                .Property(c => c.ContractValue)
                .HasPrecision(18, 2);

            // ServiceRequest Configuration
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Contract)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(sr => sr.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.AmountUSD)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.AmountZAR)
                .HasPrecision(18, 2);

            // SEED DATA - Initial test data
            modelBuilder.Entity<Client>().HasData(
                new Client
                {
                    Id = 1,
                    Name = "TechMove Logistics SA",
                    Email = "admin@techmove.co.za",
                    Phone = "+27 11 123 4567",
                    Address = "123 Main Street, Johannesburg",
                    Region = "Africa",
                    TaxId = "ZA123456789",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Client
                {
                    Id = 2,
                    Name = "Global Freight Solutions",
                    Email = "contact@globalfreight.com",
                    Phone = "+1 212 555 7890",
                    Address = "456 Business Ave, New York",
                    Region = "North America",
                    TaxId = "US987654321",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 15)
                },
                new Client
                {
                    Id = 3,
                    Name = "Asia Pacific Logistics",
                    Email = "info@aplogistics.sg",
                    Phone = "+65 6789 1234",
                    Address = "789 Harbor Drive, Singapore",
                    Region = "Asia Pacific",
                    TaxId = "SG456789123",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 2, 1)
                }
            );

            modelBuilder.Entity<Contract>().HasData(
                new Contract
                {
                    Id = 1,
                    ContractReference = "CT-2024-001",
                    ClientId = 1,
                    StartDate = new DateTime(2024, 1, 1),
                    EndDate = new DateTime(2024, 12, 31),
                    Status = ContractStatus.Active,
                    ServiceLevel = ServiceLevel.Premium,
                    ContractValue = 150000,
                    AutoRenew = true,
                    NoticePeriodDays = 30,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Contract
                {
                    Id = 2,
                    ContractReference = "CT-2024-002",
                    ClientId = 2,
                    StartDate = new DateTime(2023, 1, 1),
                    EndDate = new DateTime(2023, 12, 31),
                    Status = ContractStatus.Expired,
                    ServiceLevel = ServiceLevel.Standard,
                    ContractValue = 75000,
                    AutoRenew = false,
                    NoticePeriodDays = 60,
                    CreatedAt = new DateTime(2023, 1, 1)
                },
                new Contract
                {
                    Id = 3,
                    ContractReference = "CT-2024-003",
                    ClientId = 3,
                    StartDate = new DateTime(2024, 3, 1),
                    EndDate = new DateTime(2024, 8, 31),
                    Status = ContractStatus.Active,
                    ServiceLevel = ServiceLevel.Enterprise,
                    ContractValue = 500000,
                    AutoRenew = true,
                    NoticePeriodDays = 90,
                    CreatedAt = new DateTime(2024, 3, 1)
                }
            );
        }
    }
}