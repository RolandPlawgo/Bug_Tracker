using System;
using Xunit;
using Bug_Tracker.Models;
using Bug_Tracker.Controllers;
using Moq;
using Bug_Tracker.Data;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Bug_Tracker_Tests.Controllers
{
    public class HomeControllerTests
    {
        [Fact]
        public async Task Index_ReturnsAViewResultWithAHomeViewModel()
        {
            var loggerMock = new Mock<ILogger<HomeController>>();
            var store = new Mock<IUserStore<IdentityUser>>();
            var userManagerMock = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("11111");
            var projectRepositoryMock = new Mock<IGenericRepository<Project>>();
            //projectRepositoryMock.Setup(p => p.GetAsync()).ReturnsAsync(GetTestProjects());
            projectRepositoryMock.Setup(p => p.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<Expression<Func<Project, bool>>>(), It.IsAny<Func<IQueryable<Project>, IOrderedQueryable<Project>>>()))
                .ReturnsAsync(GetTestProjects());
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(p => p.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<Expression<Func<Ticket, bool>>>(), It.IsAny<Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>>()))
                .ReturnsAsync(GetTestTickets());
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(p => p.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<Expression<Func<Comment, bool>>>(), It.IsAny<Func<IQueryable<Comment>, IOrderedQueryable<Comment>>>()))
                .ReturnsAsync(GetTestComments());
            var controller = new HomeController(loggerMock.Object, ticketRepositoryMock.Object, commentRepositoryMock.Object, userManagerMock.Object, projectRepositoryMock.Object);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<HomeVieweModel>(viewResult.ViewData.Model);
        }





        private IEnumerable<Comment> GetTestComments()
        {
            return new List<Comment>()
            {
                new Comment()
                {
                    Id = 1,
                    OwnerId = "11111",
                    Text = "Comment 1 text",
                    Date = new DateTime(2022,1,1),
                    TicketId = 1,
                    Ticket = GetTestTicket(1)!
                },
                new Comment()
                {
                    Id = 2,
                    OwnerId = "11111",
                    Text = "Comment 2 text",
                    Date = new DateTime(2022,2,1),
                    TicketId = 2,
                    Ticket = GetTestTicket(2)!
                }
            };
        }
        private Comment? GetTestComment(int id)
        {
            return GetTestComments().Where(c => c.Id == id).FirstOrDefault();
        }
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
    }
}
