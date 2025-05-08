using System;
using System.Collections.Generic;
using EmployeeBOApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeBOApp.Data
{
    public partial class EmployeeDatabaseContext : DbContext
    {
        public EmployeeDatabaseContext()
        {
        }

        public EmployeeDatabaseContext(DbContextOptions<EmployeeDatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<EmployeeInformation> EmployeeInformations { get; set; }
        public virtual DbSet<ProjectInformation> ProjectInformations { get; set; }
        public virtual DbSet<TicketingTable> TicketingTables { get; set; }
        public virtual DbSet<Login> Logins { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer("Server=DESKTOP-UUFH1U7\\SQLEXPRESS;Database=EmployeeDatabase;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeInformation>(entity =>
            {
                entity.HasKey(e => e.EmpId).HasName("PK__Employee__AF2DBA79231D8271");

                entity.ToTable("EmployeeInformation");

                entity.Property(e => e.EmpId)
                    .HasMaxLength(100)
                    .HasColumnName("EmpID");
                entity.Property(e => e.EmpName).HasMaxLength(100);
                entity.Property(e => e.ProjectId)
                    .HasMaxLength(100)
                    .HasColumnName("ProjectID");

                entity.HasOne(d => d.Project).WithMany(p => p.EmployeeInformations)
                    .HasForeignKey(d => d.ProjectId)
                    .HasConstraintName("FK__EmployeeI__Proje__4CA06362");
            });

            modelBuilder.Entity<ProjectInformation>(entity =>
            {
                entity.HasKey(e => e.ProjectId).HasName("PK__ProjectI__761ABED07F7F21B5");

                entity.ToTable("ProjectInformation");

                entity.Property(e => e.ProjectId)
                    .HasMaxLength(100)
                    .HasColumnName("ProjectID");
                entity.Property(e => e.DepartmentID)
                   .HasMaxLength(100)
                   .HasColumnName("DepartmentID");
                entity.Property(e => e.Dm)
                    .HasMaxLength(100)
                    .HasColumnName("DM");
                entity.Property(e => e.DmemailId)
                    .HasMaxLength(100)
                    .HasColumnName("DMEmailID");
                entity.Property(e => e.Pm)
                    .HasMaxLength(100)
                    .HasColumnName("PM");
                entity.Property(e => e.PmemailId)
                    .HasMaxLength(100)
                    .HasColumnName("PMEmailID");
                entity.Property(e => e.ProjectName).HasMaxLength(100);
                entity.Property(e => e.ShortProjectName).HasMaxLength(50);
            });

            modelBuilder.Entity<TicketingTable>(entity =>
            {
                entity.HasKey(e => e.TicketingId).HasName("PK__Ticketin__51D2E4BDCF844E5A");

                entity.ToTable("TicketingTable");

                entity.Property(e => e.TicketingId).HasColumnName("TicketingID");
                entity.Property(e => e.ApprovedBy).HasMaxLength(100);
                entity.Property(e => e.EmpId)
                    .HasMaxLength(100)
                    .HasColumnName("EmpID");
                entity.Property(e => e.RequestedBy).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(d => d.Emp).WithMany(p => p.TicketingTables)
                    .HasForeignKey(d => d.EmpId)
                    .HasConstraintName("FK__Ticketing__EmpID__4F7CD00D");
            });

            modelBuilder.Entity<Login>(entity =>
            {
                entity.HasKey(e => e.EmailId).HasName("PK_Login");

                entity.ToTable("Login");

                entity.Property(e => e.EmailId)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .IsRequired();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
