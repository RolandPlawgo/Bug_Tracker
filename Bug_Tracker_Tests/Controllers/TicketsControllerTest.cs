using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Bug_Tracker.Controllers;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Runtime.CompilerServices;

namespace Bug_Tracker_Tests.Controllers
{
    public class TicketsControllerTest
    {
        #region Index Tests
        [Theory]
        [InlineData("", "any", "any", "any", 1, 0)]
        [InlineData("", "bug", "high", "any", 1, 2)]
        [InlineData("Xyz", "bug", "high", "Project Title", 1, 4)]
        public async Task Index_Get_CallsGetWithTheRightParameters_ReturnsAViewResultWithTickets(string searchString, string statusFilter, string priorityFilter, string projectFilter, int page, int numberOfFilters)
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            TicketsController controller = CreateController(ticketRepositoryMock);

            var result = await controller.Index("", searchString, statusFilter, priorityFilter, projectFilter, page);

            ticketRepositoryMock.Verify(r => r.GetAsync(page, It.IsAny<int>(), "Project", It.Is<List<Expression<Func<Ticket, bool>>>>(l => l.Count() == numberOfFilters), It.IsAny<Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>>()));
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Ticket>>(viewResult.ViewData.Model);
        }
        [Fact]
        public async Task Index_Get_ReturnsARedirectToActionResult_IfArgumentExceptionThrown()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetAsync(2, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<Expression<Func<Ticket, bool>>>>(), It.IsAny<Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>>())).Throws(new InvalidOperationException(""));
            TicketsController controller = CreateController(ticketRepositoryMock);

            var result = await controller.Index("", "", "", "", "", 2);

            Assert.IsType<RedirectToActionResult>(result);
        }
        #endregion

        #region Create Tests
        [Fact]
        public async Task Create_Get_ReturnsAViewResult()
        {
            TicketsController controller = CreateController();

            var result = await controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_CreatesTheTicket_ReturnsARedirectToActionResult_IfProjectExistsAndModelStateValid()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestProject(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock, UserManagerMock("11111"), AuthorizationServiceMock(true));
            Ticket ticket = GetTestTicket(1)!;

            var result = await controller.Create(ticket, ticket.Project.Title);

            ticketRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Ticket>()));
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public async Task Create_Post_ReturnsANotFoundResult_IfProjectDoesntExist()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestProject(3));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock, UserManagerMock("11111"), AuthorizationServiceMock(true));
            Ticket ticket = GetTestTicket(1)!;

            var result = await controller.Create(ticket, ticket.Project.Title);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Create_Post_ReturnsAViewResult_IfModelInvalid()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestProject(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock, UserManagerMock("11111"), AuthorizationServiceMock(true));
            controller.ModelState.AddModelError("key", "message");
            Ticket ticket = GetTestTicket(1)!;

            var result = await controller.Create(ticket, ticket.Project.Title);

            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async Task Create_Post_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestProject(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            var authorizationServiceMock = AuthorizationServiceMock(false);
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock, UserManagerMock("11111"), authorizationServiceMock);
            Ticket ticket = GetTestTicket(1)!;

            var result = await controller.Create(ticket, ticket.Project.Title);

            Assert.IsType<ForbidResult>(result);
        }
        #endregion

        #region Details Tests
        [Fact]
        public async Task Details_Get_ReturnsAViewResultWithATicket_IfTicketExists()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);

            var result = await controller.Details(1);

            var viewResullt = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Ticket>(viewResullt.ViewData.Model);
            Assert.Equal("Ticket 1 title", model.Title);
        }
        [Fact]
        public async Task Details_Get_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).ReturnsAsync(GetTestTicket(3));
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);

            var result = await controller.Details(3);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Details_Get_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).ReturnsAsync(GetTestTicket(1));
            var authorizationServiceMock = AuthorizationServiceMock(false);
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock, authorizationServiceMock);

            var result = await controller.Details(1);

            Assert.IsType<ForbidResult>(result);
        }
        #endregion

        #region Edit Tests
        [Fact]
        public async Task Edit_Get_ReturnsAViewResultWithATicket_IfTicketExists()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(ticketRepositoryMock);

            var result = await controller.Edit(1);

            var viewResullt = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Ticket>(viewResullt.ViewData.Model);
            Assert.Equal("Ticket 1 title", model.Title);
        }
        [Fact]
        public async Task Edit_Get_ReturnsANotFoundResult_IfTicketDoesntExists()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).ReturnsAsync(GetTestTicket(3));
            var controller = CreateController(ticketRepositoryMock);

            var result = await controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Edit_Get_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).ReturnsAsync(GetTestTicket(1));
            Mock<IAuthorizationService> authorizationServiceMock = AuthorizationServiceMock(false);
            var controller = CreateController(ticketRepositoryMock, authorizationServiceMock);

            var result = await controller.Edit(1);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Edit_Post_EditsTheTicket_ReturnsARedirectToActionResult_IfModelValid()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestProject(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestTicket(1));
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1)!).ReturnsAsync(GetTestTicket(1)!);
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);

            var result = await controller.Edit(1, GetTestTicket(1)!, GetTestProject(1)!.Title);

            ticketRepositoryMock.Verify(r => r.EditAsync(It.IsAny<Ticket>()));
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public async Task Edit_Post_ReturnsAViewResultWithATicket_IfModelInvalid()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestProject(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestTicket(1));
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1)!).ReturnsAsync(GetTestTicket(1)!);
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);
            controller.ModelState.AddModelError("key", "message");

            var result = await controller.Edit(1, GetTestTicket(1)!, GetTestProject(1)!.Title);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<Ticket>(viewResult.ViewData.Model);
        }
        [Fact]
        public async Task Edit_Post_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestProject(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestTicket(3));
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1)!).ReturnsAsync(GetTestTicket(1)!);
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);

            var result = await controller.Edit(3, GetTestTicket(1)!, GetTestProject(1)!.Title);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Edit_Post_ReturnsANotFoundResult_IfProjectDoesntExist()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestProject(3));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestTicket(1));
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1)!).ReturnsAsync(GetTestTicket(1)!);
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);

            var result = await controller.Edit(1, GetTestTicket(1)!, GetTestProject(1)!.Title);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Edit_Post_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestProject(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>())).ReturnsAsync(GetTestTicket(1));
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1)!).ReturnsAsync(GetTestTicket(1)!);
            Mock<IAuthorizationService> authorizationServiceMock = AuthorizationServiceMock(false);
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock, authorizationServiceMock);

            var result = await controller.Edit(1, GetTestTicket(1)!, GetTestProject(1)!.Title);

            Assert.IsType<ForbidResult>(result);
        }
        #endregion

        #region Delete Tests
        [Fact]
        public async Task Delete_Get_ReturnsAViewResultWithATicket_IfTicketExists()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(ticketRepositoryMock);

            var result = await controller.Delete(1);

            var viewResullt = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Ticket>(viewResullt.ViewData.Model);
            Assert.Equal("Ticket 1 title", model.Title);
        }
        [Fact]
        public async Task Delete_Get_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).ReturnsAsync(GetTestTicket(3));
            var controller = CreateController(ticketRepositoryMock);

            var result = await controller.Delete(3);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Post_DeletesTheTicket_ReturnsARedirectToActionResult_IfTicketExists()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(t => t.GetEntityAsync(1)).ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(ticketRepositoryMock);

            var result = await controller.Delete(1, new FormCollection(null));

            ticketRepositoryMock.Verify(r => r.DeleteAsync(1));
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public async Task Delete_Post_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(3)).ReturnsAsync(GetTestTicket(3));
            var controller = CreateController(ticketRepositoryMock);

            var result = await controller.Delete(3, new FormCollection(null));

            Assert.IsType<NotFoundResult>(result);
        }
        public async Task Delete_Post_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(ticketRepositoryMock);

            var result = await controller.Delete(1);

            var viewResullt = Assert.IsType<ForbidResult>(result);
        }
        #endregion




        #region Mocking
        private Mock<IAuthorizationService> AuthorizationServiceMock(bool succeeded)
        {
            Mock<IAuthorizationService> authorizationServiceMock = new Mock<IAuthorizationService>();
            if (succeeded)
            {
                authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.Is<object?>(o => o.GetType() == typeof(Ticket)), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                    .ReturnsAsync(AuthorizationResult.Success);
            }
            else
            {
                authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.Is<object?>(o => o.GetType() == typeof(Ticket)), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                    .ReturnsAsync(AuthorizationResult.Failed);
            }
            return authorizationServiceMock;
        }

        private Mock<UserManager<IdentityUser>> UserManagerMock()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
        }
        private Mock<UserManager<IdentityUser>> UserManagerMock(string id)
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            var userManagerMock = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(id);
            return userManagerMock;
        }
        #endregion

        #region Create controller
        private TicketsController CreateController()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            var loggerMock = new Mock<ILogger<TicketsController>>();
            return new TicketsController(ticketRepositoryMock.Object, projectRepositoryMock.Object, loggerMock.Object, UserManagerMock().Object, AuthorizationServiceMock(true).Object);
        }
        private TicketsController CreateController(Mock<IGenericRepository<Ticket>> ticketRepositoryMock)
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            var loggerMock = new Mock<ILogger<TicketsController>>();
            return new TicketsController(ticketRepositoryMock.Object, projectRepositoryMock.Object, loggerMock.Object, UserManagerMock().Object, AuthorizationServiceMock(true).Object);
        }
        private TicketsController CreateController(Mock<IGenericRepository<Ticket>> ticketRepositoryMock, Mock<IGenericRepository<Project>> projectRepositoryMock)
        {
            var loggerMock = new Mock<ILogger<TicketsController>>();
            return new TicketsController(ticketRepositoryMock.Object, projectRepositoryMock.Object, loggerMock.Object, UserManagerMock().Object, AuthorizationServiceMock(true).Object);
        }

        private TicketsController CreateController(Mock<IAuthorizationService> authorizationService)
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            var loggerMock = new Mock<ILogger<TicketsController>>();
            return new TicketsController(ticketRepositoryMock.Object, projectRepositoryMock.Object, loggerMock.Object, UserManagerMock().Object, authorizationService.Object);
        }
        private TicketsController CreateController(Mock<IGenericRepository<Ticket>> ticketRepositoryMock, Mock<IAuthorizationService> authorizationServiceMock)
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            var loggerMock = new Mock<ILogger<TicketsController>>();
            return new TicketsController(ticketRepositoryMock.Object, projectRepositoryMock.Object, loggerMock.Object, UserManagerMock().Object, authorizationServiceMock.Object);
        }
        private TicketsController CreateController(Mock<IGenericRepository<Ticket>> ticketRepositoryMock, Mock<IGenericRepository<Project>> projectRepositoryMock, Mock<IAuthorizationService> authorizationServiceMock)
        {
            var loggerMock = new Mock<ILogger<TicketsController>>();
            return new TicketsController(ticketRepositoryMock.Object, projectRepositoryMock.Object, loggerMock.Object, UserManagerMock().Object, authorizationServiceMock.Object);
        }

        private TicketsController CreateController(Mock<UserManager<IdentityUser>> userManagerMock, Mock<IAuthorizationService> authorizationServiceMock)
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            var loggerMock = new Mock<ILogger<TicketsController>>();
            return new TicketsController(ticketRepositoryMock.Object, projectRepositoryMock.Object, loggerMock.Object, userManagerMock.Object, authorizationServiceMock.Object);
        }
        private TicketsController CreateController(Mock<IGenericRepository<Ticket>> ticketRepositoryMock, Mock<UserManager<IdentityUser>> userManagerMock, Mock<IAuthorizationService> authorizationServiceMock)
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            var loggerMock = new Mock<ILogger<TicketsController>>();
            return new TicketsController(ticketRepositoryMock.Object, projectRepositoryMock.Object, loggerMock.Object, userManagerMock.Object, authorizationServiceMock.Object);
        }
        private TicketsController CreateController(Mock<IGenericRepository<Ticket>> ticketRepositoryMock, Mock<IGenericRepository<Project>> projectRepositoryMock,
            Mock<UserManager<IdentityUser>> userManagerMock, Mock<IAuthorizationService> authorizationServiceMock)
        {
            var loggerMock = new Mock<ILogger<TicketsController>>();
            return new TicketsController(ticketRepositoryMock.Object, projectRepositoryMock.Object, loggerMock.Object, userManagerMock.Object, authorizationServiceMock.Object);
        }
        #endregion

        #region Getting data
        private IEnumerable<Ticket> GetTestTickets()
        {
            return new List<Ticket>()
            {
                new Ticket()
                {
                    Id = 1,
                    OwnerId = "11111",
                    Title = "Ticket 1 title",
                    ShortDescription = "Short description 1",
                    LongDescription = "Long description 1",
                    Date = new DateTime(2022, 1, 1),
                    Priority = Priority.low,
                    Status = Status.bug,
                    Comments = new List<Comment>(){},
                    Project = GetTestProject(1)!,
                    ProjectId = 1
                },
                new Ticket()
                {
                    Id = 2,
                    OwnerId = "11111",
                    Title = "Ticket 2 title",
                    ShortDescription = "Short description 2",
                    LongDescription = "Long description 2",
                    Date = new DateTime(2022, 1, 1),
                    Priority = Priority.high,
                    Status = Status.feature,
                    Comments = new List<Comment>(){},
                    Project = GetTestProject(2)!,
                    ProjectId = 2
                }
            };
        }
        private Ticket? GetTestTicket(int id)
        {
            return GetTestTickets().Where(t => t.Id == id).FirstOrDefault();
        }
        private IEnumerable<Project> GetTestProjects()
        {
            return new List<Project>()
            {
                new Project()
                {
                    Id = 1,
                    Title = "Project 1 title",
                    Description = "Project 1 description",
                    OwnerId = "11111",
                    Tickets = new List<Ticket>()
                },
                new Project()
                {
                    Id = 2,
                    Title = "Project 2 title",
                    Description = "Project 2 description",
                    OwnerId = "11111",
                    Tickets = new List<Ticket>()
                }
            };
        }
        private Project? GetTestProject(int id)
        {
            return GetTestProjects().Where(p => p.Id == id).FirstOrDefault();
        }
        #endregion
    }
}
