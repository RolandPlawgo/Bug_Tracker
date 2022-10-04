using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bug_Tracker.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommentsController> _logger;
        public CommentsController(ApplicationDbContext context, ILogger<CommentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Comments?ticketId=1
        public ActionResult Index(int ticketId)
        {
            _logger.LogInformation("GET: Comments?ticketId={ticketId}", ticketId);

            ViewData["TicketId"] = ticketId;
            ViewData["TicketTitle"] = _context.Tickets.Where(t => t.Id == ticketId).First().Title;

            var comments = _context.Comments.Where(c => c.TicketId == ticketId);
            return View(comments.ToList());
        }

        // GET: Comments/Create?ticketId=1
        public ActionResult Create(int ticketId)
        {
            _logger.LogInformation("GET: Comments/Create?ticketId={ticketId}", ticketId);

            ViewData["TicketId"] = ticketId;
            return View();
        }

        // POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int ticketId, [Bind("Text")] Comment comment)
        {
            ViewData["TicketId"] = ticketId;

            ModelState.ClearValidationState("Ticket");
            ModelState.MarkFieldValid("Ticket");
            if(ModelState.IsValid)
            {
                _logger.LogInformation("POST: Comments/Create (ticketId={ticketId})", ticketId);

                _context.Tickets.Include(t => t.Comments).Where(t => t.Id == ticketId).First().Comments.Add(comment);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index), new {ticketId = ticketId});
            }
            else
            {
                _logger.LogInformation("POST: Comments/Create (ticketId={ticketId}) - model state invalid", ticketId);

                return View();
            }
        }

        // GET: Comments/Edit/1?ticketId=1
        public ActionResult Edit(int id, int ticketId)
        {
            _logger.LogInformation("GET: Comments/Edit/{id}?ticketId={ticketId}", id, ticketId);

            ViewData["TicketId"] = ticketId;

            Comment comment = _context.Comments.Where(c => c.Id == id).First();

            return View(comment);
        }

        // POST: Comments/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, int ticketId, [Bind("Text")] Comment comment)
        {
            ViewData["TicketId"] = ticketId;

            ModelState.ClearValidationState("Ticket");
            ModelState.MarkFieldValid("Ticket");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("POST: Comments/Edit/{id} (ticketId={ticketId})", id, ticketId);

                _context.Comments.Where(c => c.Id == id).First().Text = comment.Text;
                _context.SaveChanges();
                return RedirectToAction(nameof(Index), new { ticketId = ticketId });
            }
            else
            {
                _logger.LogInformation("POST: Comments/Edit/{id} (ticketId={ticketId}) - model state invalid", id, ticketId);

                return View();
            }
        }


        // GET: Comments/Delete/1?ticketId=1
        public ActionResult Delete(int id, int ticketId)
        {
            _logger.LogInformation("GET: Comments/Delete/{id}?ticketId={ticketId}", id, ticketId);

            ViewData["TicketId"] = ticketId;
            return View(_context.Comments.Where(c => c.Id == id).First());
        }

        // POST: Comments/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, int ticketId, IFormCollection collection)
        {
            _logger.LogInformation("POST: Comments/Delete/{id} (ticketId={ticketId})", id, ticketId);

            ViewData["TicketId"] = ticketId;

            _context.Comments.Remove(_context.Comments.Where(c => c.Id == id).First());
            _context.SaveChanges();
            return RedirectToAction(nameof(Index), new { ticketId = ticketId });
        }
    }
}
