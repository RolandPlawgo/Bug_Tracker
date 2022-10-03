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
        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Comments?ticketId=1
        public ActionResult Index(int ticketId)
        {
            ViewData["TicketId"] = ticketId;
            ViewData["TicketTitle"] = _context.Tickets.Where(t => t.Id == ticketId).First().Title;

            var comments = _context.Comments.Where(c => c.TicketId == ticketId);
            return View(comments.ToList());
        }

        // GET: Comments/Create?ticketId=1
        public ActionResult Create(int ticketId)
        {
            ViewData["TicketId"] = ticketId;
            return View();
        }

        // POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int ticketId, [Bind("Text")] Comment comment)
        {
            ViewData["TicketId"] = ticketId;
            try
            {
                _context.Tickets.Include(t => t.Comments).Where(t => t.Id == ticketId).First().Comments.Add(comment);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index), new {ticketId = ticketId});
            }
            catch
            {
                return View();
            }
        }

        // GET: Comments/Edit/1?ticketId=1
        public ActionResult Edit(int id, int ticketId)
        {
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
            try
            {
                _context.Comments.Where(c => c.Id == id).First().Text = comment.Text;
                _context.SaveChanges();
                return RedirectToAction(nameof(Index), new {ticketId = ticketId});
            }
            catch
            {
                return View();
            }
        }

        // GET: Comments/Delete/1?ticketId=1
        public ActionResult Delete(int id, int ticketId)
        {
            ViewData["TicketId"] = ticketId;
            return View();
        }

        // POST: Comments/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, int ticketId, IFormCollection collection)
        {
            ViewData["TicketId"] = ticketId;
            try
            {
                _context.Comments.Remove(_context.Comments.Where(c => c.Id == id).First());
                _context.SaveChanges();
                return RedirectToAction(nameof(Index), new {ticketId = ticketId});
            }
            catch
            {
                return View();
            }
        }
    }
}
