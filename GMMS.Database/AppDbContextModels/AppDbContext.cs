using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GMMS.Database.AppDbContextModels;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblMember> TblMembers { get; set; }

    public virtual DbSet<TblMembership> TblMemberships { get; set; }

    public virtual DbSet<TblMembershipPlan> TblMembershipPlans { get; set; }

    public virtual DbSet<TblPayment> TblPayments { get; set; }

    public virtual DbSet<TblPaymentMethod> TblPaymentMethods { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblUserSession> TblUserSessions { get; set; }

    public virtual DbSet<TblAuditLog> TblAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblMember>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK__Tbl_Memb__0CF04B1805BB98F3");

            entity.ToTable("Tbl_Member");

            entity.HasIndex(e => e.MemberCode, "UQ__Tbl_Memb__84CA637700FA42E5").IsUnique();

            entity.Property(e => e.MemberCode).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<TblMembership>(entity =>
        {
            entity.HasKey(e => e.MembershipId).HasName("PK__Tbl_Memb__92A786791A0BD763");

            entity.ToTable("Tbl_Membership");

            entity.Property(e => e.Status).HasMaxLength(20);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Member).WithMany(p => p.TblMemberships)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Membership_Member");

            entity.HasOne(d => d.MembershipPlan).WithMany(p => p.TblMemberships)
                .HasForeignKey(d => d.MembershipPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Membership_MembershipPlan");
        });

        modelBuilder.Entity<TblMembershipPlan>(entity =>
        {
            entity.HasKey(e => e.MembershipPlanId).HasName("PK__Tbl_Memb__8E444BB63E2A6ABC");

            entity.ToTable("Tbl_MembershipPlan");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PlanName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PlanCode).HasMaxLength(50);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<TblPayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Tbl_Paym__9B556A38D39F22DB");

            entity.ToTable("Tbl_Payment");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Sspath)
                .HasMaxLength(500)
                .HasColumnName("SSPath");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Membership).WithMany(p => p.TblPayments)
                .HasForeignKey(d => d.MembershipId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Membership");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.TblPayments)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_PaymentMethod");
        });

        modelBuilder.Entity<TblPaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__Tbl_Paym__DC31C1D3D827C608");

            entity.ToTable("Tbl_PaymentMethod");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PaymentMethodCode).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        // TblUser
        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_TblUser");

            entity.ToTable("Tbl_User");

            entity.HasIndex(e => e.UserName, "UQ_TblUser_UserName").IsUnique();

            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.Role).HasMaxLength(50);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MustChangePassword).HasDefaultValue(true);
        });

        // TblUserSession
        modelBuilder.Entity<TblUserSession>(entity =>
        {
            entity.HasKey(e => e.UserSessionId).HasName("PK_TblUserSession");

            entity.ToTable("Tbl_UserSession");

            entity.Property(e => e.SessionId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.LoginTime).HasDefaultValueSql("(getdate())");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TblAuditLog
        modelBuilder.Entity<TblAuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PK_TblAuditLog");

            entity.ToTable("Tbl_AuditLog");

            entity.Property(e => e.TableName).HasMaxLength(100);
            entity.Property(e => e.RecordId).HasMaxLength(50);
            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.OldValue).HasColumnType("nvarchar(max)");
            entity.Property(e => e.NewValue).HasColumnType("nvarchar(max)");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        OnModelCreatingPartial(modelBuilder);

        modelBuilder.Entity<TblUser>().HasData(
            new TblUser
            {
                UserId = 1,
                UserName = "owner",
                PasswordHash = "$2a$11$51n3uUKJ0zfp8Suf/AH2wulPNfkwy4CqEslohD8VpYIwA5gTaOXKG",
                Role = "Owner",
                IsActive = true,
                MustChangePassword = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new TblUser
            {
                UserId = 2,
                UserName = "admin",
                PasswordHash = "$2a$11$GJjdLlC9Kb9d4LGvnYVHO.IiBoNPUTRCDS4.TR92T7bHxTveFHtkq",
                Role = "Admin",
                IsActive = true,
                MustChangePassword = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}