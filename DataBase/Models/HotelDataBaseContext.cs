using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataBase.Models;

public partial class HotelDataBaseContext : DbContext
{
    public HotelDataBaseContext()
    {
    }

    public HotelDataBaseContext(DbContextOptions<HotelDataBaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<RoomType> RoomTypes { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<Reservation> Reservation { get; set; }
    public virtual DbSet<Room> Rooms { get; set; }
    public virtual DbSet<Service> Services { get; set; }
    public virtual DbSet<ServiceUsage> ServiceUsage { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Add your database connection string if not configured in Startup.cs
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.FirstName).HasMaxLength(25).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(25).IsRequired();
            entity.Property(e => e.Birthday).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(15).IsUnicode(false).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PassportNumber).HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeesId);

            entity.Property(e => e.EmployeesId).HasColumnName("EmployeesID");
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Position).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Birthday).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(15).IsUnicode(false).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(50).IsRequired();
            entity.Property(e => e.HireDate).IsRequired();
            entity.Property(e => e.ResidencePlace).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Education).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Salary).HasColumnType("decimal(10, 2)").IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(e => e.TypeId);

            entity.Property(e => e.TypeId).HasColumnName("TypeID");
            entity.Property(e => e.TypeName).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(7, 2)").IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Capacity).IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId);

            entity.Property(e => e.RoomId).HasColumnName("RoomID");
            entity.Property(e => e.RoomNumber).IsRequired();
            entity.Property(e => e.IsAvailable).IsRequired();

            entity.HasOne(d => d.RoomType)
                .WithMany(p => p.Rooms)
                .HasForeignKey(d => d.RoomTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rooms_RoomTypes");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.ReservationId);

            entity.Property(e => e.ReservationId).HasColumnName("ReservationID");
            entity.Property(e => e.CheckInDate).IsRequired();
            entity.Property(e => e.CheckOutDate).IsRequired();
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)").IsRequired();

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.Reservations)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Reservation_Customer");

            entity.HasOne(d => d.Room)
                .WithMany(p => p.Reservations)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Reservation_Room");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServicesId);

            entity.Property(e => e.ServicesId).HasColumnName("ServicesID");
            entity.Property(e => e.ServicesName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)").IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
        });

        modelBuilder.Entity<ServiceUsage>(entity =>
        {
            entity.HasKey(e => e.UsageId);

            entity.Property(e => e.UsageId).HasColumnName("UsageID");

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.ServiceUsages)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ServiceUsage_Customer");

            entity.HasOne(d => d.Services)
                .WithMany(p => p.ServiceUsages)
                .HasForeignKey(d => d.ServicesId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ServiceUsage_Service");

            entity.HasOne(d => d.Employee)
                .WithMany(p => p.ServiceUsages)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ServiceUsage_Employee");
        });

        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

