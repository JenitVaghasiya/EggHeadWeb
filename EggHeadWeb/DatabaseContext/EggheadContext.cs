namespace EggHeadWeb.DatabaseContext
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class EggheadContext : DbContext
    {
        public EggheadContext()
            : base("name=EggheadContext")
        {
        }

        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<AdminEmailTemplate> AdminEmailTemplates { get; set; }
        public virtual DbSet<AdminFrontend> AdminFrontends { get; set; }
        public virtual DbSet<AdminPaymentInfo> AdminPaymentInfoes { get; set; }
        public virtual DbSet<AdminTask> AdminTasks { get; set; }
        public virtual DbSet<APVariable> APVariables { get; set; }
        public virtual DbSet<Area> Areas { get; set; }
        public virtual DbSet<Assign> Assigns { get; set; }
        public virtual DbSet<Birthday> Birthdays { get; set; }
        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<BookingShortInfoTemp> BookingShortInfoTemps { get; set; }
        public virtual DbSet<Camp> Camps { get; set; }
        public virtual DbSet<Class> Classes { get; set; }
        public virtual DbSet<Coupon> Coupons { get; set; }
        public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }
        public virtual DbSet<Frontend> Frontends { get; set; }
        public virtual DbSet<Grade> Grades { get; set; }
        public virtual DbSet<GradeGroup> GradeGroups { get; set; }
        public virtual DbSet<Instructor> Instructors { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Parent> Parents { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<PrivateMessage> PrivateMessages { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Workshop> Workshops { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>()
                .Property(e => e.Zip)
                .IsUnicode(false);

            modelBuilder.Entity<Admin>()
                .HasMany(e => e.AdminEmailTemplates)
                .WithRequired(e => e.Admin)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Admin>()
                .HasMany(e => e.AdminFrontends)
                .WithRequired(e => e.Admin)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Admin>()
                .HasMany(e => e.AdminPaymentInfoes)
                .WithRequired(e => e.Admin)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Admin>()
                .HasMany(e => e.AdminTasks)
                .WithRequired(e => e.Admin)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Admin>()
                .HasMany(e => e.PrivateMessages)
                .WithOptional(e => e.Admin)
                .HasForeignKey(e => e.FromAdminId);

            modelBuilder.Entity<Admin>()
                .HasMany(e => e.PrivateMessages1)
                .WithOptional(e => e.Admin1)
                .HasForeignKey(e => e.ToAdminId);

            modelBuilder.Entity<APVariable>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Area>()
                .HasMany(e => e.Birthdays)
                .WithRequired(e => e.Area)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Area>()
                .HasMany(e => e.Instructors)
                .WithRequired(e => e.Area)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Area>()
                .HasMany(e => e.Locations)
                .WithRequired(e => e.Area)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Birthday>()
                .Property(e => e.ContactNumber)
                .IsUnicode(false);

            modelBuilder.Entity<BookingShortInfoTemp>()
                .Property(e => e.Cost)
                .HasPrecision(18, 10);

            modelBuilder.Entity<Camp>()
                .Property(e => e.Cost)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Class>()
                .Property(e => e.Cost)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Coupon>()
                .Property(e => e.Code)
                .IsUnicode(false);

            modelBuilder.Entity<Coupon>()
                .Property(e => e.DiscountAmount)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Grade>()
                .HasMany(e => e.Students)
                .WithRequired(e => e.Grade)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Grade>()
                .HasMany(e => e.GradeGroups)
                .WithMany(e => e.Grades)
                .Map(m => m.ToTable("GradeGroupDetail").MapLeftKey("GradeId").MapRightKey("GradeGroupId"));

            modelBuilder.Entity<GradeGroup>()
                .HasMany(e => e.Camps)
                .WithRequired(e => e.GradeGroup)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<GradeGroup>()
                .HasMany(e => e.Classes)
                .WithRequired(e => e.GradeGroup)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<GradeGroup>()
                .HasMany(e => e.Workshops)
                .WithRequired(e => e.GradeGroup)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Instructor>()
                .Property(e => e.PhoneNumber)
                .IsUnicode(false);

            modelBuilder.Entity<Instructor>()
                .Property(e => e.Zip)
                .IsUnicode(false);

            modelBuilder.Entity<Instructor>()
                .HasMany(e => e.Assigns)
                .WithRequired(e => e.Instructor)
                .HasForeignKey(e => e.InstructorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Instructor>()
                .HasMany(e => e.Assigns1)
                .WithOptional(e => e.Instructor1)
                .HasForeignKey(e => e.AssistantId);

            modelBuilder.Entity<Instructor>()
                .HasMany(e => e.Camps)
                .WithRequired(e => e.Instructor)
                .HasForeignKey(e => e.InstructorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Instructor>()
                .HasMany(e => e.Camps1)
                .WithOptional(e => e.Instructor1)
                .HasForeignKey(e => e.AssistantId);

            modelBuilder.Entity<Instructor>()
                .HasMany(e => e.Classes)
                .WithRequired(e => e.Instructor)
                .HasForeignKey(e => e.InstructorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Instructor>()
                .HasMany(e => e.Classes1)
                .WithOptional(e => e.Instructor1)
                .HasForeignKey(e => e.AssistantId);

            modelBuilder.Entity<Instructor>()
                .HasMany(e => e.Workshops)
                .WithRequired(e => e.Instructor)
                .HasForeignKey(e => e.InstructorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Instructor>()
                .HasMany(e => e.Workshops1)
                .WithOptional(e => e.Instructor1)
                .HasForeignKey(e => e.AssistantId);

            modelBuilder.Entity<Location>()
                .Property(e => e.PhoneNumber)
                .IsUnicode(false);

            modelBuilder.Entity<Location>()
                .Property(e => e.Zip)
                .IsUnicode(false);

            modelBuilder.Entity<Location>()
                .HasMany(e => e.Camps)
                .WithRequired(e => e.Location)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Location>()
                .HasMany(e => e.Classes)
                .WithRequired(e => e.Location)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Location>()
                .HasMany(e => e.Parents)
                .WithRequired(e => e.Location)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Location>()
                .HasMany(e => e.Workshops)
                .WithRequired(e => e.Location)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Parent>()
                .Property(e => e.Password)
                .IsUnicode(false);

            modelBuilder.Entity<Parent>()
                .Property(e => e.PhoneNumer)
                .IsUnicode(false);

            modelBuilder.Entity<Parent>()
                .Property(e => e.Zip)
                .IsUnicode(false);

            modelBuilder.Entity<Parent>()
                .HasMany(e => e.Students)
                .WithRequired(e => e.Parent)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Payment>()
                .Property(e => e.Amount)
                .HasPrecision(18, 0);

            modelBuilder.Entity<Payment>()
                .Property(e => e.PaymentMessage)
                .IsUnicode(false);

            modelBuilder.Entity<PrivateMessage>()
                .Property(e => e.MessageContent)
                .IsUnicode(false);

            modelBuilder.Entity<Student>()
                .Property(e => e.Gender)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Student>()
                .HasMany(e => e.Bookings)
                .WithRequired(e => e.Student)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Workshop>()
                .Property(e => e.Cost)
                .HasPrecision(18, 0);
        }
    }
}
