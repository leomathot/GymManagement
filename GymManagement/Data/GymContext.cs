using GymManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Data
{
    public class GymContext : DbContext
    {
        //To give access to IHttpContextAccessor for Audit Data with IAuditable
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Property to hold the UserName value
        public string UserName
        {
            get; private set;
        }

        public GymContext(DbContextOptions<GymContext> options, IHttpContextAccessor httpContextAccessor)
             : base(options)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            if (_httpContextAccessor.HttpContext != null)
            {
                //We have a HttpContext, but there might not be anyone Authenticated
                UserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            }
            else
            {
                //No HttpContext so seeding data
                UserName = "Seed Data";
            }
        }

        public GymContext(DbContextOptions<GymContext> options)
            : base(options)
        {
            _httpContextAccessor = null!;
            UserName = "Seed Data";
        }

        public DbSet<FitnessCategory> FitnessCategories { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<ClassTime> ClassTimes { get; set; }
        public DbSet<GroupClass> GroupClasses { get; set; }
        public DbSet<MembershipType> MembershipTypes { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<WorkoutExercise> WorkoutExercises { get; set; }
        public DbSet<ExerciseCategory> ExerciseCategories { get; set; }
        public DbSet<ClientPhoto> ClientPhotos { get; set; }
        public DbSet<ClientThumbnail> ClientThumbnails { get; set; }
        public DbSet<InstructorDocument> InstructorDocuments { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Prevent Cascade Delete from FitnessCategory to GroupClass
            //so we are prevented from deleting a FitnessCategory with
            //GroupClasses scheduled
            modelBuilder.Entity<FitnessCategory>()
                .HasMany<GroupClass>(fc => fc.GroupClasses)
                .WithOne(gc => gc.FitnessCategory)
                .HasForeignKey(gc => gc.FitnessCategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            //Same for Instructor
            modelBuilder.Entity<Instructor>()
                .HasMany<GroupClass>(i => i.GroupClasses)
                .WithOne(gc => gc.Instructor)
                .HasForeignKey(gc => gc.InstructorID)
                .OnDelete(DeleteBehavior.Restrict);

            //Same for ClassTime
            modelBuilder.Entity<ClassTime>()
                .HasMany<GroupClass>(ct => ct.GroupClasses)
                .WithOne(gc => gc.ClassTime)
                .HasForeignKey(gc => gc.ClassTimeID)
                .OnDelete(DeleteBehavior.Restrict);

            //Similar for MembershipType and Client
            modelBuilder.Entity<MembershipType>()
                .HasMany<Client>(m => m.Clients)
                .WithOne(c => c.MembershipType)
                .HasForeignKey(c => c.MembershipTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            //Prevent Cascade Delete from Client to Enrollment
            //so we are prevented from deleting a Client who is
            //Enrolled in any Group Classes.
            //NOTE: we will allow cascade delete from GroupClass
            //to Enrollment so if a class is deleted, the related
            //enrollment records are cleaned up as well.
            //Also Note: We will allow cascade delete from Client
            //to Workout
            modelBuilder.Entity<Client>()
                .HasMany<Enrollment>(c => c.Enrollments)
                .WithOne(e => e.Client)
                .HasForeignKey(e => e.ClientID)
                .OnDelete(DeleteBehavior.Restrict);

            //3 New restrictions for Part 3A
            modelBuilder.Entity<Exercise>()
                .HasMany<ExerciseCategory>(d => d.ExerciseCategories)
                .WithOne(p => p.Exercise)
                .HasForeignKey(p => p.ExerciseID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Exercise>()
                .HasMany<WorkoutExercise>(d => d.WorkoutExercises)
                .WithOne(p => p.Exercise)
                .HasForeignKey(p => p.ExerciseID)
                .OnDelete(DeleteBehavior.Restrict);

            //Many to Many Intersection
            modelBuilder.Entity<Enrollment>()
            .HasKey(e => new { e.ClientID, e.GroupClassID });

			//Many to Many Intersection
			modelBuilder.Entity<ExerciseCategory>()
            .HasKey(e => new { e.FitnessCategoryID, e.ExerciseID });
			//Many to Many Intersection
			modelBuilder.Entity<WorkoutExercise>()
            .HasKey(e => new { e.WorkoutID, e.ExerciseID });

            //Add a unique index to the Instructor Email 
            modelBuilder.Entity<Instructor>()
            .HasIndex(i => i.Email)
            .IsUnique();

			//Add a unique index to the Client Email 
			modelBuilder.Entity<Client>()
			.HasIndex(c => c.Email)
			.IsUnique();

			//Add a unique index to the Client MembershipNumber 
			modelBuilder.Entity<Client>()
            .HasIndex(c => c.MembershipNumber)
            .IsUnique();

            //Add a unique composite index to the GroupClass 
            modelBuilder.Entity<GroupClass>()
            .HasIndex(gc => new { gc.InstructorID, gc.DOW, gc.ClassTimeID })
            .IsUnique();

            //Many to Many Intersection
            modelBuilder.Entity<Enrollment>()
            .HasKey(e => new { e.ClientID, e.GroupClassID });

			// Add unique constraint to the Category property of the FitnessCategory model class
			modelBuilder.Entity<FitnessCategory>()
				.HasIndex(fc => fc.Category)
				.IsUnique();

			// Add unique constraint to the Name property of the Exercise model class
			modelBuilder.Entity<Exercise>()
				.HasIndex(e => e.Name)
				.IsUnique();
		}

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            trackable.CreatedOn = now;
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }
    }
}
