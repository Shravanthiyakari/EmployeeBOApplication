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
        public virtual DbSet<Bgvmap> Bgvmaps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeInformation>(entity =>
            {
                entity.HasKey(e => e.EmpId).HasName("PK__Employee__AF2DBA79231D8271");

                entity.ToTable("EmployeeInformation");

                entity.Property(e => e.EmpId)
                    .HasMaxLength(100)
                    .HasColumnName("EmpID");

                entity.Property(e => e.EmpName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Deallocation)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(e => e.ProjectId)
                    .HasMaxLength(100)
                    .HasColumnName("ProjectID");

                entity.Property(e => e.BGVMappingId)
                    .HasColumnName("BGVMappingId");

                // Foreign key to ProjectInformation
                entity.HasOne(d => d.Project)
                    .WithMany(p => p.EmployeeInformations)
                    .HasForeignKey(d => d.ProjectId)
                    .HasConstraintName("FK__EmployeeI__Proje__4CA06362");

                // Foreign key to Bgvmap
                entity.HasOne(d => d.BgvMap)
                    .WithMany(p => p.EmployeeInformations)
                    .HasForeignKey(d => d.BGVMappingId)
                    .HasConstraintName("FK_EmployeeInformation_BGVMap");
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
                entity.Property(e => e.BGVId)
                    .HasMaxLength(100)
                    .HasColumnName("BGVId");

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

            modelBuilder.Entity<Bgvmap>(entity =>
            {
                entity.HasKey(e => e.BGVMappingId)
                    .HasName("PK__Bgvmap__C7C22B0B4A7F4A20");

                entity.ToTable("Bgvmap");

                entity.Property(e => e.BGVMappingId)
                    .HasColumnName("BGVMappingId")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.EmpId)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(true);  // nvarchar is Unicode

                entity.Property(e => e.BGVId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasIndex(e => e.BGVId)
                    .IsUnique()
                    .HasDatabaseName("UQ_Bgvmap_BGVId");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime2")
                    .IsRequired();
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
