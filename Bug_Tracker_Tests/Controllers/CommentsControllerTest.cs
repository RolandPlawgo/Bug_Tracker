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
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Bug_Tracker_Tests.Controllers
{
    public class CommentsControllerTest
    {
        #region Index tests
        [Fact]
        public async Task Index_ReturnsAViewResultWithAllComments_IfTicketExists()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<Expression<Func<Comment, bool>>>(), null)).ReturnsAsync(GetTestComments());
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Index(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Comment>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }
        [Fact]
        public async Task Index_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<Expression<Func<Comment, bool>>>(), null)).ReturnsAsync(GetTestComments());
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(3))!.ReturnsAsync(GetTestTicket(3));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Index(3);

            Assert.IsType<NotFoundResult>(result);
        }
        #endregion

        #region Cerate tests
        [Fact]
        public async Task Create_Get_ReturnsAViewResult_IfTicketExists()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(ticketRepositoryMock);

            var result = await controller.Create(1);

            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async Task Create_Get_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(3))!.ReturnsAsync(GetTestTicket(3));
            var controller = CreateController(ticketRepositoryMock);

            var result = await controller.Create(3);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_Post_CreatesTheComment_ReturnsARedirectToActionResult_IfTicketExistsAndModelStateValid()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock, UserManagerMock("11111"), AuthorizationServiceMock(true));

            var result = await controller.Create(1, GetTestComment(1)!);

            commentRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Comment>()));
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public async Task Create_Post_ReturnsAViewResult_IfModelStateInValid()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(ticketRepositoryMock, UserManagerMock("11111"), AuthorizationServiceMock(true));
            controller.ModelState.AddModelError("key", "message");

            var result = await controller.Create(1, GetTestComment(1)!);

            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async Task Create_Post_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(3))!.ReturnsAsync(GetTestTicket(3));
            var controller = CreateController(ticketRepositoryMock, UserManagerMock("11111"), AuthorizationServiceMock(true));

            var result = await controller.Create(3, GetTestComment(3)!);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Create_Post_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock, UserManagerMock("11111"), AuthorizationServiceMock(false));

            var result = await controller.Create(1, GetTestComment(1)!);

            Assert.IsType<ForbidResult>(result);
        }
        #endregion

        #region Edit tests
        [Fact]
        public async Task Edit_Get_ReturnsAViewResultWithAComment_IfTicketExistsAndCommentExists()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Edit(1, 1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Comment>(viewResult.ViewData.Model);
            Assert.Equal("Comment 1 text", model.Text);
        }
        [Fact]
        public async Task Edit_Get_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(3))!.ReturnsAsync(GetTestTicket(3));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Edit(1, 3);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Edit_Get_ReturnsANotFoundResult_IfProjectDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(3)).ReturnsAsync(GetTestComment(3));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Edit(3, 1);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Edit_Get_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock, AuthorizationServiceMock(false));

            var result = await controller.Edit(1, 1);

            var viewResult = Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Edit_Post_EditsTheComment_ReturnsARedirectToActionResult_IfModelStateValidAndTicketExistsAndCommentExists()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Edit(1, 1, GetTestComment(1)!);

            commentRepositoryMock.Verify(r => r.EditAsync(It.IsAny<Comment>()));
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public async Task Edit_Post_ReturnsAViewResult_IfModelStateInvalid()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);
            controller.ModelState.AddModelError("key", "message");

            var result = await controller.Edit(1, 1, GetTestComment(1)!);

            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async Task Edit_Post_EditsTheComment_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(3))!.ReturnsAsync(GetTestTicket(3));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Edit(1, 3, GetTestComment(1)!);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Edit_Post_EditsTheComment_ReturnsANotFoundResult_IfCommentDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(3)).ReturnsAsync(GetTestComment(3));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Edit(3, 1, GetTestComment(3)!);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Edit_Post_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock, AuthorizationServiceMock(false));

            var result = await controller.Edit(1, 1, GetTestComment(1)!);

            Assert.IsType<ForbidResult>(result);
        }
        #endregion

        #region Delete tests
        [Fact]
        public async Task Delete_Get_ReturnsAViewResult_IfTicketExistsAndCommentExists()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Delete(1, 1);

            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async Task Delete_Get_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(3))!.ReturnsAsync(GetTestTicket(3));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Delete(1, 3);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Delete_Get_ReturnsANotFoundResult_IfCommentDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(3)).ReturnsAsync(GetTestComment(3));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Delete(3, 1);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Delete_Get_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock, AuthorizationServiceMock(false));

            var result = await controller.Delete(1, 1);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Delete_Post_DeletesTheComment_ReturnsARedirectToActionResult_IfTicketExistsAndCommentExists()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Delete(1, 1, new FormCollection(null));

            commentRepositoryMock.Verify(r => r.DeleteAsync(1));
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public async Task Delete_Post_ReturnsANotFoundResult_IfTicketDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(3))!.ReturnsAsync(GetTestTicket(3));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Delete(1, 3, new FormCollection(null));

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Delete_Post_ReturnsANotFoundResult_IfCommentDoesntExist()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(3)).ReturnsAsync(GetTestComment(3));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock);

            var result = await controller.Delete(3, 1, new FormCollection(null));

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Delete_Post_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();
            commentRepositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestComment(1));
            var ticketRepositoryMock = new Mock<IGenericRepository<Ticket>>();
            ticketRepositoryMock.Setup(r => r.GetEntityAsync(1))!.ReturnsAsync(GetTestTicket(1));
            var controller = CreateController(commentRepositoryMock, ticketRepositoryMock, AuthorizationServiceMock(false));

            var result = await controller.Delete(1, 1, new FormCollection(null));

            Assert.IsType<ForbidResult>(result);
        }
        #endregion





        #region Mocking
        /// <summary>
        /// Mocks IAuthorizationService.
        /// </summary>
        /// <param name="succeeded">The value returned by AuthorizeAsync</param>
        private Mock<IAuthorizationService> AuthorizationServiceMock(bool succeeded)
        {
            Mock<IAuthorizationService> authorizationServiceMock = new Mock<IAuthorizationService>();
            if (succeeded)
            {
                authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.Is<object?>(o => o.GetType() == typeof(Comment)), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                    .ReturnsAsync(AuthorizationResult.Success);
            }
            else
            {
                authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.Is<object?>(o => o.GetType() == typeof(Comment)), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                    .ReturnsAsync(AuthorizationResult.Failed);
            }
            return authorizationServiceMock;
        }

        private Mock<UserManager<IdentityUser>> UserManagerMock()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
        }
        /// <summary>
        /// Mocks UserManager.
        /// </summary>
        /// <param name="id">The value returned by GetUserId</param>
        private Mock<UserManager<IdentityUser>> UserManagerMock(string id)
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            var userManagerMock = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);
            userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(id);
            return userManagerMock;
        }
        #endregion

        #region Create Controller
        private CommentsController CreateController(Mock<IGenericRepository<Ticket>> ticketRepositoryMock)
        {
            var loggerMock = new Mock<ILogger<CommentsController>>();
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();

            return new CommentsController(commentRepositoryMock.Object, ticketRepositoryMock.Object, loggerMock.Object, UserManagerMock().Object, AuthorizationServiceMock(true).Object);
        }
        private CommentsController CreateController(Mock<IGenericRepository<Comment>> commentRepositoryMock, Mock<IGenericRepository<Ticket>> ticketRepositoryMock)
        {
            var loggerMock = new Mock<ILogger<CommentsController>>();

            return new CommentsController(commentRepositoryMock.Object, ticketRepositoryMock.Object, loggerMock.Object, UserManagerMock().Object, AuthorizationServiceMock(true).Object);
        }
        private CommentsController CreateController(Mock<IGenericRepository<Comment>> commentRepositoryMock, Mock<IGenericRepository<Ticket>> ticketRepositoryMock, Mock<IAuthorizationService> authorizationServiceMock)
        {
            var loggerMock = new Mock<ILogger<CommentsController>>();

            return new CommentsController(commentRepositoryMock.Object, ticketRepositoryMock.Object, loggerMock.Object, UserManagerMock().Object, authorizationServiceMock.Object);
        }
        private CommentsController CreateController(Mock<IGenericRepository<Comment>> commentRepositoryMock, Mock<IGenericRepository<Ticket>> ticketRepositoryMock, Mock<UserManager<IdentityUser>> userManagerMock, Mock<IAuthorizationService> AuthorizationServiceMock)
        {
            var loggerMock = new Mock<ILogger<CommentsController>>();

            return new CommentsController(commentRepositoryMock.Object, ticketRepositoryMock.Object, loggerMock.Object, userManagerMock.Object, AuthorizationServiceMock.Object);
        }
        private CommentsController CreateController(Mock<IGenericRepository<Ticket>> ticketRepositoryMock, Mock<UserManager<IdentityUser>> userManagerMock, Mock<IAuthorizationService> AuthorizationServiceMock)
        {
            var loggerMock = new Mock<ILogger<CommentsController>>();
            var commentRepositoryMock = new Mock<IGenericRepository<Comment>>();

            return new CommentsController(commentRepositoryMock.Object, ticketRepositoryMock.Object, loggerMock.Object, userManagerMock.Object, AuthorizationServiceMock.Object);
        }
        #endregion



        #region Getting data
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
        #endregion
    }
}

