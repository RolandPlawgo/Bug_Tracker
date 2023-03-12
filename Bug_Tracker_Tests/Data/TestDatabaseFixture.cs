using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bug_Tracker_Tests.Data
{
    public class TestDatabaseFixture
    {
        private const string connectionString = "Server=(localdb)\\mssqllocaldb;Database=aspnet-Bug_Tracker_Tests-B2B95390-88C8-430B-967F-76A5B7B60C17;Trusted_Connection=True;MultipleActiveResultSets=true";

        private static readonly object _lock = new();
        private static bool _databaseInitialized = false;

        public TestDatabaseFixture()
        {
            lock (_lock)
            {
                if (!_databaseInitialized)
                {
                    using (ApplicationDbContext context = CreateContext())
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();

                        Comment comment1 = new Comment() { Text = "Comment 1 text", OwnerId = "11111", Date = new DateTime(2000, 1, 1) };
                        Comment comment2 = new Comment() { Text = "Comment 2 text", OwnerId = "11111", Date = new DateTime(1960, 2, 29) };
                        Comment comment3 = new Comment() { Text = "Comment 3 text", OwnerId = "22222", Date = new DateTime(2010, 1, 31) };
                        Ticket ticket1 = new Ticket() { Title = "Ticket 1 title", OwnerId = "11111", Comments = new List<Comment>() { comment1, comment2, comment3 }, ShortDescription = "Short description 1", LongDescription = "Long description of ticket 1", Date = new DateTime(2020, 1, 1), Status = Status.feature, Priority = Priority.medium };
                        Ticket ticket2 = new Ticket() { Title = "Ticket 2 title", OwnerId = "11111", Comments = new List<Comment>() { comment1 }, ShortDescription = "Short description 2", LongDescription = "Long description of ticket 2", Date = new DateTime(2020, 2, 29), Status = Status.feature, Priority = Priority.none };
                        Ticket ticket3 = new Ticket() { Title = "Ticket 3 title", OwnerId = "22222", Comments = new List<Comment>(), ShortDescription = "Short description 3", LongDescription = "Long description of ticket 3", Date = new DateTime(2022, 12, 31), Status = Status.bug, Priority = Priority.high };
                        Project project1 = new Project() { Title = "Project 1 title", Description = "Project 1 description", OwnerId = "11111", Tickets = new List<Ticket>() { ticket1, ticket2 } };
                        Project project2 = new Project() { Title = "Project 2 title", Description = "Project 2 description", OwnerId = "11111", Tickets = new List<Ticket>() { ticket3 } };
                        Project project3 = new Project() { Title = "Project 3 title", Description = "Project 3 description", OwnerId = "22222", Tickets = new List<Ticket>() };

                        context.AddRange(
                            comment1,
                            comment2,
                            comment3,
                            ticket1,
                            ticket2,
                            ticket3,
                            project1,
                            project2,
                            project3
                            );
                        context.SaveChanges();
                    }
                }

                _databaseInitialized = true;
            }
        }

        public ApplicationDbContext CreateContext()
        {
            return new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options);
        }
    }
}
