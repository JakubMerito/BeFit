using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;

namespace BeFit.Controllers
{
    [Authorize]
    public class StatsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Stats
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var fourWeeksAgo = DateTime.Now.AddDays(-28);


            var exercisesInPeriod = await _context.CompletedExercises
                .Include(e => e.ExerciseType)
                .Include(e => e.TrainingSession)
                .Where(e => e.UserId == userId && e.TrainingSession.StartTime >= fourWeeksAgo)
                .ToListAsync();


            var statistics = exercisesInPeriod
                .GroupBy(e => e.ExerciseTypeId)
                .Select(group => new ExerciseStatistics
                {
                    ExerciseType = group.First().ExerciseType,
                    Count = group.Count(),
                    TotalRepetitions = group.Sum(e => e.Sets * e.Reps),
                    AverageWeight = group.Average(e => e.Weight),
                    MaxWeight = group.Max(e => e.Weight)
                })
                .OrderByDescending(s => s.Count)
                .ToList();


            var totalTimeInMinutes = exercisesInPeriod
                .Select(e => e.TrainingSession)
                .Distinct()
                .Sum(s => (s.EndTime - s.StartTime).TotalMinutes);

            ViewData["TotalWorkoutTime"] = totalTimeInMinutes;
            ViewData["TotalWorkouts"] = exercisesInPeriod
                .Select(e => e.TrainingSessionId)
                .Distinct()
                .Count();
            ViewData["DateRange"] = $"{fourWeeksAgo.ToString("dd.MM.yyyy")} - {DateTime.Now.ToString("dd.MM.yyyy")}";

            return View(statistics);
        }
    }
}
