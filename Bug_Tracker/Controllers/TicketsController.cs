using Bug_Tracker.Authorization;
using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Net.Sockets;

namespace Bug_Tracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ILogger<TicketsController> _logger;
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly IGenericRepository<Project> _projectRepository;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        public TicketsController(IGenericRepository<Ticket> ticketRepository, IGenericRepository<Project> projectRepository, ILogger<TicketsController> logger, UserManager<IdentityUser> userManager, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _ticketRepository = ticketRepository;
            _projectRepository = projectRepository;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        // GET: Tickets
        //public async Task<IActionResult> Index(string sortOrder, string searchString, string statusFilter, string priorityFilter, string projectFilter, int page = 1, string ownersTickets = "false")
        public async Task<IActionResult> Index(string sortOrder, string searchString, string statusFilter, string priorityFilter, string projectFilter, int page = 1, bool ownersTickets = false)
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
            ViewData["OwnersTickets"] = ownersTickets;

            ViewData["Projects"] = await GetProjectTitlesAsync();

            List<string> includeProperties = new List<string>() { "Project" };

            List<Expression<Func<Ticket, bool>>> filters = new List<Expression<Func<Ticket, bool>>>();
            Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>>? orderBy = null;

            //if (ownersTickets == "true")
            if (ownersTickets)
            {
                filters.Add(t => t.OwnerId == _userManager.GetUserId(User));
            }
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                filters.Add(t => t.Title.ToLower().Contains(searchString.ToLower())
                               || t.ShortDescription.ToLower().Contains(searchString.ToLower()));
            }
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "any")
            {
                filters.Add(t => t.Status == (Status)Enum.Parse(typeof(Status), statusFilter));
            }
            if (!string.IsNullOrEmpty(priorityFilter) && priorityFilter != "any")
            {
                filters.Add(t => t.Priority == (Priority)Enum.Parse(typeof(Priority), priorityFilter));
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


            IEnumerable<Ticket> ticketsToDisplay = await _ticketRepository.GetAsync(page, elementsOnPage, includeProperties, filters, orderBy);

            int pages = (int)Math.Ceiling((decimal)ticketsToDisplay.Count() / (decimal)elementsOnPage);
            ViewData["Pages"] = pages;

            return View(ticketsToDisplay);
        }

        // GET: Tickets/Create
        public async Task<ActionResult> Create()
        {
            _logger.LogInformation("GET: Tickets/Create");

            ViewData["ProjectTitles"] = await GetProjectTitlesAsync();
            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Title,ShortDescription,LongDescription,Status,Priority")] Ticket ticket, string projectTitle)
        {
            ViewData["ProjectTitles"] = await GetProjectTitlesAsync();

            ticket.Date = DateTime.Now;
            ticket.Comments = new List<Comment>();
            ticket.OwnerId = _userManager.GetUserId(User);

            Project? project = await _projectRepository.GetEntityAsync(p => p.Title == projectTitle);
            if (project == null)
            {
                return NotFound();
            }
            ticket.ProjectId = project.Id;

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(User, ticket, TicketOperationAuthorizationRequirements.Create);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            ModelState.ClearValidationState("Comments");
            ModelState.MarkFieldValid("Comments");
            ModelState.ClearValidationState("Project");
            ModelState.MarkFieldValid("Project");
            ModelState.ClearValidationState("OwnerId");
            ModelState.MarkFieldValid("OwnerId");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("POST: Tickets/Create");

                await _ticketRepository.CreateAsync(ticket);
                await _ticketRepository.SaveAsync();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                _logger.LogInformation("POST: Tickets/Create - model state invalid");

                return View();
            }
        }

        // GET: Tickets/Details/1
        public async Task<ActionResult> Details(int id)
        {
            _logger.LogInformation("GET: Tickets/Details/{id}", id);

            Ticket? ticket = await _ticketRepository.GetEntityAsync(t => t.Id == id, new List<string>() { "Project", "Comments" });
            if (ticket == null)
            {
                return NotFound();
            }

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(User, ticket, TicketOperationAuthorizationRequirements.Read);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return View(ticket);
        }

        // GET: Tickets/Edit/1
        public async Task<ActionResult> Edit(int id)
        {
            _logger.LogInformation("GET: Tickets/Edit/{id}", id);

            var projectTitles = await GetProjectTitlesAsync();
            ViewData["ProjectTitles"] = projectTitles;

            Ticket? ticket = await _ticketRepository.GetEntityAsync(t => t.Id == id, new List<string>() { "Project" });
            if (ticket == null)
            {
                return NotFound();
            }

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(User, ticket, TicketOperationAuthorizationRequirements.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return View(ticket);
        }

        // POST: Tickets/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("Title,ShortDescription,LongDescription,Status,Priority")] Ticket ticket, string projectTitle)
        {
            Ticket? ticketToEdit = await _ticketRepository.GetEntityAsync(id);
            if (ticketToEdit == null)
            {
                return NotFound();
            }
            ticketToEdit.Title = ticket.Title;
            ticketToEdit.ShortDescription = ticket.ShortDescription;
            ticketToEdit.LongDescription = ticket.LongDescription;
            ticketToEdit.Status = ticket.Status;
            ticketToEdit.Priority = ticket.Priority;
            Project? project = await _projectRepository.GetEntityAsync(p => p.Title == projectTitle);
            if (project == null)
            {
                return NotFound();
            }
            ticketToEdit.Project = project;
            ticketToEdit.ProjectId = project.Id;

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(User, ticketToEdit, TicketOperationAuthorizationRequirements.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            ModelState.ClearValidationState("Comments");
            ModelState.MarkFieldValid("Comments");
            ModelState.ClearValidationState("Project");
            ModelState.MarkFieldValid("Project");
            ModelState.ClearValidationState("OwnerId");
            ModelState.MarkFieldValid("OwnerId");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("POST: Tickets/Edit/{id}", id);
                await _ticketRepository.EditAsync(ticketToEdit);
                await _ticketRepository.SaveAsync();

                return RedirectToAction(nameof(Details), new { id = id });
            }
            else
            {
                _logger.LogInformation("POST: Tickets/Edit/{id} - model state invalid", id);

                List<string> projectTitles = await GetProjectTitlesAsync();
                ViewData["ProjectTitles"] = projectTitles;
                
                return View(ticketToEdit);
            }
        }

        // GET: Tickets/Delete/1
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("GET: Tickets/Delete/{id}", id);

            Ticket? ticket = await _ticketRepository.GetEntityAsync(t => t.Id == id, new List<string>() { "Project" });
            if (ticket == null)
            {
                return NotFound();
            }

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(User, ticket, TicketOperationAuthorizationRequirements.Delete);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return View(ticket);
        }

        // Post: Tickets/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection formCollection)
        {
            _logger.LogInformation("POST: Tickets/Delete/{id}", id);

            Ticket? ticket = await _ticketRepository.GetEntityAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(User, ticket, TicketOperationAuthorizationRequirements.Delete);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            await _ticketRepository.DeleteAsync(id);
            await _ticketRepository.SaveAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<List<string>> GetProjectTitlesAsync()
        {
            return (from p in await _projectRepository.GetAsync()
                    select p.Title)
                    .ToList();
        }
    }
}
