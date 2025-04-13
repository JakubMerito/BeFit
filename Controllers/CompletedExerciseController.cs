using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BeFit.Controllers
{
    [Authorize] // Only authenticated users can access this controller
    public class CompletedExercisesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompletedExercisesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CompletedExercises
        public async Task<IActionResult> Index(int? sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If session ID is provided, show exercises for that session only
            var completedExercisesQuery = _context.CompletedExercises
                .Include(c => c.ExerciseType)
                .Include(c => c.TrainingSession)
                .Where(c => c.UserId == userId);

            if (sessionId.HasValue)
            {
                // Verify session belongs to user
                var session = await _context.TrainingSessions
                    .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

                if (session == null)
                {
                    return NotFound();
                }

                completedExercisesQuery = completedExercisesQuery.Where(c => c.TrainingSessionId == sessionId);
                ViewData["SessionId"] = sessionId;
                ViewData["SessionDate"] = session.StartTime.ToString("dd.MM.yyyy HH:mm");
            }

            var completedExercises = await completedExercisesQuery
                .OrderByDescending(c => c.TrainingSession.StartTime)
                .ToListAsync();

            return View(completedExercises);
        }

        // GET: CompletedExercises/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var completedExercise = await _context.CompletedExercises
                .Include(c => c.ExerciseType)
                .Include(c => c.TrainingSession)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (completedExercise == null)
            {
                return NotFound();
            }

            return View(completedExercise);
        }

        // GET: CompletedExercises/Create
        // Optional parameter for pre-selecting a training session
        public async Task<IActionResult> Create(int? sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewData["UserId"] = userId;
            var sessions = await _context.TrainingSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

            if (sessions.Count == 0)
            {
                TempData["ErrorMessage"] = "Najpierw musisz utworzyć sesję treningową.";
                return RedirectToAction("Create", "TrainingSessions");
            }

            ViewData["ExerciseTypeId"] = new SelectList(_context.ExerciseTypes, "Id", "Name");

            
            if (sessionId.HasValue && sessions.Any(s => s.Id == sessionId))
            {
                ViewData["TrainingSessionId"] = sessionId;
            }
            else
            {
                ViewData["TrainingSessionId"] = null;
            }

            return View();
        }

        // POST: CompletedExercises/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TrainingSessionId,ExerciseTypeId,Sets,Reps,Weight,Notes,UserId")] CompletedExercise completedExercise)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var session = await _context.TrainingSessions
                .FirstOrDefaultAsync(s => s.Id == completedExercise.TrainingSessionId && s.UserId == userId);

            if (session == null)
            {
                ModelState.AddModelError("TrainingSessionId", "Nieprawidłowa sesja treningowa");
            }

            Console.WriteLine(ModelState.IsValid);

            if (ModelState.IsValid)
            {
                _context.Add(completedExercise);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details",  "TrainingSessions", new { id = completedExercise.TrainingSessionId });
            }

            ViewData["ExerciseTypeId"] = new SelectList(_context.ExerciseTypes, "Id", "Name", completedExercise.ExerciseTypeId);
            ViewData["TrainingSessionId"] = new SelectList(
                _context.TrainingSessions.Where(s => s.UserId == userId),
                "Id", "StartTime", completedExercise.TrainingSessionId);

            return View(completedExercise);
        }

        // GET: CompletedExercises/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var completedExercise = await _context.CompletedExercises
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (completedExercise == null)
            {
                return NotFound();
            }

            ViewData["ExerciseTypeId"] = new SelectList(_context.ExerciseTypes, "Id", "Name", completedExercise.ExerciseTypeId);
            ViewData["TrainingSessionId"] = new SelectList(
                _context.TrainingSessions.Where(s => s.UserId == userId),
                "Id", "StartTime", completedExercise.TrainingSessionId);

            return View(completedExercise);
        }

        // POST: CompletedExercises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TrainingSessionId,ExerciseTypeId,Sets,Reps,Weight,Notes")] CompletedExercise completedExercise)
        {
            if (id != completedExercise.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify the exercise belongs to the user
            var existingExercise = await _context.CompletedExercises
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existingExercise == null || existingExercise.UserId != userId)
            {
                return NotFound();
            }

            // Verify the training session belongs to the user
            var session = await _context.TrainingSessions
                .FirstOrDefaultAsync(s => s.Id == completedExercise.TrainingSessionId && s.UserId == userId);

            if (session == null)
            {
                ModelState.AddModelError("TrainingSessionId", "Nieprawidłowa sesja treningowa");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set the user ID to preserve ownership
                    completedExercise.UserId = userId;

                    _context.Update(completedExercise);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompletedExerciseExists(completedExercise.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { sessionId = completedExercise.TrainingSessionId });
            }

            ViewData["ExerciseTypeId"] = new SelectList(_context.ExerciseTypes, "Id", "Name", completedExercise.ExerciseTypeId);
            ViewData["TrainingSessionId"] = new SelectList(
                _context.TrainingSessions.Where(s => s.UserId == userId),
                "Id", "StartTime", completedExercise.TrainingSessionId);

            return View(completedExercise);
        }

        // GET: CompletedExercises/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var completedExercise = await _context.CompletedExercises
                .Include(c => c.ExerciseType)
                .Include(c => c.TrainingSession)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (completedExercise == null)
            {
                return NotFound();
            }

            return View(completedExercise);
        }

        // POST: CompletedExercises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var completedExercise = await _context.CompletedExercises
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (completedExercise != null)
            {
                var sessionId = completedExercise.TrainingSessionId;
                _context.CompletedExercises.Remove(completedExercise);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { sessionId = sessionId });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CompletedExerciseExists(int id)
        {
            return _context.CompletedExercises.Any(e => e.Id == id);
        }
    }
}