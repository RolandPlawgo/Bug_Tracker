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

        // GET: Projects
        public ActionResult Index()
        {
            _logger.LogInformation("GET: Projects");
            return View(_context.Projects.ToList());
        }

        // GET: Projects/Details/5
        public ActionResult Details(int id)
        {
            _logger.LogInformation("GET: Projects/Details/{id}", id);
            return View(_context.Projects.Where(project => project.Id == id).First());
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
                _context.Projects.Add(project);
                _context.SaveChanges();

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
            return View(_context.Projects.Where(project => project.Id == id).First());
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
                var projectToEdit = _context.Projects.Where(project => project.Id == id).First();
                projectToEdit.Title = project.Title;
                projectToEdit.Description = project.Description;
                _context.SaveChanges();

                return RedirectToAction(nameof(Details), new {id });
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
            return View(_context.Projects.Where(project => project.Id == id).First());
        }

        // POST: Projects/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                _logger.LogInformation("POST: Projects/Delete/{id}", id);
                _context.Projects.Remove(_context.Projects.Where(project => project.Id == id).First());
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError("POST: Projects/Delete/{id} Exception {exception}", id, ex.Message);
                return View();
            }
        }
    }
}
