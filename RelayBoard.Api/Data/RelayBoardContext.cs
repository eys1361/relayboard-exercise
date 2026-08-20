using Microsoft.EntityFrameworkCore;
using RelayBoard.Api.Domain;

namespace RelayBoard.Api.Data;

public class RelayBoardContext(DbContextOptions<RelayBoardContext> options) : DbContext(options)
{
    public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();
    public DbSet<DriverStatus> DriverStatuses => Set<DriverStatus>();
    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Assignment> Assignments => Set<Assignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VehicleType>(entity =>
        {
            entity.ToTable("VehicleTypes");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(16);
            entity.Property(e => e.Name).HasMaxLength(64);
            entity.HasData(
                new VehicleType { Id = VehicleTypeIds.Bike, Code = "BIKE", Name = "Bike" },
                new VehicleType { Id = VehicleTypeIds.Car, Code = "CAR", Name = "Car" },
                new VehicleType { Id = VehicleTypeIds.Van, Code = "VAN", Name = "Van" });
        });

        modelBuilder.Entity<DriverStatus>(entity =>
        {
            entity.ToTable("DriverStatuses");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(16);
            entity.Property(e => e.Name).HasMaxLength(64);
            entity.HasData(
                new DriverStatus { Id = DriverStatusIds.OffDuty, Code = "OFF_DUTY", Name = "Off duty" },
                new DriverStatus { Id = DriverStatusIds.Available, Code = "AVAILABLE", Name = "Available" },
                new DriverStatus { Id = DriverStatusIds.OnJob, Code = "ON_JOB", Name = "On job" });
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.ToTable("OrderStatuses");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(16);
            entity.Property(e => e.Name).HasMaxLength(64);
            entity.HasData(
                new OrderStatus { Id = OrderStatusIds.Open, Code = "OPEN", Name = "Open" },
                new OrderStatus { Id = OrderStatusIds.Assigned, Code = "ASSIGNED", Name = "Assigned" },
                new OrderStatus { Id = OrderStatusIds.InTransit, Code = "IN_TRANSIT", Name = "In transit" },
                new OrderStatus { Id = OrderStatusIds.Delivered, Code = "DELIVERED", Name = "Delivered" },
                new OrderStatus { Id = OrderStatusIds.Cancelled, Code = "CANCELLED", Name = "Cancelled" });
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Addresses");
            entity.Property(e => e.Line1).HasMaxLength(128);
            entity.Property(e => e.Line2).HasMaxLength(128);
            entity.Property(e => e.City).HasMaxLength(64);
            entity.Property(e => e.State).HasMaxLength(32);
            entity.Property(e => e.PostalCode).HasMaxLength(16);
            entity.Ignore(e => e.Display);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.Phone).HasMaxLength(32);
            entity.Property(e => e.Email).HasMaxLength(128);
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.ToTable("Drivers");
            entity.Property(e => e.FirstName).HasMaxLength(64);
            entity.Property(e => e.LastName).HasMaxLength(64);
            entity.Property(e => e.Phone).HasMaxLength(32);
            entity.Ignore(e => e.FullName);
            entity.HasIndex(e => e.DriverStatusId);
            entity.HasIndex(e => e.VehicleTypeId);
            entity.HasOne(e => e.VehicleType)
                .WithMany(e => e.Drivers)
                .HasForeignKey(e => e.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.DriverStatus)
                .WithMany(e => e.Drivers)
                .HasForeignKey(e => e.DriverStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.OrderStatusId);
            entity.HasIndex(e => e.AssignedDriverId);
            entity.Property(e => e.OrderNumber).HasMaxLength(32);
            entity.Property(e => e.Notes).HasMaxLength(512);
            entity.HasOne(e => e.Customer)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PickupAddress)
                .WithMany()
                .HasForeignKey(e => e.PickupAddressId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.DropoffAddress)
                .WithMany()
                .HasForeignKey(e => e.DropoffAddressId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.OrderStatus)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.OrderStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.RequiredVehicleType)
                .WithMany(e => e.RequiredByOrders)
                .HasForeignKey(e => e.RequiredVehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AssignedDriver)
                .WithMany(e => e.AssignedOrders)
                .HasForeignKey(e => e.AssignedDriverId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.ToTable("Assignments");
            entity.HasIndex(e => new { e.DriverId, e.UnassignedAt });
            entity.HasIndex(e => new { e.DriverId, e.StopSequence });
            entity.HasIndex(e => e.OrderId);
            entity.HasOne(e => e.Order)
                .WithMany(e => e.Assignments)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Driver)
                .WithMany(e => e.Assignments)
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
