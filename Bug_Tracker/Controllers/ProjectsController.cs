using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bug_Tracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ILogger<ProjectsController> _logger;
        IGenericRepository<Project> _projectRepository;
        public ProjectsController(IGenericRepository<Project> projectRepository, ILogger<ProjectsController> logger)
        {
            _logger = logger;
            _projectRepository = projectRepository;
        }

        // GET: Projects
        public ActionResult Index()
        {
            _logger.LogInformation("GET: Projects");
            IEnumerable<Project> projects = _projectRepository.Get();
            return View(projects);
        }

        // GET: Projects/Details/5
        public ActionResult Details(int id)
        {
            _logger.LogInformation("GET: Projects/Details/{id}", id);
            Project? project = _projectRepository.GetEntity(id);
            if (project == null)
            {
                return NotFound();
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
        public ActionResult Create([Bind("Title,Description")] Project project)
        {
            project.Tickets = new List<Ticket>();
            ModelState.ClearValidationState("Tickets");
            ModelState.MarkFieldValid("Tickets");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("POST: Projects/Create");
                _projectRepository.Create(project);
                _projectRepository.Save();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                _logger.LogInformation("POST: Projects/Create - model state invalid");
                return View();
            }
        }

        // GET: Projects/Edit/5
        public ActionResult Edit(int id)
        {
            _logger.LogInformation("GET: Projects/Edit/{id}", id);
            Project? project = _projectRepository.GetEntity(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind("Title,Description")] Project project)
        {
            ModelState.ClearValidationState("Tickets");
            ModelState.MarkFieldValid("Tickets");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("POST: Projects/Edit/{id}", id);
                Project projectToEdit = _projectRepository.GetEntity(id)!;
                projectToEdit.Title = project.Title;
                projectToEdit.Description = project.Description;
                try
                {
                    _projectRepository.Edit(projectToEdit);
                    _projectRepository.Save();

                    return RedirectToAction(nameof(Details), new { id = id });
                }
                catch (Exception ex)
                {
                    _logger.LogError("POST: Projects/Edit/{id} Exception {exception}", id, ex.Message);
                    // TODO: Error page
                    return RedirectToAction(nameof(Details), new { id = id });
                }
            }
            else
            {
                _logger.LogInformation("POST: Projects/Edit - model state invalid");
                return View(project);
            }
        }

        // GET: Projects/Delete/5
        public ActionResult Delete(int id)
        {
            _logger.LogInformation("GET: Projects/Delete/{id}", id);
            Project? project = _projectRepository.GetEntity(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            _logger.LogInformation("POST: Projects/Delete/{id}", id);
            try
            {
                _projectRepository.Delete(id);
                _projectRepository.Save();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError("POST: Projects/Delete/{id} - Exception: {exception}", id, ex.Message);
                // TODO: Error page
                return RedirectToAction(nameof(Details));
            }
        }
    }
}
