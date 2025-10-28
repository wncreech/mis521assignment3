using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Betterboxd.Data;
using Betterboxd.Models;
using Betterboxd.Services;

namespace Betterboxd.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppDbContext _context;

        public MoviesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movie
                .Include(m => m.Actors) // include related actors
                .ToListAsync();

            return View(movies);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
                return NotFound();

            var sentimentService = new SentimentService(Environment.GetEnvironmentVariable("HUGGINGFACE_API_KEY"));
            var (results, overall, average) = await sentimentService.AnalyzeSentimentForQueryAsync(movie.title, " " + movie.year);

            var viewModel = new SentimentViewModel
            {
                QueryTitle = movie.title,
                OverallSentiment = overall,
                AverageScore = average,
                Comments = results.Select(r => new Comment
                {
                    Text = r.Text,
                    Label = r.Label,
                    Score = r.Score
                }).ToList()
            };
            if (viewModel.QueryTitle == null) Console.WriteLine("Null return.");
            return View(viewModel);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            // Load all actors for the dropdown list
            ViewData["Actors"] = new MultiSelectList(_context.Actor, "Id", "Name");
            return View();
        }
        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,title,genre,year,imdbUrl,posterUrl")] Movie movie, int[] selectedActors)
        {
            if (ModelState.IsValid)
            {
                // Attach selected actors to the movie
                foreach (var actorId in selectedActors)
                {
                    var actor = await _context.Actor.FindAsync(actorId);
                    if (actor != null)
                    {
                        movie.Actors.Add(actor);
                    }
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, reload dropdown
            ViewData["Actors"] = new MultiSelectList(_context.Actor, "Id", "Name", selectedActors);
            return View(movie);
        }

        // GET: Movies/Edit/5
        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .Include(m => m.Actors) // include related actors
                .FirstOrDefaultAsync(m => m.id == id);

            if (movie == null)
            {
                return NotFound();
            }

            // Pre-select the movie's existing actors
            var selectedActorIds = movie.Actors.Select(a => a.Id).ToList();

            ViewData["Actors"] = new MultiSelectList(_context.Actor, "Id", "Name", selectedActorIds);
            return View(movie);
        }


        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,title,genre,year,imdbUrl,posterUrl")] Movie movie, int[] selectedActors)
        {
            if (id != movie.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMovie = await _context.Movie
                        .Include(m => m.Actors)
                        .FirstOrDefaultAsync(m => m.id == id);

                    if (existingMovie == null)
                    {
                        return NotFound();
                    }

                    // Update scalar fields
                    existingMovie.title = movie.title;
                    existingMovie.genre = movie.genre;
                    existingMovie.year = movie.year;
                    existingMovie.imdbUrl = movie.imdbUrl;
                    existingMovie.posterUrl = movie.posterUrl;

                    // Update actor associations
                    existingMovie.Actors.Clear();
                    foreach (var actorId in selectedActors)
                    {
                        var actor = await _context.Actor.FindAsync(actorId);
                        if (actor != null)
                        {
                            existingMovie.Actors.Add(actor);
                        }
                    }

                    _context.Update(existingMovie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.id))
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

            ViewData["Actors"] = new MultiSelectList(_context.Actor, "Id", "Name", selectedActors);
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.id == id);
        }


    }
}
