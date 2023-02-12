using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bug_Tracker.Data;
using Moq;
using Bug_Tracker.Models;
using Bug_Tracker.Controllers;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Core.Types;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Bug_Tracker_Tests.Controllers
{
    public class ProjectsConrollerTest
    {
        [Fact]
        public async Task Index_ReturnsAViewResultWithAllProjects()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(GetTestProjects());
            ProjectsController controller = CreateController(repositoryMock);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Project>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        #region Details tests
        [Fact]
        public async Task Details_ReturnsAViewResultWithAProject()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestProject(1));
            ProjectsController controller = CreateController(repositoryMock);

            var result = await controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Project>(viewResult.ViewData.Model);
            Assert.Equal("Project 1 title", model.Title);
        }
        [Fact]
        public async Task Details_ReturnsANotFoundResult_IfProjectDoesntExist()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(3)).ReturnsAsync(GetTestProject(3));
            ProjectsController controller = CreateController(repositoryMock);

            var result = await controller.Details(1);

            var viewResult = Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Details_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestProject(1));
            Mock<IAuthorizationService> authorizationServiceMock = AuthorizationServiceMock(false);
            ProjectsController controller = CreateController(repositoryMock, authorizationServiceMock);

            var result = await controller.Details(1);

            var viewResult = Assert.IsType<ForbidResult>(result);
        }
        #endregion

        #region Create tests
        [Fact]
        public void Create_Get_ReturnsAViewResult()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            ProjectsController controller = CreateController(repositoryMock);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_CreatesAndSavesAProject_ReturnsARedirectToActionResult_IfModelValid()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            ProjectsController controloller = CreateController(repositoryMock);
            Project project = GetTestProject(1)!;

            var result = await controloller.Create(project);

            repositoryMock.Verify(r => r.CreateAsync(project));
            repositoryMock.Verify(r => r.SaveAsync());
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public async Task Create_Post_ReturnsAViewResult_IfModelInvalid()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            ProjectsController controloller = CreateController(repositoryMock);
            controloller.ModelState.AddModelError("Key", "ErrorMessage");
            Project? invalidProject = new Project();

            var result = await controloller.Create(invalidProject);

            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async Task Create_Post_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            Mock<IAuthorizationService> authorizationServiceMock = AuthorizationServiceMock(false);
            ProjectsController controloller = CreateController(repositoryMock, authorizationServiceMock);
            Project project = GetTestProject(1)!;

            var result = await controloller.Create(project);

            Assert.IsType<ForbidResult>(result);
        }
        #endregion

        #region Edit tests
        [Fact]
        public async Task Edit_Get_ReturnsAViewResultWithTheProject_IfProjectExists()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestProject(1));
            ProjectsController controller = CreateController(repositoryMock);

            var result = await controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Project>(viewResult.ViewData.Model);
            Assert.Equal("Project 1 title", model.Title);
        }
        [Fact]
        public async Task Edit_Get_ReturnsANotFoundResult_IfProjectDoesntExist()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(3)).ReturnsAsync(GetTestProject(3));
            ProjectsController controller = CreateController(repositoryMock);

            var result = await controller.Edit(1);

            var viewResult = Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Edit_Get_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestProject(1));
            var authorizationServiceMock = AuthorizationServiceMock(false);
            ProjectsController controller = CreateController(repositoryMock, authorizationServiceMock);

            var result = await controller.Edit(1);

            var viewResult = Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Edit_Post_EditsAndSavesTheProject_ReturnsARedirectToActionResult_IfModelValid()
        {
            Project? project = GetTestProject(1);
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(project);
            ProjectsController controller = CreateController(repositoryMock);

            var result = await controller.Edit(1, project);

            repositoryMock.Verify(r => r.EditAsync(It.IsAny<Project>()));
            repositoryMock.Verify(r => r.SaveAsync());
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public async Task Edit_Post_ReturnsAViewResultWithTheProject_IfModelInvalid()
        {
            Project? project = GetTestProject(1);
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(project);
            ProjectsController controller = CreateController(repositoryMock);
            controller.ModelState.AddModelError("Key", "Message");

            var result = await controller.Edit(1, project);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Project>(viewResult.ViewData.Model);
            Assert.Equal("Project 1 title", model.Title);
        }
        public async Task Edit_PostReturnsANotFoundResult_IfProjectDoesntExist()
        {
            Project? project = GetTestProject(3);
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(3)).ReturnsAsync(project);
            ProjectsController controller = CreateController(repositoryMock);

            var result = await controller.Edit(3, project);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Edit_Post_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            Project? project = GetTestProject(1);
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(project);
            var authorizationServiceMock = AuthorizationServiceMock(false);
            ProjectsController controller = CreateController(repositoryMock, authorizationServiceMock);

            var result = await controller.Edit(1, project);

            Assert.IsType<ForbidResult>(result);
        }
        #endregion

        #region Delete tests
        [Fact]
        public async Task Delete_Get_ReturnsAViewResultWithTheProject_IfProjectExists()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestProject(1));
            ProjectsController controller = CreateController(repositoryMock);

            var result = await controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Project>(viewResult.ViewData.Model);
            Assert.Equal("Project 1 title", model.Title);
        }
        [Fact]
        public async Task Delete_Get_ReturnsANotFoundResult_IfProjectDoesntExist()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(3)).ReturnsAsync(GetTestProject(3));
            ProjectsController controller = CreateController(repositoryMock);

            var result = await controller.Delete(3);

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Delete_Get_ReturnsForbidResult_IfAuthorizationFailed()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestProject(1));
            var authorizationServiceMock = AuthorizationServiceMock(false);
            ProjectsController controller = CreateController(repositoryMock, authorizationServiceMock);

            var result = await controller.Delete(1);

            var viewResult = Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Delete_Post_DeletesTheProject_ReturnsARedirectToActionResult_IfProjectExists()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestProject(1));
            ProjectsController controller = CreateController(repositoryMock);

            var result = await controller.Delete(1, new FormCollection(null));

            repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
            repositoryMock.Verify(r => r.SaveAsync());
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public async Task Delete_Post_ReturnsANotFoundResult_IfProjectDoesntExists()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(3)).ReturnsAsync(GetTestProject(3));
            ProjectsController controller = CreateController(repositoryMock);

            var result = await controller.Delete(3, new FormCollection(null));

            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Delete_Post_ReturnsAForbidResult_IfAuthorizationFailed()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntityAsync(1)).ReturnsAsync(GetTestProject(1));
            var authorizationServiceMock = AuthorizationServiceMock(false);
            ProjectsController controller = CreateController(repositoryMock, authorizationServiceMock);

            var result = await controller.Delete(1, new FormCollection(null));

            Assert.IsType<ForbidResult>(result);
        }
        #endregion



        #region Mocking
        private Mock<IAuthorizationService> AuthorizationServiceMock(bool succeeded)
        {
            Mock<IAuthorizationService> authorizationServiceMock = new Mock<IAuthorizationService>();
            if (succeeded)
            {
                authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.Is<object?>(o => o.GetType() == typeof(Project)), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                    .ReturnsAsync(AuthorizationResult.Success);
            }
            else
            {
                authorizationServiceMock.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.Is<object?>(o => o.GetType() == typeof(Project)), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
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
        private ProjectsController CreateController()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            var loggerMock = new Mock<ILogger<ProjectsController>>();
            return new ProjectsController(repositoryMock.Object, loggerMock.Object, UserManagerMock().Object, AuthorizationServiceMock(true).Object);
        }
        private ProjectsController CreateController(Mock<IGenericRepository<Project>> repositoryMock)
        {
            var loggerMock = new Mock<ILogger<ProjectsController>>();
            return new ProjectsController(repositoryMock.Object, loggerMock.Object, UserManagerMock().Object, AuthorizationServiceMock(true).Object);
        }
        private ProjectsController CreateController(Mock<IGenericRepository<Project>> repositoryMock, Mock<IAuthorizationService> authorizationService)
        {
            var loggerMock = new Mock<ILogger<ProjectsController>>();
            return new ProjectsController(repositoryMock.Object, loggerMock.Object, UserManagerMock().Object, authorizationService.Object);
        }
        private ProjectsController CreateController(Mock<IGenericRepository<Project>> repositoryMock, Mock<UserManager<IdentityUser>> userManager, Mock<IAuthorizationService> authorizationService)
        {
            var loggerMock = new Mock<ILogger<ProjectsController>>();
            return new ProjectsController(repositoryMock.Object, loggerMock.Object, UserManagerMock().Object, AuthorizationServiceMock(true).Object);
        }
        #endregion

        #region Getting data
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
