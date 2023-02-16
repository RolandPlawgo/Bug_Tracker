using Bug_Tracker.Authorization;
using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bug_Tracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ILogger<ProjectsController> _logger;
        private readonly IGenericRepository<Project> _projectRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        public ProjectsController(IGenericRepository<Project> projectRepository, ILogger<ProjectsController> logger, UserManager<IdentityUser> userManager, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _projectRepository = projectRepository;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        // GET: Projects
        public async Task<ActionResult> Index()
        {
            _logger.LogInformation("GET: Projects");
            IEnumerable<Project> projects = await _projectRepository.GetAsync();
            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<ActionResult> Details(int id)
        {
            _logger.LogInformation("GET: Projects/Details/{id}", id);
            Project? project = await _projectRepository.GetEntityAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, project, ProjectOperationAuthorizationRequirements.Read);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return View(project);
        }

        // GET: Projects/Create
        public ActionResult Create()
        {
            _logger.LogInformation("GET: Projects/Create");
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async  Task<ActionResult> Create([Bind("Title,Description")] Project project)
        {
            project.Tickets = new List<Ticket>();

            project.OwnerId = _userManager.GetUserId(User);

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, project, ProjectOperationAuthorizationRequirements.Create);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            ModelState.ClearValidationState("Tickets");
            ModelState.MarkFieldValid("Tickets");
            ModelState.ClearValidationState("OwnerId");
            ModelState.MarkFieldValid("OwnerId");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("POST: Projects/Create");
                await _projectRepository.CreateAsync(project);
                await _projectRepository.SaveAsync();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                _logger.LogInformation("POST: Projects/Create - model state invalid");
                return View();
            }
        }

        // GET: Projects/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            _logger.LogInformation("GET: Projects/Edit/{id}", id);
            Project? project = await _projectRepository.GetEntityAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, project, ProjectOperationAuthorizationRequirements.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async  Task<ActionResult> Edit(int id, [Bind("Title,Description")] Project project)
        {
            Project? projectToEdit = await _projectRepository.GetEntityAsync(id)!;
            if (projectToEdit == null)
            {
                return NotFound();
            }
            projectToEdit.Title = project.Title;
            projectToEdit.Description = project.Description;

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, projectToEdit, ProjectOperationAuthorizationRequirements.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            ModelState.ClearValidationState("Tickets");
            ModelState.MarkFieldValid("Tickets");
            ModelState.ClearValidationState("OwnerId");
            ModelState.MarkFieldValid("OwnerId");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("POST: Projects/Edit/{id}", id);
                await _projectRepository.EditAsync(projectToEdit);
                await _projectRepository.SaveAsync();

                return RedirectToAction(nameof(Details), new { id = id });
            }
            else
            {
                _logger.LogInformation("POST: Projects/Edit - model state invalid");
                return View(project);
            }
        }

        // GET: Projects/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("GET: Projects/Delete/{id}", id);
            Project? project = await _projectRepository.GetEntityAsync(id);

            if (project == null)
            {
                _logger.LogInformation("GET: Projects/Delete/{id} - Project doesn't exist", id);
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, project, ProjectOperationAuthorizationRequirements.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            _logger.LogInformation("POST: Projects/Delete/{id}", id);

            Project? project = await _projectRepository.GetEntityAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            var isAuthorized = await _authorizationService.AuthorizeAsync(User, project, ProjectOperationAuthorizationRequirements.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            await _projectRepository.DeleteAsync(id);
            await _projectRepository.SaveAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
