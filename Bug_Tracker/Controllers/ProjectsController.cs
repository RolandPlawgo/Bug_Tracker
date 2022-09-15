using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bug_Tracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProjectsController
        public ActionResult Index()
        {
            return View(_context.Projects.ToList());
        }

        // GET: ProjectsController/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                return View(_context.Projects.Where(project => project.Id == id).First());
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: ProjectsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProjectsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Title,Description")] Project project)
        {
            try
            {
                Project newProject = project;
                newProject.Tickets = new List<Ticket>();
                _context.Projects.Add(newProject);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProjectsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View(_context.Projects.Where(project => project.Id == id).First());
        }

        // POST: ProjectsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind("Title,Description")] Project project)
        {
            try
            {
                _context.Projects.Where(project => project.Id == id).First().Title = project.Title;
                _context.Projects.Where(project => project.Id == id).First().Description = project.Description;
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProjectsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View(_context.Projects.Where(project => project.Id == id).First());
        }

        // POST: ProjectsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                _context.Projects.Remove(_context.Projects.Where(project => project.Id == id).First());
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
