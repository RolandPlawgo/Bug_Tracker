using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Bug_Tracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ILogger<TicketsController> _logger;
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IGenericRepository<Project> _projectRepository;
        public TicketsController(IGenericRepository<Ticket> ticketRepository, IGenericRepository<Project> projectRepository, ILogger<TicketsController> logger)
        {
            _logger = logger;
            _ticketRepository = ticketRepository;
            _projectRepository = projectRepository;
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

            ViewData["Projects"] = GetProjectTitles();


            string includeProperties = "";
            List<Expression<Func<Ticket, bool>>> filters = new List<Expression<Func<Ticket, bool>>>();
            Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>? orderBy = null;

            includeProperties = "Project";

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                filters.Add((t => t.Title.ToLower().Contains(searchString.ToLower())
                               || t.ShortDescription.ToLower().Contains(searchString.ToLower())));
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "any")
            {
                filters.Add((t => t.Status == (Status)Enum.Parse(typeof(Status), statusFilter)));
            }

            if (!string.IsNullOrEmpty(priorityFilter) && priorityFilter != "any")
            {
                filters.Add((t => t.Priority == (Priority)Enum.Parse(typeof(Priority), priorityFilter)));
            }

            if (!string.IsNullOrEmpty(projectFilter) && projectFilter != "any")
            {
                filters.Add(t => t.Project.Title == projectFilter);
            }

            switch (sortOrder)
            {
                case "title_asc":
                    orderBy = (tickets => tickets.OrderBy(t => t.Title));
                    break;
                case "title_desc":
                    orderBy = (tickets => tickets.OrderByDescending(t => t.Title));
                    break;
                case "priority_asc":
                    orderBy = (tickets => tickets.OrderBy(t => t.Priority));
                    break;
                case "priority_desc":
                    orderBy = (tickets => tickets.OrderByDescending(t => t.Priority));
                    break;
                case "date_asc":
                    orderBy = (tickets => tickets.OrderBy(t => t.Date));
                    break;
                default:
                    orderBy = (tickets => tickets.OrderByDescending(t => t.Date));
                    break;
            }


            int pages = 0;
            var ticketsToDisplay = _ticketRepository.Get(page, elementsOnPage, out pages, includeProperties, filters, orderBy);

            ViewData["Pages"] = pages;

            return View(ticketsToDisplay);
        }

        // GET: Tickets/Create
        public ActionResult Create()
        {
            _logger.LogInformation("GET: Tickets/Create");

            ViewData["ProjectTitles"] = GetProjectTitles();
            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Title,ShortDescription,LongDescription,Status,Priority")] Ticket ticket, string projectTitle)
        {
            ViewData["ProjectTitles"] = GetProjectTitles();

            ticket.Date = DateTime.Now;
            ticket.Comments = new List<Comment>();

            Project? project = _projectRepository.GetEntity(p => p.Title == projectTitle);
            if (project == null)
            {
                return NotFound();
            }
            ticket.ProjectId = project.Id;

            ModelState.ClearValidationState("Comments");
            ModelState.MarkFieldValid("Comments");
            ModelState.ClearValidationState("Project");
            ModelState.MarkFieldValid("Project");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("POST: Tickets/Create");

                _ticketRepository.Create(ticket);
                _ticketRepository.Save();

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

            Ticket? ticket = _ticketRepository.GetEntity(t => t.Id == id, "Project");
            if (ticket == null)
            {
                NotFound();
            }
            return View(ticket);
        }

        // GET: Tickets/Edit/1
        public ActionResult Edit(int id)
        {
            _logger.LogInformation("GET: Tickets/Edit/{id}", id);

            var projectTitles = GetProjectTitles();
            ViewData["ProjectTitles"] = projectTitles;

            Ticket? ticket = _ticketRepository.GetEntity(t => t.Id == id, "Project");
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind("Title,ShortDescription,LongDescription,Status,Priority")] Ticket ticket, string projectTitle)
        {
            var ticketToEdit = _ticketRepository.GetEntity(id)!;
            ticketToEdit.Title = ticket.Title;
            ticketToEdit.ShortDescription = ticket.ShortDescription;
            ticketToEdit.LongDescription = ticket.LongDescription;
            ticketToEdit.Status = ticket.Status;
            ticketToEdit.Priority = ticket.Priority;
            Project? project = _projectRepository.GetEntity(p => p.Title == projectTitle);
            if (project == null)
            {
                return NotFound();
            }
            ticketToEdit.Project = project;
            ticketToEdit.ProjectId = project.Id;

            ModelState.ClearValidationState("Comments");
            ModelState.MarkFieldValid("Comments");
            ModelState.ClearValidationState("Project");
            ModelState.MarkFieldValid("Project");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("POST: Tickets/Edit/{id}", id);
                try
                {
                    _ticketRepository.Edit(ticketToEdit);
                    _ticketRepository.Save();

                    return RedirectToAction(nameof(Details), new { id = id });
                }
                catch (Exception ex)
                {
                    _logger.LogError("POST: Tickets/Edit/{id} - Exception: {exception}", id, ex.Message);
                    // TODO: Error page
                    return RedirectToAction(nameof(Details), new { id = id });
                }
            }
            else
            {
                _logger.LogInformation("POST: Tickets/Edit/{id} - model state invalid", id);

                List<string> projectTitles = GetProjectTitles();
                ViewData["ProjectTitles"] = projectTitles;
                
                return View(ticketToEdit);
            }
        }

        // GET: Tickets/Delete/1
        public ActionResult Delete(int id)
        {
            _logger.LogInformation("GET: Tickets/Delete/{id}", id);

            Ticket? ticket = _ticketRepository.GetEntity(t => t.Id == id, "Project");
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        // Post: Tickets/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, IFormCollection formCollection)
        {
            _logger.LogInformation("POST: Tickets/Delete/{id}", id);

            try
            {
                _ticketRepository.Delete(id);
                _ticketRepository.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError("POST: Tickets/Delete/{id} - Exception: {exception}", id, ex.Message);
                // TODO: Error page
                return RedirectToAction(nameof(Details), new {id = id});
            }
        }


        public List<string> GetProjectTitles()
        {
            return (from p in _projectRepository.Get()
                    select p.Title)
                    .ToList();
        }
    }
}
