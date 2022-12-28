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

namespace Bug_Tracker_Tests
{
    public class TicketsControllerTest
    {
        [Theory]
        [InlineData("", "any", "any", "any", 1, 0)]
        [InlineData("", "bug", "high", "any", 1, 2)]
        [InlineData("Xyz", "bug", "high", "Project Title", 1, 4)]
        public void Index_Get_CallsGetWithTheRightParameters_ReturnsAViewResultWithTickets(string searchString, string statusFilter, string priorityFilter, string projectFilter, int page, int numberOfFilters)
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            TicketsController controller = CreateController(ticketRepositoryMock);
            int pages = 0;

            var result = controller.Index("", searchString, statusFilter, priorityFilter, projectFilter, page);

            ticketRepositoryMock.Verify(r => r.Get(page, It.IsAny<int>(), out pages, "Project", It.Is<List<Expression<Func<Ticket, bool>>>>(l => l.Count() == numberOfFilters), It.IsAny<Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>>()));
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Ticket>>(viewResult.ViewData.Model);
        }
        [Fact]
        public void Index_Get_ReturnsARedirectToActionResult_IfArgumentExceptionThrown()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            int pages = 1;
            ticketRepositoryMock.Setup(r => r.Get(2, It.IsAny<int>(), out pages, It.IsAny<string>(), It.IsAny<List<Expression<Func<Ticket, bool>>>>(), It.IsAny<Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>>())).Throws(new InvalidOperationException(""));
            TicketsController controller = CreateController(ticketRepositoryMock);

            var result = controller.Index("", "", "", "", "", 2);

            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public void Create_Get_ReturnsAViewResult()
        {
            TicketsController controller = CreateController();

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Post_CreatesTheTicket_ReturnsARedirectToActionResult_IfProjectExistsAndModelStateValid()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).Returns(GetTestProject(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);
            Ticket ticket = GetTestTicket(1)!;

            var result = controller.Create(ticket, ticket.Title);

            ticketRepositoryMock.Verify(r => r.Create(It.IsAny<Ticket>()));
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public void Create_Post_ReturnsANotFoundResult_IfProjectDoesntExist()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).Returns(GetTestProject(3));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);
            Ticket ticket = GetTestTicket(1)!;

            var result = controller.Create(ticket, ticket.Title);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public void Create_Post_ReturnsAViewResult_IfModelInvalid()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).Returns(GetTestProject(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);
            controller.ModelState.AddModelError("key", "message");
            Ticket ticket = GetTestTicket(1)!;

            var result = controller.Create(ticket, ticket.Title);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Details_ReturnsAViewResultWithATicket_IfTicketExists()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).Returns(GetTestTicket(1));
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);

            var result = controller.Details(1);

            var viewResullt = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Ticket>(viewResullt.ViewData.Model);
            Assert.Equal("Ticket 1 title", model.Title);
        }
        [Fact]
        public void Details_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).Returns(GetTestTicket(3));
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);

            var result = controller.Details(3);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_ReturnsAViewResultWithATicket_IfTicketExists()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).Returns(GetTestTicket(1));
            var controller = CreateController(ticketRepositoryMock);

            var result = controller.Edit(1);

            var viewResullt = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Ticket>(viewResullt.ViewData.Model);
            Assert.Equal("Ticket 1 title", model.Title);
        }
        [Fact]
        public void Edit_Get_ReturnsANotFoundResult_IfTicketDoesntExists()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).Returns(GetTestTicket(3));
            var controller = CreateController(ticketRepositoryMock);

            var result = controller.Edit(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Post_EditsTheTicket_ReturnsARedirectToActionResult_IfModelValid()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).Returns(GetTestProject(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>())).Returns(GetTestTicket(1));
            ticketRepositoryMock.Setup(r => r.GetEntity(1)!).Returns(GetTestTicket(1)!);
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);

            var result = controller.Edit(1, GetTestTicket(1)!, GetTestProject(1)!.Title);

            ticketRepositoryMock.Verify(r => r.Edit(It.IsAny<Ticket>()));
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public void Edit_Post_ReturnsAViewResultWithATicket_IfModelInvalid()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).Returns(GetTestProject(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>())).Returns(GetTestTicket(1));
            ticketRepositoryMock.Setup(r => r.GetEntity(1)!).Returns(GetTestTicket(1)!);
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);
            controller.ModelState.AddModelError("key", "message");

            var result = controller.Edit(1, GetTestTicket(1)!, GetTestProject(1)!.Title);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<Ticket>(viewResult.ViewData.Model);
        }
        [Fact]
        public void Edit_Post_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).Returns(GetTestProject(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>())).Returns(GetTestTicket(3));
            ticketRepositoryMock.Setup(r => r.GetEntity(1)!).Returns(GetTestTicket(1)!);
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);

            var result = controller.Edit(3, GetTestTicket(1)!, GetTestProject(1)!.Title);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public void Edit_Post_ReturnsANotFoundResult_IfProjectDoesntExist()
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            projectRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<string>())).Returns(GetTestProject(3));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<string>())).Returns(GetTestTicket(1));
            ticketRepositoryMock.Setup(r => r.GetEntity(1)!).Returns(GetTestTicket(1)!);
            var controller = CreateController(ticketRepositoryMock, projectRepositoryMock);

            var result = controller.Edit(1, GetTestTicket(1)!, GetTestProject(1)!.Title);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_Get_ReturnsAViewResultWithATicket_IfTicketExists()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).Returns(GetTestTicket(1));
            var controller = CreateController(ticketRepositoryMock);

            var result = controller.Delete(1);

            var viewResullt = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Ticket>(viewResullt.ViewData.Model);
            Assert.Equal("Ticket 1 title", model.Title);
        }
        [Fact]
        public void Delete_Get_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(It.IsAny<Expression<Func<Ticket, bool>>>(), "Project")).Returns(GetTestTicket(3));
            var controller = CreateController(ticketRepositoryMock);

            var result = controller.Delete(3);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_Post_DeletesTheTicket_ReturnsARedirectToActionResult_IfTicketExists()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            var controller = CreateController(ticketRepositoryMock);

            var result = controller.Delete(1, new FormCollection(null));

            ticketRepositoryMock.Verify(r => r.Delete(1));
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public void Delete_Post_ReturnsARedirectToActionResult_IfTicketDoesntExist()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.Delete(3)).Throws(new InvalidOperationException("The entity to be deleted does not exist"));
            var controller = CreateController(ticketRepositoryMock);

            var result = controller.Delete(3, new FormCollection(null));

            Assert.IsType<RedirectToActionResult>(result);
        }





        private TicketsController CreateController(Mock<IGenericRepository<Ticket>> ticketRepositoryMock, Mock<IGenericRepository<Project>> projectRepositoryMock)
        {
            var loggerMock = new Mock<ILogger<TicketsController>>();
            return new TicketsController(ticketRepositoryMock.Object, projectRepositoryMock.Object, loggerMock.Object);
        }
        private TicketsController CreateController(Mock<IGenericRepository<Ticket>> ticketRepositoryMock)
        {
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            var loggerMock = new Mock<ILogger<TicketsController>>();
            return new TicketsController(ticketRepositoryMock.Object, projectRepositoryMock.Object, loggerMock.Object);
        }
        private TicketsController CreateController()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            var loggerMock = new Mock<ILogger<TicketsController>>();
            return new TicketsController(ticketRepositoryMock.Object, projectRepositoryMock.Object, loggerMock.Object);
        }
        private IEnumerable<Ticket> GetTestTickets()
        {
            return new List<Ticket>()
            {
                new Ticket()
                {
                    Id = 1,
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
                    Tickets = new List<Ticket>()
                },
                new Project()
                {
                    Id = 2,
                    Title = "Project 2 title",
                    Description = "Project 2 description",
                    Tickets = new List<Ticket>()
                }
            };
        }
        private Project? GetTestProject(int id)
        {
            return GetTestProjects().Where(p => p.Id == id).FirstOrDefault();
        }
    }
}
