using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bug_Tracker.Models;
using Bug_Tracker.Controllers;
using Xunit;
using Moq;
using Bug_Tracker.Data;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.AspNetCore.Http;

namespace Bug_Tracker_Tests
{
    public class CommentsControllerTest
    {

        [Fact]
        public void Index_ReturnsAViewResultWithAllComments_IfTicketExists()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<Expression<Func<Comment, bool>>>(), null)).Returns(GetTestComments());
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(1))!.Returns(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Index(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Comment>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }
        [Fact]
        public void Index_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<Expression<Func<Comment, bool>>>(), null)).Returns(GetTestComments());
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(3))!.Returns(GetTestTicket(3));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Index(3);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_Get_ReturnsAViewResult_IfTicketExists()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(1))!.Returns(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Create(1);

            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public void Create_Get_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(3))!.Returns(GetTestTicket(3));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Create(3);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_Post_CreatesTheComment_ReturnsARedirectToActionResult_IfTicketExistsAndModelStateValid()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(1))!.Returns(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Create(1, GetTestComment(1)!);

            commentRepositoryMock.Verify(r => r.Create(It.IsAny<Comment>()));
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public void Create_Post_ReturnsAViewResult_IfModelStateInValid()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(1))!.Returns(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);
            controller.ModelState.AddModelError("key", "message");

            var result = controller.Create(1, GetTestComment(1)!);

            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public void Create_Post_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(3))!.Returns(GetTestTicket(3));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Create(3, GetTestComment(3)!);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Get_ReturnsAViewResultWithAComment_IfTicketExistsAndCommentExists()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(1))!.Returns(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Edit(1, 1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Comment>(viewResult.ViewData.Model);
            Assert.Equal("Comment 1 text", model.Text);
        }
        [Fact]
        public void Edit_Get_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(3))!.Returns(GetTestTicket(3));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Edit(1, 3);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public void Edit_Get_ReturnsANotFoundResult_IfProjectDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntity(3)).Returns(GetTestComment(3));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(1))!.Returns(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Edit(3, 1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Post_EditsTheComment_ReturnsARedirectToActionResult_IfModelStateValidAndTicketExistsAndCommentExists()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(1))!.Returns(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Edit(1, 1, GetTestComment(1)!);

            commentRepositoryMock.Verify(r => r.Edit(It.IsAny<Comment>()));
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public void Edit_Post_ReturnsAViewResult_IfModelStateInvalid()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(1))!.Returns(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);
            controller.ModelState.AddModelError("key", "message");

            var result = controller.Edit(1, 1, GetTestComment(1)!);

            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public void Edit_Post_EditsTheComment_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(3))!.Returns(GetTestTicket(3));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Edit(1, 3, GetTestComment(1)!);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public void Edit_Post_EditsTheComment_ReturnsANotFoundResult_IfCommentDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntity(3)).Returns(GetTestComment(3));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(1))!.Returns(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Edit(3, 1, GetTestComment(3)!);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_Get_ReturnsAViewResult_IfTicketExistsAndCommentExists()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(1))!.Returns(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Delete(1, 1);

            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public void Delete_Get_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(3))!.Returns(GetTestTicket(3));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Delete(1, 3);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public void Delete_Get_ReturnsANotFoundResult_IfCommentDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntity(3)).Returns(GetTestComment(3));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(1))!.Returns(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Delete(3, 1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_Post_DeletesTheComment_ReturnsARedirectToActionResult_IfTicketExistsAndCommentExists()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(1))!.Returns(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Delete(1, 1, new FormCollection(null));

            commentRepositoryMock.Verify(r => r.Delete(1));
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public void Delete_Post_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(3))!.Returns(GetTestTicket(3));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Delete(1, 3, new FormCollection(null));

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public void Delete_Post_ReturnsANotFoundResult_IfCommentExistsAndCommentExists()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntity(3))!.Returns(GetTestTicket(3));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = controller.Delete(1, 3, new FormCollection(null));

            Assert.IsType<NotFoundResult>(result);
        }






        private CommentsController CreateController(Mock<IGenericRepository<Comment>> commentRepositoryMock, Mock<IGenericRepository<Ticket>> ticketRepositoryMock)
        {
            var loggerMock = new Mock<ILogger<CommentsController>>();

            return new CommentsController(commentRepositoryMock.Object, ticketRepositoryMock.Object, loggerMock.Object);
        }

        private IEnumerable<Comment> GetTestComments()
        {
            return new List<Comment>()
            {
                new Comment()
                {
                    Id = 1,
                    Text = "Comment 1 text",
                    Date = new DateTime(2022,1,1),
                    TicketId = 1,
                    Ticket = GetTestTicket(1)!
                },
                new Comment()
                {
                    Id = 2,
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

