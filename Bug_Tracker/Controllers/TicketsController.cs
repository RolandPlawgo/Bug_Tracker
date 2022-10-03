using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketsController> _logger;
        public TicketsController(ApplicationDbContext context, ILogger<TicketsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Tickets
        public IActionResult Index(string sortOrder, string searchString, string statusFilter, string priorityFilter, string projectFilter, int page = 1)
        {
            _logger.LogInformation("GET: Tickets");

            int elementsOnPage = 10;

            ViewData["CurrentPage"] = page;

            ViewData["TitleSortParam"] = sortOrder == "title_asc" ? "title_desc" : "title_asc";
            ViewData["DateSortParam"] = sortOrder == "date_desc" ? "date_asc" : "date_desc";
            ViewData["PrioritySortParam"] = sortOrder == "priority_desc" ? "priority_asc" : "priority_desc";

            ViewData["SortOrder"] = sortOrder;
            ViewData["SearchString"] = searchString;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["PriorityFilter"] = priorityFilter;
            ViewData["ProjectFilter"] = projectFilter;

            List<string> projectTitles = new List<string>();
            foreach (var project in _context.Projects)
            {
                projectTitles.Add(project.Title);
            }
            ViewData["Projects"] = projectTitles;

            var tickets = from s in _context.Tickets.Include(t => t.Project)
                          select s;
            
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                tickets = tickets.Where(t => t.Title.ToLower().Contains(searchString.ToLower())
                                          || t.ShortDescription.ToLower().Contains(searchString.ToLower())
                                          || t.Project.Title.ToLower().Contains(searchString.ToLower()));
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "any")
            {
                tickets = tickets.Where(t => t.Status == (Status)Enum.Parse(typeof(Status), statusFilter));
            }

            if (!string.IsNullOrEmpty(priorityFilter) && priorityFilter != "any")
            {
                tickets = tickets.Where(t => t.Priority == (Priority)Enum.Parse(typeof(Priority), priorityFilter));
            }

            if (!string.IsNullOrEmpty(projectFilter) && projectFilter != "any")
            {
                tickets = tickets.Where(t => t.Project.Title == projectFilter);
            }

            switch (sortOrder)
            {
                case "title_asc":
                    tickets = tickets.OrderBy(t => t.Title);
                    break;
                case "title_desc":
                    tickets = tickets.OrderByDescending(t => t.Title);
                    break;
                case "priority_asc":
                    tickets = tickets.OrderBy(t => t.Priority);
                    break;
                case "priority_desc":
                    tickets = tickets.OrderByDescending(t => t.Priority);
                    break;
                case "date_asc":
                    tickets = tickets.OrderBy(t => t.Date);
                    break;
                default:
                    tickets = tickets.OrderByDescending(t => t.Date);
                    break;
            }

            ViewData["Pages"] = (int)Math.Ceiling((decimal)tickets.Count() / (decimal)elementsOnPage);

            List<Ticket> ticketsToDisplay = tickets.Skip((page - 1) * elementsOnPage).Take(elementsOnPage).ToList();

            return View(ticketsToDisplay);
        }

        // GET: Tickets/Create
        public ActionResult Create()
        {
            _logger.LogInformation("GET: Tickets/Create");

            List<string> projectTitles = (from p in _context.Projects 
                                          select p.Title)
                                          .ToList();
            ViewData["ProjectTitles"] = projectTitles;
            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Title,ShortDescription,LongDescription,Status,Priority")] Ticket ticket, string projectTitle)
        {
            List<string> projectTitles = (from p in _context.Projects
                                          select p.Title)
                                          .ToList();
            ViewData["ProjectTitles"] = projectTitles;

            ticket.Date = DateTime.Now;
            ticket.Comments = new List<Comment>();

            var project = _context.Projects.Include(p => p.Tickets).Where(p => p.Title == projectTitle).First();
            //ticket.Project = project;

            ModelState.ClearValidationState("Comments");
            ModelState.MarkFieldValid("Comments");
            ModelState.ClearValidationState("Project");
            ModelState.MarkFieldValid("Project");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("POST: Tickets/Create");

                project.Tickets.Add(ticket);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                _logger.LogInformation("POST: Tickets/Create - model state invalid");

                return View();
            }
        }

        // GET: Tickets/Details/1
        public ActionResult Details(int id)
        {
            _logger.LogInformation("GET: Tickets/Details/{id}", id);

            Ticket ticket = _context.Tickets.Include(t => t.Project).Where(t => t.Id == id).First();
            return View(ticket);
        }

        // GET: Tickets/Edit/1
        public ActionResult Edit(int id)
        {
            _logger.LogInformation("GET: Tickets/Edit/{id}", id);

            List<string> projectTitles = (from p in _context.Projects
                                          select p.Title)
                                          .ToList();
            ViewData["ProjectTitles"] = projectTitles;

            Ticket ticket = _context.Tickets.Include(t => t.Project).Where(t => t.Id == id).First();
            return View(ticket);
        }

        // POST: Tickets/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind("Title,ShortDescription,LongDescription,Status,Priority")] Ticket ticket, string projectTitle)
        {
            var ticketToEdit = _context.Tickets.Include(t => t.Project).Where(t => t.Id == id).First();
            ticketToEdit.Title = ticket.Title;
            ticketToEdit.ShortDescription = ticket.ShortDescription;
            ticketToEdit.LongDescription = ticket.LongDescription;
            ticketToEdit.Status = ticket.Status;
            ticketToEdit.Priority = ticket.Priority;
            ticketToEdit.Project = _context.Projects.Where(p => p.Title == projectTitle).First();
            ticketToEdit.ProjectId = ticketToEdit.Project.Id;

            ModelState.ClearValidationState("Comments");
            ModelState.MarkFieldValid("Comments");
            ModelState.ClearValidationState("Project");
            ModelState.MarkFieldValid("Project");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("POST: Tickets/Edit/{id}", id);

                _context.SaveChanges();

                return RedirectToAction(nameof(Details), new { id = id });
            }
            else
            {
                _logger.LogInformation("POST: Tickets/Edit/{id} - model state invalid", id);

                List<string> projectTitles = (from p in _context.Projects
                                              select p.Title)
                                          .ToList();
                ViewData["ProjectTitles"] = projectTitles;
                
                return View(ticketToEdit);
            }
        }

        // GET: Tickets/Delete/1
        public ActionResult Delete(int id)
        {
            _logger.LogInformation("GET: Tickets/Delete/{id}", id);

            Ticket ticket = _context.Tickets.Include(t => t.Project).Where(t => t.Id == id).First();
            return View(ticket);
        }

        // Post: Tickets/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, IFormCollection formCollection)
        {
            _logger.LogInformation("POST: Tickets/Delete/{id}", id);

            _context.Tickets.Remove(_context.Tickets.Where(t => t.Id == id).First());
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
