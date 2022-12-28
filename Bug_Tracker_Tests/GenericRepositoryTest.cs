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

namespace Bug_Tracker_Tests
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
        public void Get_EntityNotNull()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = projectRepository.Get();

                Assert.NotNull(allProjects);
            }
        }

        [Fact]
        public void Get_ReturnsExpectedEntities()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = projectRepository.Get();

                Assert.Equal("Project 1 title", allProjects.First().Title);
                Assert.Equal("Project 1 description", allProjects.First().Description);
                Assert.Equal(3, allProjects.Count());
            }
        }

        [Fact]
        public void Get_IncludedPropertiesNotNull()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = projectRepository.Get(includeProperties: "Tickets", filter: null);

                Assert.All(allProjects, item => Assert.NotNull(item));
            }
        }

        [Fact]
        public void Get_ReturnsIncludedProperties()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = projectRepository.Get(includeProperties: "Tickets", filter: null);

                Assert.Equal("Short description 1", allProjects.First().Tickets.First().ShortDescription);
                Assert.Equal("Long description of ticket 1", allProjects.First().Tickets.First().LongDescription);
                Assert.True(DateTime.Equals(new DateTime(2020, 1, 1), allProjects.First().Tickets.First().Date));
                Assert.Equal(Status.feature , allProjects.First().Tickets.First().Status);
                Assert.Equal(Priority.medium, allProjects.First().Tickets.First().Priority);
            }
        }

        [Fact]
        public void Get_IncludedPropertiesNotNullInOverloadWithMultipleFilters()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = projectRepository.Get(includeProperties: "Tickets", filter: null);

                foreach (Project project in allProjects)
                {
                    Assert.NotNull(project.Tickets);
                }
            }
        }

        [Fact]
        public void Get_ReturnsIncludedPropertiesInOverloadWithMultipleFilters()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = projectRepository.Get(includeProperties: "Tickets", filters: null);

                Assert.Equal("Short description 1", allProjects.First().Tickets.First().ShortDescription);
                Assert.Equal("Long description of ticket 1", allProjects.First().Tickets.First().LongDescription);
                Assert.True(DateTime.Equals(new DateTime(2020, 1, 1), allProjects.First().Tickets.First().Date));
                Assert.Equal(Status.feature, allProjects.First().Tickets.First().Status);
                Assert.Equal(Priority.medium, allProjects.First().Tickets.First().Priority);
            }
        }

        [Fact]
        public void Get_ReturnsFilteredEntities()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = projectRepository.Get(filter: t => t.Title.Contains("title"));
                IEnumerable<Project> oneProject = projectRepository.Get(filter: t => t.Title.Contains("3"));
                IEnumerable<Project> noProjects = projectRepository.Get(filter: t => t.Title.Contains("!"));

                Assert.True(allProjects.Count() == 3);
                Assert.True(oneProject.Count() == 1);
                Assert.True(noProjects.Count() == 0);
            }
        }
        [Fact]
        public void Get_ReturnsFilteredEntitiesWithMultipleFilters()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> allProjects = projectRepository.Get(filters: new List<Expression<Func<Project, bool>>>() { t => t.Title.Contains("title"), t => t.Description.Contains("description") });
                IEnumerable<Project> oneProject = projectRepository.Get(filters: new List<Expression<Func<Project, bool>>>() { t => t.Title.Contains("title"), t => t.Description.Contains("1") });
                IEnumerable<Project> noProjects = projectRepository.Get(filters: new List<Expression<Func<Project, bool>>>() { t => t.Title.Contains("title"), t => t.Description.Contains("!") });

                Assert.True(allProjects.Count() == 3);
                Assert.True(oneProject.Count() == 1);
                Assert.True(noProjects.Count() == 0);
            }
        }

        [Fact]
        public void Get_ReturnOrderedEntities()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> projectOrderedByTitleDescending = projectRepository.Get(orderBy: project => project.OrderByDescending(p => p.Title), filter: null);

                Assert.Collection(projectOrderedByTitleDescending,
                    item => Assert.Equal("Project 3 title", item.Title),
                    item => Assert.Equal("Project 2 title", item.Title),
                    item => Assert.Equal("Project 1 title", item.Title));
            }
        }

        [Fact]
        public void Get_ReturnOrderedEntitiesInOverloadWithMultipleFilters()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> projectOrderedByTitleDescending = projectRepository.Get(orderBy: project => project.OrderByDescending(p => p.Title), filters: null);

                Assert.Collection(projectOrderedByTitleDescending,
                    item => Assert.Equal("Project 3 title", item.Title),
                    item => Assert.Equal("Project 2 title", item.Title),
                    item => Assert.Equal("Project 1 title", item.Title));
            }
        }

        [Fact]
        public void Get_ReturnsPagedEntities()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> projectsOnOnePage = projectRepository.Get(1, 3, out int pages, filter: null);
                IEnumerable<Project> projectsOnTwoPages = projectRepository.Get(2, 2, out int pages2, filter: null);
                IEnumerable<Project> projectsOnThreePages = projectRepository.Get(3, 1, out int pages3, filter: null);

                Assert.Equal(1, pages);
                Assert.Equal(2, pages2);
                Assert.Equal(3, pages3);
                Assert.True(projectsOnOnePage.Count() == 3);
                Assert.True(projectsOnTwoPages.Count() == 1);
                Assert.True(projectsOnThreePages.Count() == 1);
            }
        }
        [Fact]
        public void Get_ReturnsPagedEntitiesInOverloadWithMultipleFilters()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                IEnumerable<Project> projectsOnOnePage = projectRepository.Get(1, 3, out int pages, filters: null);
                IEnumerable<Project> projectsOnTwoPages = projectRepository.Get(2, 2, out int pages2, filters: null);
                IEnumerable<Project> projectsOnThreePages = projectRepository.Get(3, 1, out int pages3, filters: null);

                Assert.Equal(1, pages);
                Assert.Equal(2, pages2);
                Assert.Equal(3, pages3);
                Assert.True(projectsOnOnePage.Count() == 3);
                Assert.True(projectsOnTwoPages.Count() == 1);
                Assert.True(projectsOnThreePages.Count() == 1);
            }
        }

        #endregion

        [Fact]
        public void GetEntity_ReturnsTheRightEntity()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project? project1 = projectRepository.GetEntity(1);
                Project? project2 = projectRepository.GetEntity(2);
                Project? project3 = projectRepository.GetEntity(3);

                Assert.Equal("Project 1 title", project1!.Title);
                Assert.Equal("Project 2 title", project2!.Title);
                Assert.Equal("Project 3 title", project3!.Title);
            }
        }

        [Fact]
        public void GetEntity_ReturnsNullIfEntityDoesntExist()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project? project = projectRepository.GetEntity(10);

                Assert.Null(project);
            }
        }

        [Fact]
        public void GetEntity_ReturnsFilteredEntity()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project? project1 = projectRepository.GetEntity(filter: t => t.Title.Contains("title"));
                Project? project3 = projectRepository.GetEntity(filter: t => t.Title.Contains("3"));

                Assert.Equal("Project 1 title", project1!.Title);
                Assert.Equal("Project 3 title", project3!.Title);
            }
        }

        [Fact]
        public void GetEntity_ReturnsNullIfNoEntityMatchesFilter()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project? notExistingProject = projectRepository.GetEntity(filter: t => t.Title.Contains("!"));

                Assert.Null(notExistingProject);
            }
        }

        [Fact]
        public void GetEntity_IncludedPropertyNotNull()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project? project = projectRepository.GetEntity(null, "Tickets");

                Assert.NotNull(project!.Tickets);
            }
        }

        [Fact]
        public void GetEntity_ReturnsIncludedProperties()
        {
            using (var context = Fixture.CreateContext())
            {
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project? project = projectRepository.GetEntity(null, "Tickets");

                Assert.Equal("Short description 1", project!.Tickets.First().ShortDescription);
                Assert.Equal("Long description of ticket 1", project.Tickets.First().LongDescription);
                Assert.True(DateTime.Equals(new DateTime(2020, 1, 1), project.Tickets.First().Date));
                Assert.Equal(Status.feature, project.Tickets.First().Status);
                Assert.Equal(Priority.medium, project.Tickets.First().Priority);
            }
        }




        [Fact]
        public void Create_AddsEntitiesToTheDatabase()
        {
            using (var context = Fixture.CreateContext())
            {
                context.Database.BeginTransaction();
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project newProject = new Project() { Title = "Project 4 title", Description = "Project 4 description", Tickets = new List<Ticket>() };
                projectRepository.Create(newProject);
                context.SaveChanges();

                context.ChangeTracker.Clear();
                var project = context.Projects.Single(p => p.Description == "Project 4 description");
                Assert.Equal("Project 4 title", project.Title);
            }
        }



        [Fact]
        public void Edit_UpdatesEntities()
        {
            using (var context = Fixture.CreateContext())
            {
                context.Database.BeginTransaction();
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                Project project = projectRepository.GetEntity(1)!;
                project.Title = "Edited project title";
                projectRepository.Edit(project);
                context.SaveChanges();

                context.ChangeTracker.Clear();
                Assert.Equal("Edited project title", context.Projects.Single(p => p.Description == "Project 1 description").Title);
            }
        }

        [Fact]
        public void Delete_RemovesEntities()
        {
            using (var context = Fixture.CreateContext())
            {
                context.Database.BeginTransaction();
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                projectRepository.Delete(2);
                context.SaveChanges();

                context.ChangeTracker.Clear();
                Assert.Equal(2, context.Projects.Count());
            }
        }

        [Fact]
        public void Save_SavesChangesToTheDatabase()
        {
            using (var context = Fixture.CreateContext())
            {
                context.Database.BeginTransaction();
                GenericRepository<Project> projectRepository = new GenericRepository<Project>(context);

                context.Projects.Add(new Project() { Title = "Project 4 title", Description = "Project 4 description", Tickets = new List<Ticket>() });
                projectRepository.Save();

                context.ChangeTracker.Clear();
                Assert.Equal(4, context.Projects.Count());
            }
        }
    }
}
