using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using Moq.EntityFrameworkCore;
using Bug_Tracker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp;
using Bug_Tracker.Models;
using Xunit.Sdk;
using System.Net.Http.Headers;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Bug_Tracker_Tests.Data
{
    public class GenericRepositoryTest : IClassFixture<TestDatabaseFixture>
    {
        public TestDatabaseFixture Fixture { get; }
        public GenericRepositoryTest(TestDatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        #region Get tests
        [Fact]
        public async Task GetAsync_EntityNotNull()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = await projectRepository.GetAsync();

                Assert.NotNull(allProjects);
            }
        }

        [Fact]
        public async Task GetAsync_ReturnsExpectedEntities()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = await projectRepository.GetAsync();

                Assert.Equal("Project 1 title", allProjects.First().Title);
                Assert.Equal("Project 1 description", allProjects.First().Description);
                Assert.Equal("11111", allProjects.First().OwnerId);
                Assert.Equal(3, allProjects.Count());
            }
        }

        [Fact]
        public async Task GetAsync_IncludedPropertiesNotNull()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = await projectRepository.GetAsync(includeProperties: "Tickets", filter: null);

                Assert.All(allProjects, item => Assert.NotNull(item));
            }
        }

        [Fact]
        public async Task GetAsync_ReturnsIncludedProperties()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = await projectRepository.GetAsync(includeProperties: "Tickets", filter: null);

                Assert.Equal("Short description 1", allProjects.First().Tickets.First().ShortDescription);
                Assert.Equal("Long description of ticket 1", allProjects.First().Tickets.First().LongDescription);
                Assert.True(DateTime.Equals(new DateTime(2020, 1, 1), allProjects.First().Tickets.First().Date));
                Assert.Equal(Status.feature, allProjects.First().Tickets.First().Status);
                Assert.Equal(Priority.medium, allProjects.First().Tickets.First().Priority);
            }
        }

        [Fact]
        public async Task GetAsync_IncludedPropertiesNotNull_InOverloadWithMultipleFilters()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = await projectRepository.GetAsync(includeProperties: "Tickets", filter: null);

                foreach (Project project in allProjects)
                {
                    Assert.NotNull(project.Tickets);
                }
            }
        }

        [Fact]
        public async Task GetAsync_ReturnsIncludedProperties_InOverloadWithMultipleFilters()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = await projectRepository.GetAsync(includeProperties: "Tickets", filters: null);

                Assert.Equal("Short description 1", allProjects.First().Tickets.First().ShortDescription);
                Assert.Equal("Long description of ticket 1", allProjects.First().Tickets.First().LongDescription);
                Assert.True(DateTime.Equals(new DateTime(2020, 1, 1), allProjects.First().Tickets.First().Date));
                Assert.Equal(Status.feature, allProjects.First().Tickets.First().Status);
                Assert.Equal(Priority.medium, allProjects.First().Tickets.First().Priority);
            }
        }

        [Fact]
        public async Task GetAsync_ReturnsFilteredEntities()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = await projectRepository.GetAsync(filter: t => t.Title.Contains("title"));
                IEnumerable<Project> oneProject = await projectRepository.GetAsync(filter: t => t.Title.Contains("3"));
                IEnumerable<Project> noProjects = await projectRepository.GetAsync(filter: t => t.Title.Contains("!"));

                Assert.True(allProjects.Count() == 3);
                Assert.True(oneProject.Count() == 1);
                Assert.True(noProjects.Count() == 0);
            }
        }
        [Fact]
        public async Task GetAsync_ReturnsFilteredEntitiesWithMultipleFilters()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = await projectRepository.GetAsync(filters: new List<Expression<Func<Project, bool>>>() { t => t.Title.Contains("title"), t => t.Description.Contains("description") });
                IEnumerable<Project> oneProject = await projectRepository.GetAsync(filters: new List<Expression<Func<Project, bool>>>() { t => t.Title.Contains("title"), t => t.Description.Contains("1") });
                IEnumerable<Project> noProjects = await projectRepository.GetAsync(filters: new List<Expression<Func<Project, bool>>>() { t => t.Title.Contains("title"), t => t.Description.Contains("!") });

                Assert.True(allProjects.Count() == 3);
                Assert.True(oneProject.Count() == 1);
                Assert.True(noProjects.Count() == 0);
            }
        }

        [Fact]
        public async Task GetAsync_ReturnOrderedEntities()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> projectOrderedByTitleDescending = await projectRepository.GetAsync(orderBy: project => project.OrderByDescending(p => p.Title), filter: null);

                Assert.Collection(projectOrderedByTitleDescending,
                    item => Assert.Equal("Project 3 title", item.Title),
                    item => Assert.Equal("Project 2 title", item.Title),
                    item => Assert.Equal("Project 1 title", item.Title));
            }
        }

        [Fact]
        public async Task GetAsync_ReturnOrderedEntities_InOverloadWithMultipleFilters()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> projectOrderedByTitleDescending = await projectRepository.GetAsync(orderBy: project => project.OrderByDescending(p => p.Title), filters: null);

                Assert.Collection(projectOrderedByTitleDescending,
                    item => Assert.Equal("Project 3 title", item.Title),
                    item => Assert.Equal("Project 2 title", item.Title),
                    item => Assert.Equal("Project 1 title", item.Title));
            }
        }

        [Fact]
        public async Task GetAsync_ReturnsPagedEntities()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> projectsOnOnePage = await projectRepository.GetAsync(1, 3, filter: null);
                IEnumerable<Project> projectsOnTwoPages = await projectRepository.GetAsync(2, 2, filter: null);
                IEnumerable<Project> projectsOnThreePages = await projectRepository.GetAsync(3, 1, filter: null);

                Assert.True(projectsOnOnePage.Count() == 3);
                Assert.True(projectsOnTwoPages.Count() == 1);
                Assert.True(projectsOnThreePages.Count() == 1);
            }
        }
        [Fact]
        public async Task GetAsync_ReturnsPagedEntities_InOverloadWithMultipleFilters()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> projectsOnOnePage = await projectRepository.GetAsync(1, 3, filters: null);
                IEnumerable<Project> projectsOnTwoPages = await projectRepository.GetAsync(2, 2, filters: null);
                IEnumerable<Project> projectsOnThreePages = await projectRepository.GetAsync(3, 1, filters: null);

                Assert.True(projectsOnOnePage.Count() == 3);
                Assert.True(projectsOnTwoPages.Count() == 1);
                Assert.True(projectsOnThreePages.Count() == 1);
            }
        }
        #endregion


        #region GetEntity tests
        [Fact]
        public async Task GetEntityAsync_ReturnsTheRightEntity()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project? project1 = await projectRepository.GetEntityAsync(1);
                Project? project2 = await projectRepository.GetEntityAsync(2);
                Project? project3 = await projectRepository.GetEntityAsync(3);

                Assert.Equal("Project 1 title", project1!.Title);
                Assert.Equal("Project 2 title", project2!.Title);
                Assert.Equal("Project 3 title", project3!.Title);
            }
        }

        [Fact]
        public async Task GetEntityAsync_ReturnsNullIfEntityDoesntExist()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project? project = await projectRepository.GetEntityAsync(10);

                Assert.Null(project);
            }
        }

        [Fact]
        public async Task GetEntityAsync_ReturnsFilteredEntity()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project? project1 = await projectRepository.GetEntityAsync(filter: t => t.Title.Contains("title"));
                Project? project3 = await projectRepository.GetEntityAsync(filter: t => t.Title.Contains("3"));

                Assert.Equal("Project 1 title", project1!.Title);
                Assert.Equal("Project 3 title", project3!.Title);
            }
        }

        [Fact]
        public async Task GetEntityAsync_ReturnsNullIfNoEntityMatchesFilter()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project? notExistingProject = await projectRepository.GetEntityAsync(filter: t => t.Title.Contains("!"));

                Assert.Null(notExistingProject);
            }
        }

        [Fact]
        public async Task GetEntityAsync_IncludedPropertyNotNull()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project? project = await projectRepository.GetEntityAsync(null, "Tickets");

                Assert.NotNull(project!.Tickets);
            }
        }

        [Fact]
        public async Task GetEntityAsync_ReturnsIncludedProperties()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project? project = await projectRepository.GetEntityAsync(null, "Tickets");

                Assert.Equal("Short description 1", project!.Tickets.First().ShortDescription);
                Assert.Equal("Long description of ticket 1", project.Tickets.First().LongDescription);
                Assert.True(DateTime.Equals(new DateTime(2020, 1, 1), project.Tickets.First().Date));
                Assert.Equal(Status.feature, project.Tickets.First().Status);
                Assert.Equal(Priority.medium, project.Tickets.First().Priority);
            }
        }
        #endregion


        [Fact]
        public async Task CreateAsync_AddsEntitiesToTheDatabase()
        {
            using (var context = Fixture.CreateContext())
            {
                context.Database.BeginTransaction();
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project newProject = new Project() { Title = "Project 4 title", Description = "Project 4 description", OwnerId = "22222", Tickets = new List<Ticket>() };
                await projectRepository.CreateAsync(newProject);
                context.SaveChanges();

                context.ChangeTracker.Clear();
                var project = context.Projects.Single(p => p.Description == "Project 4 description");
                Assert.Equal("Project 4 title", project.Title);
            }
        }


        [Fact]
        public async Task EditAsync_UpdatesEntities()
        {
            using (var context = Fixture.CreateContext())
            {
                context.Database.BeginTransaction();
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project project = await projectRepository.GetEntityAsync(1)!;
                project.Title = "Edited project title";
                await projectRepository.EditAsync(project);
                context.SaveChanges();

                context.ChangeTracker.Clear();
                Assert.Equal("Edited project title", context.Projects.Single(p => p.Description == "Project 1 description").Title);
            }
        }


        [Fact]
        public async Task DeleteAsync_RemovesEntities()
        {
            using (var context = Fixture.CreateContext())
            {
                context.Database.BeginTransaction();
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                await projectRepository.DeleteAsync(2);
                context.SaveChanges();

                context.ChangeTracker.Clear();
                Assert.Equal(2, context.Projects.Count());
            }
        }


        [Fact]
        public async Task SaveAsync_SavesChangesToTheDatabase()
        {
            using (var context = Fixture.CreateContext())
            {
                context.Database.BeginTransaction();
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                context.Projects.Add(new Project() { Title = "Project 4 title", Description = "Project 4 description", OwnerId = "22222", Tickets = new List<Ticket>() });
                await projectRepository.SaveAsync();

                context.ChangeTracker.Clear();
                Assert.Equal(4, context.Projects.Count());
            }
        }
    }
}
