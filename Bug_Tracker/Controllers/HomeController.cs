using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Bug_Tracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        IGenericRepository<Ticket> _ticketRepository;
        IGenericRepository<Comment> _commentRepository;
        IGenericRepository<Project> _projectRepository;
        UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IGenericRepository<Ticket> ticketRepository, IGenericRepository<Comment> commentRepository, UserManager<IdentityUser> userManager, IGenericRepository<Project> projectRepository)
        {
            _logger = logger;
            _ticketRepository = ticketRepository;
            _commentRepository = commentRepository;
            _userManager = userManager;
            _projectRepository = projectRepository;
        }

        //GET: Home
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("GET: Home/Index");

            HomeVieweModel model = new HomeVieweModel();

            List<Ticket> latestUsersTickets = (await _ticketRepository.GetAsync(new List<string>() { "Project" }, filter: t => t.OwnerId == _userManager.GetUserId(User), orderBy: x => x.OrderByDescending(t => t.Date))).ToList();
            List<Comment> latestUsersComments = (await _commentRepository.GetAsync(new List<string>() { "Ticket" }, filter: c => c.OwnerId == _userManager.GetUserId(User), orderBy: c => c.OrderByDescending(t => t.Date))).ToList();
            List<Project> projects = new List<Project>();
            foreach (Ticket ticket in latestUsersTickets)
            {
                projects.Add(ticket.Project);
                if (projects.Count == 3)
                {
                    break;
                }
            }
            if (projects.Count < 3)
            {
                foreach (Comment comments in latestUsersComments)
                {
                    if (projects.Contains(await _projectRepository.GetEntityAsync(comments.Ticket.ProjectId)))
                    {
                        continue;
                    }
                    if (await _projectRepository.GetEntityAsync(comments.Ticket.ProjectId) != null)
                    {
                        projects.Add(await _projectRepository.GetEntityAsync(comments.Ticket.ProjectId));
                    }
                    if (projects.Count == 3)
                    {
                        break;
                    }
                }
            }
            if (projects.Count < 3)
            {
                foreach (Project project in await _projectRepository.GetAsync())
                {
                    if (projects.Contains(project))
                    {
                        continue;
                    }
                    projects.Add(project);
                    if (projects.Count == 3)
                    {
                        break;
                    }
                }
            }
            model.Projects = projects;


            // Latest comments on tickets owned by the user
            List<Comment> commentsToDisplay = (await _commentRepository.GetAsync(1, 5, new List<string>() { "Ticket" }, c => c.Ticket.OwnerId == _userManager.GetUserId(User), x => x.OrderByDescending(c => c.Date))).ToList();
            model.NewCommentsOnUsersTickets = commentsToDisplay;


            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}