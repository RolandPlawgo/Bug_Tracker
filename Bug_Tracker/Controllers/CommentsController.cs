using Bug_Tracker.Authorization;
using Bug_Tracker.Data;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bug_Tracker.Controllers
{
    public class CommentsController : Controller
    {
        private readonly IGenericRepository<Comment> _commentRepository;
        private readonly IGenericRepository<Ticket> _ticketRepository;
        private readonly ILogger<CommentsController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        public CommentsController(IGenericRepository<Comment> commentRepository, IGenericRepository<Ticket> ticketRepository, ILogger<CommentsController> logger, UserManager<IdentityUser> userManager, IAuthorizationService authorizationService)
        {
            _commentRepository = commentRepository;
            _ticketRepository = ticketRepository;
            _logger = logger;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        // GET: Comments?ticketId=1
        public async Task<ActionResult> Index(int ticketId)
        {
            _logger.LogInformation("GET: Comments?ticketId={ticketId}", ticketId);

            if (await _ticketRepository.GetEntityAsync(ticketId) == null)
            {
                return NotFound();
            }

            ViewData["TicketId"] = ticketId;
            ViewData["TicketTitle"] = (await _ticketRepository.GetEntityAsync(ticketId))!.Title;

            List<Comment> comments = (await _commentRepository.GetAsync(filter: c => c.TicketId == ticketId)).ToList();
            return View(comments);
        }

        // GET: Comments/Create?ticketId=1
        public async Task<ActionResult> Create(int ticketId)
        {
            _logger.LogInformation("GET: Comments/Create?ticketId={ticketId}", ticketId);

            if (await _ticketRepository.GetEntityAsync(ticketId) == null)
            {
                return NotFound();
            }

            ViewData["TicketId"] = ticketId;
            return View();
        }

        // POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int ticketId, [Bind("Text")] Comment comment)
        {
            if (await _ticketRepository.GetEntityAsync(ticketId) == null)
            {
                return NotFound();
            }

            comment.OwnerId = _userManager.GetUserId(User);

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, comment, CommentOperationAuthorizationRequirements.Create);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            ViewData["TicketId"] = ticketId;

            ModelState.ClearValidationState("Ticket");
            ModelState.MarkFieldValid("Ticket");
            ModelState.ClearValidationState("OwnerId");
            ModelState.MarkFieldValid("OwnerId");
            if (ModelState.IsValid)
            {
                _logger.LogInformation("POST: Comments/Create (ticketId={ticketId})", ticketId);

                comment.TicketId = ticketId;
                comment.Date = DateTime.Now;
                await _commentRepository.CreateAsync(comment);
                await _commentRepository.SaveAsync();
                return RedirectToAction(nameof(Index), new {ticketId = ticketId});
            }
            else
            {
                _logger.LogInformation("POST: Comments/Create (ticketId={ticketId}) - model state invalid", ticketId);

                return View();
            }
        }

        // GET: Comments/Edit/1?ticketId=1
        public async Task<ActionResult> Edit(int id, int ticketId)
        {
            _logger.LogInformation("GET: Comments/Edit/{id}?ticketId={ticketId}", id, ticketId);

            if (await _ticketRepository.GetEntityAsync(ticketId) == null)
            {
                return NotFound();
            }

            ViewData["TicketId"] = ticketId;

            Comment? comment = await _commentRepository.GetEntityAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, comment, CommentOperationAuthorizationRequirements.Update);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return View(comment);
        }

        // POST: Comments/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, int ticketId, [Bind("Text")] Comment comment)
        {
            if (await _ticketRepository.GetEntityAsync(ticketId) == null)
            {
                return NotFound();
            }

            _logger.LogInformation("POST: Comments/Edit/{id} (ticketId={ticketId})", id, ticketId);

            Comment? commentToEdit = await _commentRepository.GetEntityAsync(id);
            if (commentToEdit == null)
            {
                return NotFound();
            }
            commentToEdit.Text = comment.Text;

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, commentToEdit, CommentOperationAuthorizationRequirements.Create);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            ViewData["TicketId"] = ticketId;

            ModelState.ClearValidationState("Ticket");
            ModelState.MarkFieldValid("Ticket");
            ModelState.ClearValidationState("OwnerId");
            ModelState.MarkFieldValid("OwnerId");
            if (ModelState.IsValid)
            {
                try
                {
                    await _commentRepository.EditAsync(commentToEdit);
                    await _commentRepository.SaveAsync();
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
        public async Task<ActionResult> Delete(int id, int ticketId)
        {
            _logger.LogInformation("GET: Comments/Delete/{id}?ticketId={ticketId}", id, ticketId);

            if (await _ticketRepository.GetEntityAsync(ticketId) == null)
            {
                return NotFound();
            }

            ViewData["TicketId"] = ticketId;

            Comment? comment = await _commentRepository.GetEntityAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, comment, CommentOperationAuthorizationRequirements.Delete);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return View();
        }

        // POST: Comments/Delete/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, int ticketId, IFormCollection collection)
        {
            _logger.LogInformation("POST: Comments/Delete/{id} (ticketId={ticketId})", id, ticketId);

            if (await _ticketRepository.GetEntityAsync(ticketId) == null)
            {
                return NotFound();
            }
            Comment? comment = await _commentRepository.GetEntityAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var isAuthorized = await _authorizationService.AuthorizeAsync(User, comment, CommentOperationAuthorizationRequirements.Create);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            ViewData["TicketId"] = ticketId;

            try
            {
                await _commentRepository.DeleteAsync(id);
                await _commentRepository.SaveAsync();
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
