using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BeFit.Controllers
{
    [Authorize] // Only authenticated users can access this controller
    public class TrainingSessionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainingSessionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TrainingSessions
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var trainingSessions = await _context.TrainingSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

            return View(trainingSessions);
        }

        // GET: TrainingSessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var trainingSession = await _context.TrainingSessions
                .Include(t => t.CompletedExercises)
                .ThenInclude(e => e.ExerciseType)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (trainingSession == null)
            {
                return NotFound();
            }

            return View(trainingSession);
        }

        // GET: TrainingSessions/Create
        public IActionResult Create()
        {
            // Set default values for start and end time
            var trainingSession = new TrainingSession
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1),
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            return View(trainingSession);
        }

        // POST: TrainingSessions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartTime,EndTime,Notes,UserId")] TrainingSession trainingSession)
        {
            if (trainingSession.EndTime <= trainingSession.StartTime)
            {
                ModelState.AddModelError("EndTime", "Czas zakończenia musi być późniejszy niż czas rozpoczęcia");
            }


            if (ModelState.IsValid)
            {

                _context.Add(trainingSession);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(trainingSession);
        }

        // GET: TrainingSessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var trainingSession = await _context.TrainingSessions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (trainingSession == null)
            {
                return NotFound();
            }

            trainingSession.UserId = userId;

            return View(trainingSession);
        }

        // POST: TrainingSessions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartTime,EndTime,Notes,UserId")] TrainingSession trainingSession)
        {
            if (id != trainingSession.Id)
            {
                return NotFound();
            }

            // Verify ownership
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingSession = await _context.TrainingSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingSession == null || existingSession.UserId != userId)
            {
                return NotFound();
            }

            // Validate that EndTime is after StartTime
            if (trainingSession.EndTime <= trainingSession.StartTime)
            {
                ModelState.AddModelError("EndTime", "Czas zakończenia musi być późniejszy niż czas rozpoczęcia");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve the original UserId
                    trainingSession.UserId = userId;

                    _context.Update(trainingSession);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainingSessionExists(trainingSession.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(trainingSession);
        }

        // GET: TrainingSessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var trainingSession = await _context.TrainingSessions
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (trainingSession == null)
            {
                return NotFound();
            }

            return View(trainingSession);
        }

        // POST: TrainingSessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var trainingSession = await _context.TrainingSessions
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (trainingSession != null)
            {
                _context.TrainingSessions.Remove(trainingSession);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TrainingSessionExists(int id)
        {
            return _context.TrainingSessions.Any(e => e.Id == id);
        }
    }
}