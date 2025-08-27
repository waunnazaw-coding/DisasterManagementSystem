using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DisasterManagementSystem_Data.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AssistanceRequest> AssistanceRequests { get; set; }

    public virtual DbSet<DisasterEvent> DisasterEvents { get; set; }

    public virtual DbSet<DisasterReport> DisasterReports { get; set; }

    public virtual DbSet<DisasterType> DisasterTypes { get; set; }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<DonationDistribution> DonationDistributions { get; set; }

    public virtual DbSet<Impact> Impacts { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<ReliefTeam> ReliefTeams { get; set; }

    public virtual DbSet<ReliefTeamActivity> ReliefTeamActivities { get; set; }

    public virtual DbSet<ReportPhoto> ReportPhotos { get; set; }

    public virtual DbSet<RequestAssignment> RequestAssignments { get; set; }

    public virtual DbSet<User> Users { get; set; }
    
    public virtual DbSet<UserReliefTeam> UserReliefTeams { get; set; }
    
    public virtual DbSet<Resource> Resources { get; set; }
    
    public virtual DbSet<DisasterKnowledge> DisasterKnowledges { get; set; }
    
    public virtual DbSet<FinancialAllocation> FinancialAllocations { get; set; }
    
    public virtual DbSet<AllocationType> AllocationTypes { get; set; }

    public virtual DbSet<GdacsdisasterEvent> GdacsdisasterEvents { get; set; }
    public virtual DbSet<Partner> Partners { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString ,  x => x.UseNetTopologySuite());
        }
    }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseSqlServer("Server=.;Database=DisasterDb;User Id=sa;Password=waunnazaw;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Partner>(entity =>
        {
            entity.ToTable("Partner");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContactName).HasMaxLength(150);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Website).HasMaxLength(250);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

            // Remove the relationship configuration with ReportPhoto
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.LogoPublicId).HasMaxLength(255);
            entity.Property(e => e.LogoFileType).HasMaxLength(20);
        });

        modelBuilder.Entity<AssistanceRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Assistan__3214EC077EEE1782");

            entity.ToTable("AssistanceRequest");

            entity.Property(e => e.ContactName).HasMaxLength(100);
            entity.Property(e => e.ContactPhone).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DetailedAddress).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FulfilledAt).HasColumnType("datetime");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValue("Medium");
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.SupportType).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.DisasterEvent).WithMany(p => p.AssistanceRequests)
                .HasForeignKey(d => d.DisasterEventId)
                .HasConstraintName("FK__Assistanc__Disas__6754599E");

            entity.HasOne(d => d.DisasterReport).WithMany(p => p.AssistanceRequests)
                .HasForeignKey(d => d.DisasterReportId)
                .HasConstraintName("FK__Assistanc__Disas__68487DD7");

            entity.HasOne(d => d.Location).WithMany(p => p.AssistanceRequests)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK__Assistanc__Locat__6A30C649");

            entity.HasOne(d => d.User).WithMany(p => p.AssistanceRequests)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Assistanc__UserI__693CA210");
        });
        
        modelBuilder.Entity<FinancialAllocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Financia__3214EC074AFABCBE");

            entity.Property(e => e.AllocationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.DetailDescription).HasMaxLength(1000);
            entity.Property(e => e.DetailName).HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(255);

            entity.HasOne(d => d.AllocationType).WithMany(p => p.FinancialAllocations)
                .HasForeignKey(d => d.AllocationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Financial__Alloc__756D6ECB");

            entity.HasOne(d => d.Donation).WithMany(p => p.FinancialAllocations)
                .HasForeignKey(d => d.DonationId)
                .HasConstraintName("FK__Financial__Donat__74794A92");
        });

        modelBuilder.Entity<GdacsdisasterEvent>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__GDACSDis__7944C810340A7F77");

            entity.ToTable("GDACSDisasterEvent");

            entity.Property(e => e.EventId).HasMaxLength(50);
            entity.Property(e => e.EventDate).HasColumnType("datetime");
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.Impact).HasMaxLength(255);
            entity.Property(e => e.Severity).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<DisasterEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disaster__3214EC074E4F68B0");

            entity.ToTable("DisasterEvent");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Severity).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.DisasterType).WithMany(p => p.DisasterEvents)
                .HasForeignKey(d => d.DisasterTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DisasterE__Disas__3C69FB99");

            entity.HasOne(d => d.Location).WithMany(p => p.DisasterEvents)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DisasterE__Locat__3D5E1FD2");
        });

        modelBuilder.Entity<DisasterKnowledge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disaster__3214EC07FFDA8E10");

            entity.ToTable("DisasterKnowledge");

            entity.Property(e => e.ContentType).HasMaxLength(50);
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DateUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DisasterType).HasMaxLength(100);
            entity.Property(e => e.Language).HasMaxLength(50);
            entity.Property(e => e.ReferenceFrom).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Resource__3214EC071FDD38F7");

            entity.Property(e => e.DateAdded)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ResourceType).HasMaxLength(50);
            entity.Property(e => e.Url).HasMaxLength(512);

            entity.HasOne(d => d.DisasterKnowledge).WithMany(p => p.Resources)
                .HasForeignKey(d => d.DisasterKnowledgeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Resources_DisasterKnowledge");
        });
        modelBuilder.Entity<DisasterReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disaster__3214EC0712CC2368");

            entity.ToTable("DisasterReport");

            entity.Property(e => e.AddressDetail).HasMaxLength(250);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Severity).HasMaxLength(20);
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.DisasterEvent).WithMany(p => p.DisasterReports)
                .HasForeignKey(d => d.DisasterEventId)
                .HasConstraintName("FK__DisasterR__Disas__5629CD9C");

            entity.HasOne(d => d.Location).WithMany(p => p.DisasterReports)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DisasterR__Locat__5812160E");

            entity.HasOne(d => d.User).WithMany(p => p.DisasterReports)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__DisasterR__UserI__571DF1D5");
        });

        modelBuilder.Entity<DisasterType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Disaster__3214EC07A26E2A75");

            entity.ToTable("DisasterType");

            entity.Property(e => e.Category).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Donation__3214EC070BAA67E9");

            entity.ToTable("Donation");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.DateReceived)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SourceType).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
           
            entity.Property(e => e.Unit).HasMaxLength(20);

            entity.HasOne(d => d.DonorUser).WithMany(p => p.Donations)
                .HasForeignKey(d => d.DonorUserId)
                .HasConstraintName("FK__Donation__DonorU__02084FDA");
        });

        modelBuilder.Entity<DonationDistribution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Donation__3214EC074DAC6BD1");

            entity.ToTable("DonationDistribution");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DateDistributed)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DistributionNotes).HasMaxLength(255);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Allocated");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AssistanceRequest).WithMany(p => p.DonationDistributions)
                .HasForeignKey(d => d.AssistanceRequestId)
                .HasConstraintName("FK__DonationD__Assis__09A971A2");

            entity.HasOne(d => d.BeneficiaryReliefTeam).WithMany(p => p.DonationDistributions)
                .HasForeignKey(d => d.BeneficiaryReliefTeamId)
                .HasConstraintName("FK__DonationD__Benef__0A9D95DB");

            entity.HasOne(d => d.DistributedByNavigation).WithMany(p => p.DonationDistributions)
                .HasForeignKey(d => d.DistributedBy)
                .HasConstraintName("FK__DonationD__Distr__0E6E26BF");

            entity.HasOne(d => d.Donation).WithMany(p => p.DonationDistributions)
                .HasForeignKey(d => d.DonationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonationD__Donat__08B54D69");
        });

        modelBuilder.Entity<Impact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Impact__3214EC0705E812B0");

            entity.ToTable("Impact");

            entity.Property(e => e.ObjectName).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Value).HasMaxLength(100);

            entity.HasOne(d => d.DisasterEvent).WithMany(p => p.Impacts)
                .HasForeignKey(d => d.DisasterEventId)
                .HasConstraintName("FK__Impact__Disaster__6383C8BA");

            entity.HasOne(d => d.DisasterReport).WithMany(p => p.Impacts)
                .HasForeignKey(d => d.DisasterReportId)
                .HasConstraintName("FK__Impact__Disaster__6477ECF3");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3214EC07D451CCD9");

            entity.ToTable("Location");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.Geography)
                .HasColumnType("geography")
                .HasDefaultValueSql("geography::ST_GeomFromText('POINT(0 0)', 4326)");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Region).HasMaxLength(50);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC075638C179");

            entity.ToTable("Notification");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__123EB7A3");
        });

        modelBuilder.Entity<ReliefTeam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReliefTe__3214EC07F9F48AEB");

            entity.ToTable("ReliefTeam");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.ContactInfo).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.SocialMediaUrl)
                .HasMaxLength(255)
                .HasColumnName("SocialMediaURL");
            entity.Property(e => e.Specialization).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.TeamLeaderName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Location).WithMany(p => p.ReliefTeams)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("FK__ReliefTea__Locat__4F7CD00D");
        });

        modelBuilder.Entity<ReliefTeamActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReliefTe__3214EC07B02C2A73");

            entity.ToTable("ReliefTeamActivity");

            entity.Property(e => e.ActivityDate).HasColumnType("datetime");
            entity.Property(e => e.ActivityType).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DetailedAddress).HasMaxLength(500);
            entity.Property(e => e.ExpenseAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ItemsDistributed).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.PostedByNavigation).WithMany(p => p.ReliefTeamActivities)
                .HasForeignKey(d => d.PostedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReliefTeamActivity_User");

            entity.HasOne(d => d.ReliefTeam).WithMany(p => p.ReliefTeamActivities)
                .HasForeignKey(d => d.ReliefTeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReliefTeamActivity_ReliefTeam");
        });

        modelBuilder.Entity<ReportPhoto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReportPh__3214EC07EC09733F");

            entity.ToTable("ReportPhoto");

            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.FileType).HasMaxLength(20);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        
            entity.HasOne(rp => rp.DisasterEvent)
                .WithMany(de => de.ReportPhotos)
                .HasForeignKey(rp => rp.DisasterEventId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(rp => rp.DisasterReport)
                .WithMany(dr => dr.ReportPhotos)
                .HasForeignKey(rp => rp.DisasterReportId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(rp => rp.ReliefTeamActivity)
                .WithMany(rta => rta.ReportPhotos)
                .HasForeignKey(rp => rp.ReliefTeamActivityId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RequestAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RequestA__3214EC07DC3D7D10");

            entity.ToTable("RequestAssignment");

            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CompletedAt).HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValue("Medium");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Assigned");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.RequestAssignmentAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .HasConstraintName("FK__RequestAs__Assig__74AE54BC");

            entity.HasOne(d => d.AssistanceRequest).WithMany(p => p.RequestAssignments)
                .HasForeignKey(d => d.AssistanceRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RequestAs__Assis__72C60C4A");

            entity.HasOne(d => d.LastUpdatedByNavigation).WithMany(p => p.RequestAssignmentLastUpdatedByNavigations)
                .HasForeignKey(d => d.LastUpdatedBy)
                .HasConstraintName("FK__RequestAs__LastU__7A672E12");

            entity.HasOne(d => d.ReliefTeam).WithMany(p => p.RequestAssignments)
                .HasForeignKey(d => d.ReliefTeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RequestAs__Relie__73BA3083");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07DC2DE6F5");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534CA97A77C").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AuthProvider).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.ExternalId).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(20).HasDefaultValue("User"); ;
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(255);

            entity.Property(e => e.RefreshTokenExpiryTime);

         
    entity.HasMany(u => u.ReliefTeamActivities)
        .WithOne(a => a.PostedByNavigation)  // Must match navigation property name
        .HasForeignKey(a => a.PostedBy)      // Foreign key property name
        .OnDelete(DeleteBehavior.Restrict)
        .HasConstraintName("FK_User_PostedActivities");


        });
        
        modelBuilder.Entity<UserReliefTeam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserReli__3214EC07DA567340");

            entity.ToTable("UserReliefTeam");

            entity.HasOne(d => d.ReliefTeam).WithMany(p => p.UserReliefTeams)
                .HasForeignKey(d => d.ReliefTeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserRelie__Relie__40F9A68C");

            entity.HasOne(d => d.User).WithMany(p => p.UserReliefTeams)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserRelie__UserI__40058253");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Contact__3214EC07776EFDBB");

            entity.ToTable("Contact");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.SubmissionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
