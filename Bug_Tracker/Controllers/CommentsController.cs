using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bug_Tracker.Controllers
{
    public class CommentsController : Controller
    {
        IGenericRepository<Comment> _commentRepository;
        IGenericRepository<Ticket> _ticketRepository;
        private readonly ILogger<CommentsController> _logger;
        public CommentsController(IGenericRepository<Comment> commentRepository, IGenericRepository<Ticket> ticketRepository, ILogger<CommentsController> logger)
        {
            _commentRepository = commentRepository;
            _ticketRepository = ticketRepository;
            _logger = logger;
        }

        // GET: Comments?ticketId=1
        public ActionResult Index(int ticketId)
        {
            _logger.LogInformation("GET: Comments?ticketId={ticketId}", ticketId);

            if (_ticketRepository.GetEntity(ticketId) == null)
            {
                return NotFound();
            }

            ViewData["TicketId"] = ticketId;
            ViewData["TicketTitle"] = _ticketRepository.GetEntity(ticketId)!.Title;

            List<Comment> comments = _commentRepository.Get(filter: c => c.TicketId == ticketId).ToList();
            return View(comments);
        }

        // GET: Comments/Create?ticketId=1
        public ActionResult Create(int ticketId)
        {
            _logger.LogInformation("GET: Comments/Create?ticketId={ticketId}", ticketId);

            if (_ticketRepository.GetEntity(ticketId) == null)
            {
                return NotFound();
            }

            ViewData["TicketId"] = ticketId;
            return View();
        }

        // POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int ticketId, [Bind("Text")] Comment comment)
        {
            if (_ticketRepository.GetEntity(ticketId) == null)
            {
                return NotFound();
            }

            ViewData["TicketId"] = ticketId;

            ModelState.ClearValidationState("Ticket");
            ModelState.MarkFieldValid("Ticket");
            if(ModelState.IsValid)
            {
                _logger.LogInformation("POST: Comments/Create (ticketId={ticketId})", ticketId);

                comment.TicketId = ticketId;
                comment.Date = DateTime.Now;
                _commentRepository.Create(comment);
                _commentRepository.Save();
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

            if (_ticketRepository.GetEntity(ticketId) == null)
            {
                return NotFound();
            }

            ViewData["TicketId"] = ticketId;

            Comment? comment = _commentRepository.GetEntity(id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, int ticketId, [Bind("Text")] Comment comment)
        {
            if (_ticketRepository.GetEntity(ticketId) == null)
            {
                return NotFound();
            }

            ViewData["TicketId"] = ticketId;

            ModelState.ClearValidationState("Ticket");
            ModelState.MarkFieldValid("Ticket");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("POST: Comments/Edit/{id} (ticketId={ticketId})", id, ticketId);

                Comment? commentToEdit = _commentRepository.GetEntity(id);
                if (commentToEdit == null)
                {
                    return NotFound();
                }
                commentToEdit.Text = comment.Text;
                try
                {
                    _commentRepository.Edit(commentToEdit);
                    _commentRepository.Save();
                    return RedirectToAction(nameof(Index), new { ticketId = ticketId });
                }
                catch (Exception ex)
                {
                    _logger.LogError("POST: Comments/Edit/{id} (ticketId={ticketId}) - Exception: {exception}", id, ticketId, ex.Message);
                    // TODO: Error page
                    return RedirectToAction(nameof(Index), new { ticketId = ticketId });
                }
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

            if (_ticketRepository.GetEntity(ticketId) == null)
            {
                return NotFound();
            }

            ViewData["TicketId"] = ticketId;

            Comment? comment = _commentRepository.GetEntity(id);
            if (comment == null)
            {
                return NotFound();
            }

            return View();
        }

        // POST: Comments/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, int ticketId, IFormCollection collection)
        {
            _logger.LogInformation("POST: Comments/Delete/{id} (ticketId={ticketId})", id, ticketId);

            if (_ticketRepository.GetEntity(ticketId) == null)
            {
                return NotFound();
            }
            if (_commentRepository.GetEntity(id) == null)
            {
                return NotFound();
            }

            ViewData["TicketId"] = ticketId;

            try
            {
                _commentRepository.Delete(id);
                _commentRepository.Save();
                return RedirectToAction(nameof(Index), new { ticketId = ticketId });
            }
            catch (Exception ex)
            {
                _logger.LogError("POST: Comments/Delete/{id} - Exception: {exception}", id, ex.Message);
                // TODO: Error page
                return RedirectToAction(nameof(Index), new { ticketId = ticketId });
            }
        }
    }
}
