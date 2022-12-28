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

namespace Bug_Tracker_Tests
{
    public class ProjectsConrollerTest
    {
        [Fact]
        public void Index_ReturnsAViewResultWithAllProjects()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.Get()).Returns(GetTestProjects());
            var loggerMock = new Mock<ILogger<ProjectsController>>();
            ProjectsController controller = new ProjectsController(repositoryMock.Object, loggerMock.Object);

            var result = controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Project>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public void Details_ReturnsAViewResultWithAProject()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestProject(1));
            var loggerMock = new Mock<ILogger<ProjectsController>>();
            ProjectsController controller = new ProjectsController(repositoryMock.Object, loggerMock.Object);

            var result = controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Project>(viewResult.ViewData.Model);
            Assert.Equal("Project 1 title", model.Title);
        }

        [Fact]
        public void Create_Get_ReturnsAViewResult()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            var loggerMock = new Mock<ILogger<ProjectsController>>();
            ProjectsController controller = new ProjectsController(repositoryMock.Object, loggerMock.Object);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Post_CreatesAndSavesAProject_ReturnsARedirectToActionResult_IfModelValid()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            ProjectsController controloller = CreateController(repositoryMock);
            Project project = GetTestProject(1)!;

            var result = controloller.Create(project);

            repositoryMock.Verify(r => r.Create(project));
            repositoryMock.Verify(r => r.Save());
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public void Create_Post_ReturnsAViewResult_IfModelInvalid()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            ProjectsController controloller = CreateController(repositoryMock);
            controloller.ModelState.AddModelError("Key", "ErrorMessage");
            Project? invalidProject = new Project();

            var result = controloller.Create(invalidProject);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Edit_Get_ReturnsAViewResultWithTheProject_IfProjectExists()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestProject(1));
            ProjectsController controller = CreateController(repositoryMock);

            var result = controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Project>(viewResult.ViewData.Model);
            Assert.Equal("Project 1 title", model.Title);
        }

        [Fact]
        public void Edit_Get_ReturnsANotFoundResult_IfProjectDoesntExist()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestProject(3));
            ProjectsController controller = CreateController(repositoryMock);

            var result = controller.Edit(1);

            var viewResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Post_EditsAndSavesTheProject_ReturnsARedirectToActionResult_IfModelValid()
        {
            Project project = GetTestProject(1)!;
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntity(1)).Returns(project);
            ProjectsController controller = CreateController(repositoryMock);

            var result = controller.Edit(1, project);

            repositoryMock.Verify(r => r.Edit(It.IsAny<Project>()));
            repositoryMock.Verify(r => r.Save());
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public void Edit_Post_ReturnsAViewResultWithTheProject_IfModelInvalid()
        {
            Project project = GetTestProject(1)!;
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntity(1)).Returns(project);
            ProjectsController controller = CreateController(repositoryMock);
            controller.ModelState.AddModelError("Key", "Message");

            var result = controller.Edit(1, project);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Project>(viewResult.ViewData.Model);
            Assert.Equal("Project 1 title", model.Title);
        }

        [Fact]
        public void Delete_Get_ReturnsAViewResultWithTheProject_IfProjectExists()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntity(1)).Returns(GetTestProject(1));
            ProjectsController controller = CreateController(repositoryMock);

            var result = controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Project>(viewResult.ViewData.Model);
            Assert.Equal("Project 1 title", model.Title);
        }
        [Fact]
        public void Delete_Get_ReturnsANotFoundResult_IfProjectDoesntExist()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.GetEntity(3)).Returns(GetTestProject(3));
            ProjectsController controller = CreateController(repositoryMock);

            var result = controller.Delete(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_Post_DeletesTheProject_ReturnsARedirectToActionResult_IfProjectExists()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            ProjectsController controller = CreateController(repositoryMock);

            var result = controller.Delete(1, new FormCollection(null));

            repositoryMock.Verify(r => r.Delete(1), Times.Once);
            repositoryMock.Verify(r => r.Save());
            Assert.IsType<RedirectToActionResult>(result);
        }
        [Fact]
        public void Delete_Post_ReturnsARedirectToActionResult_IfProjectDoesntExists()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            repositoryMock.Setup(r => r.Delete(3)).Throws(new InvalidOperationException("The entity to be deleted does not exist"));
            ProjectsController controller = CreateController(repositoryMock);

            var result = controller.Delete(3, new FormCollection(null));

            Assert.IsType<RedirectToActionResult>(result);
        }





        private ProjectsController CreateController()
        {
            var repositoryMock = new Mock<IGenericRepository<Project>>();
            var loggerMock = new Mock<ILogger<ProjectsController>>();
            return new ProjectsController(repositoryMock.Object, loggerMock.Object);
        }
        private ProjectsController CreateController(Mock<IGenericRepository<Project>> repositoryMock)
        {
            var loggerMock = new Mock<ILogger<ProjectsController>>();
            return new ProjectsController(repositoryMock.Object, loggerMock.Object);
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
