using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bug_Tracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProjectsController> _logger;
        public ProjectsController(ApplicationDbContext context, ILogger<ProjectsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ProjectsController
        public ActionResult Index()
        {
            _logger.LogInformation("GET: ProjectsController/Index");
            return View(_context.Projects.ToList());
        }

        // GET: ProjectsController/Details/5
        public ActionResult Details(int id)
        {
            _logger.LogInformation("GET: ProjectsController/Details/{id}", id);
            return View(_context.Projects.Where(project => project.Id == id).First());
        }

        // GET: ProjectsController/Create
        public ActionResult Create()
        {
            _logger.LogInformation("GET: ProjectsController/Create");
            return View();
        }

        // POST: ProjectsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Title,Description")] Project project)
        {
            try
            {
                _logger.LogInformation("POST: ProjectsController/Create");
                Project newProject = project;
                newProject.Tickets = new List<Ticket>();
                _context.Projects.Add(newProject);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("POST: ProjectsController/Create - Exception: {exception}", ex);
                return View();
            }
        }

        // GET: ProjectsController/Edit/5
        public ActionResult Edit(int id)
        {
            _logger.LogInformation("GET: ProjectsController/Edit/{id}", id);
            return View(_context.Projects.Where(project => project.Id == id).First());
        }

        // POST: ProjectsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind("Title,Description")] Project project)
        {
            try
            {
                _logger.LogInformation("POST: ProjectsController/Edit/{id}", id);
                _context.Projects.Where(project => project.Id == id).First().Title = project.Title;
                _context.Projects.Where(project => project.Id == id).First().Description = project.Description;
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("POST: ProjectsController/Edit/{id} - Exception: {exception}", id, ex);
                return View();
            }
        }

        // GET: ProjectsController/Delete/5
        public ActionResult Delete(int id)
        {
            _logger.LogInformation("GET: ProjectsController/Delete/{id}", id);
            return View(_context.Projects.Where(project => project.Id == id).First());
        }

        // POST: ProjectsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                _logger.LogInformation("POST: ProjectsController/Delete/{id}", id);
                _context.Projects.Remove(_context.Projects.Where(project => project.Id == id).First());
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("POST: ProjectsController/Delete/{id} Exception {exception}", id, ex);
                return View();
            }
        }
    }
}
