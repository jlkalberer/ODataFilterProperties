namespace ODataFilterPropertiesByRole.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    using ODataFilterPropertiesByRole.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<ODataFilterPropertiesByRole.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(ODataFilterPropertiesByRole.Models.ApplicationDbContext context)
        {
            if (!context.Roles.Any(r => r.Name == "Administrator"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var role = new IdentityRole { Name = "Administrator" };

                manager.Create(role);
            }

            if (!context.Users.Any(u => u.UserName == "admin"))
            {
                var store = new UserStore<ApplicationUser>(context);
                var manager = new UserManager<ApplicationUser>(store);
                var user = new ApplicationUser { UserName = "admin", Email = "admin@test.com" };

                manager.Create(user, "password");
                manager.AddToRole(user.Id, "Administrator");
            }

            if (!context.Users.Any(u => u.UserName == "test"))
            {
                var store = new UserStore<ApplicationUser>(context);
                var manager = new UserManager<ApplicationUser>(store);
                var user = new ApplicationUser { UserName = "test", Email = "test@test.com" };

                manager.Create(user, "password");
            }
            context.SaveChanges();

            var testUser = context.Users.First(u => u.UserName == "test");
            if (!testUser.Enrollments.Any())
            {
                var course = new Course { Credits = 4, Title = "Test Course", };
                var course2 = new Course { Credits = 4, Title = "Test Course 2", };
                context.Courses.Add(course);
                context.Courses.Add(course2);
                context.SaveChanges();

                testUser.Enrollments.Add(new Enrollment
                {
                    Course = course,
                    Grade = Grade.B,
                });
                testUser.Enrollments.Add(new Enrollment
                {
                    Course = course2,
                    Grade = Grade.A,
                });
                context.SaveChanges();
            }
        }
    }
}
