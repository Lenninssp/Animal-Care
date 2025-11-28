using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Animal_Care.Models;

public partial class AnimalCare2Context : DbContext
{
    public AnimalCare2Context()
    {
    }

    public AnimalCare2Context(DbContextOptions<AnimalCare2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentType> AppointmentTypes { get; set; }

    public virtual DbSet<ClinicHour> ClinicHours { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Owner> Owners { get; set; }

    public virtual DbSet<Pet> Pets { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VetSchedule> VetSchedules { get; set; }

    public virtual DbSet<Veterinarian> Veterinarians { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AnimalCare2;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__APPOINTM__3213E83FB1E2F306");

            entity.ToTable("APPOINTMENT");

            entity.HasIndex(e => e.PetId, "IX_Appointment_Pet");

            entity.HasIndex(e => e.StartTime, "IX_Appointment_StartTime");

            entity.HasIndex(e => e.Status, "IX_Appointment_Status");

            entity.HasIndex(e => e.VetId, "IX_Appointment_Vet");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AppointmentTypeId).HasColumnName("appointment_type_id");
            entity.Property(e => e.CanceledAt)
                .HasColumnType("datetime")
                .HasColumnName("canceled_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.PetId).HasColumnName("pet_id");
            entity.Property(e => e.RecepcionistUserId).HasColumnName("recepcionist_user_id");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VetId).HasColumnName("vet_id");

            entity.HasOne(d => d.AppointmentType).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.AppointmentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointment_Type");

            entity.HasOne(d => d.Pet).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointment_Pet");

            entity.HasOne(d => d.RecepcionistUser).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.RecepcionistUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointment_Receptionist");

            entity.HasOne(d => d.Vet).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.VetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointment_Veterinarian");
        });

        modelBuilder.Entity<AppointmentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__APPOINTM__3213E83FAC46BDEA");

            entity.ToTable("APPOINTMENT_TYPE");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ClinicHour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CLINIC_H__3213E83F6CC94B16");

            entity.ToTable("CLINIC_HOURS");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CloseTime).HasColumnName("close_time");
            entity.Property(e => e.DayOfWeek)
                .HasMaxLength(10)
                .HasColumnName("day_of_week");
            entity.Property(e => e.OpenTime).HasColumnName("open_time");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MEDICAL___3213E83F865021B7");

            entity.ToTable("MEDICAL_RECORD");

            entity.HasIndex(e => e.AppointmentId, "IX_MedicalRecord_Appointment");

            entity.HasIndex(e => e.PetId, "IX_MedicalRecord_Pet");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.Diagnosis).HasColumnName("diagnosis");
            entity.Property(e => e.PetId).HasColumnName("pet_id");
            entity.Property(e => e.Treatment).HasColumnName("treatment");
            entity.Property(e => e.VetId).HasColumnName("vet_id");
            entity.Property(e => e.VisitDate)
                .HasColumnType("date")
                .HasColumnName("visit_date");

            entity.HasOne(d => d.Appointment).WithMany(p => p.MedicalRecordAppointments)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalRecord_Appointment_Id");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.MedicalRecordIdNavigation)
                .HasForeignKey<MedicalRecord>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalRecord_Appointment");

            entity.HasOne(d => d.Pet).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalRecord_Pet");

            entity.HasOne(d => d.Vet).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.VetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalRecord_Veterinarian");
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OWNER__3213E83F220EB3A4");

            entity.ToTable("OWNER");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PET__3213E83F72F7C448");

            entity.ToTable("PET");

            entity.HasIndex(e => e.OwnerId, "IX_Pet_Owner");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.Species)
                .HasMaxLength(50)
                .HasColumnName("species");

            entity.HasOne(d => d.Owner).WithMany(p => p.Pets)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pet_Owner");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ROLE__3213E83F75D1B316");

            entity.ToTable("ROLE");

            entity.HasIndex(e => e.Name, "UQ__ROLE__72E12F1B800102C2").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__USER__3213E83F170F8959");

            entity.ToTable("USER");

            entity.HasIndex(e => e.Email, "IX_User_Email");

            entity.HasIndex(e => e.RoleId, "IX_User_Role");

            entity.HasIndex(e => e.Email, "UQ__USER__AB6E616494CEDEEF").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");
        });

        modelBuilder.Entity<VetSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VET_SCHE__3213E83F8026FA34");

            entity.ToTable("VET_SCHEDULE");

            entity.HasIndex(e => e.VetId, "IX_VetSchedule_Vet");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DayOfWeek)
                .HasMaxLength(10)
                .HasColumnName("day_of_week");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.VetId).HasColumnName("vet_id");

            entity.HasOne(d => d.Vet).WithMany(p => p.VetSchedules)
                .HasForeignKey(d => d.VetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VetSchedule_Veterinarian");
        });

        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__VETERINA__B9BE370F6950497D");

            entity.ToTable("VETERINARIAN");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Specialty)
                .HasMaxLength(100)
                .HasColumnName("specialty");

            entity.HasOne(d => d.User).WithOne(p => p.Veterinarian)
                .HasForeignKey<Veterinarian>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Veterinarian_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
