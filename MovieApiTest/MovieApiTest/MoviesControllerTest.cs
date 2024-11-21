using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using MovieApi.Models;
using MovieApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace MovieApi.Tests
{
    [TestClass]
    public class MoviesControllerTests
    {
        private DbContextOptions<MovieContext> _contextOptions;

        [TestInitialize]
        public void Initialize()
        {
            _contextOptions = new DbContextOptionsBuilder<MovieContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            // Ensure the in-memory database is cleared before each test
            using var context = new MovieContext(_contextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        [TestMethod]
        public async Task AddMovieAsync_ShouldAddMovieSuccessfully()
        {
            // Arrange
            using var context = new MovieContext(_contextOptions);
            var controller = new MoviesController(context);

            var newMovie = new Movie
            {
                Title = "Inception",
                Genre = "Sci-Fi",
                ReleaseDate = new DateTime(2010, 7, 16)
            };

            // Act
            var addedMovie = await controller.PostMovie(newMovie);

            // Assert
            Assert.IsNotNull(addedMovie);
            Assert.AreEqual(1, context.Movies.CountAsync().Result);
            var retrievedMovie = await context.Movies.FirstAsync();
            Assert.AreEqual("Inception", retrievedMovie.Title);
        }
        [TestMethod]
        public async Task GetMovieAsync_ShouldReturnMovieById()
        {
            // Arrange
            using var context = new MovieContext(_contextOptions);
            context.Movies.Add(new Movie { Title = "Existing Movie", Genre = "Drama", ReleaseDate = DateTime.Now });
            await context.SaveChangesAsync();

            var controller = new MoviesController(context);
            var movieId = 1; // Assuming the first movie has ID 1

            // Act
            var result = await controller.GetMovie(movieId); // result is of type ActionResult<Movie>
            var movie = result.Value; // Access the actual Movie object


            // Assert
            Assert.IsNotNull(movie);
            Assert.AreEqual("Existing Movie", movie.Title);
        }
        [TestMethod]
        public async Task UpdateMovieAsync_ShouldModifyExistingMovie()
        {
            // Arrange
            using var context = new MovieContext(_contextOptions);
            var movie = new Movie { Title = "Old Title", Genre = "Drama", ReleaseDate = DateTime.Now };
            context.Movies.Add(movie);
            await context.SaveChangesAsync();

            var controller = new MoviesController(context);
            var movieToUpdate = await context.Movies.FirstAsync();
            movieToUpdate.Title = "Updated Title";

            // Act
            await controller.PutMovie(movieToUpdate.Id, movieToUpdate);
            await context.SaveChangesAsync();

            // Assert
            var updatedMovie = await context.Movies.FirstOrDefaultAsync(m => m.Id == movieToUpdate.Id);
            Assert.IsNotNull(updatedMovie);
            Assert.AreEqual("Updated Title", updatedMovie.Title);
        }
        [TestMethod]
        public async Task DeleteMovieAsync_ShouldRemoveMovieFromDatabase()
        {
            // Arrange
            using var context = new MovieContext(_contextOptions);
            var movie = new Movie { Title = "To Be Deleted", Genre = "Thriller", ReleaseDate = DateTime.Now };
            context.Movies.Add(movie);
            await context.SaveChangesAsync();

            var controller = new MoviesController(context);
            var movieId = movie.Id;

            // Act
            await controller.DeleteMovie(movieId);
            await context.SaveChangesAsync();

            // Assert
            var deletedMovie = await context.Movies.FirstOrDefaultAsync(m => m.Id == movieId);
            Assert.IsNull(deletedMovie);
        }
        [TestMethod]
        public async Task GetAllMoviesAsync_ShouldReturnAllMovies()
        {
            // Arrange
            using var context = new MovieContext(_contextOptions);


            // Seed the in-memory database with movies
            context.Movies.AddRange(
                new Movie { Title = "Movie 1", Genre = "Drama", ReleaseDate = DateTime.Now.AddYears(-2) },
                new Movie { Title = "Movie 2", Genre = "Action", ReleaseDate = DateTime.Now.AddYears(-1) }
            );
            await context.SaveChangesAsync();

            var controller = new MoviesController(context);

            // Act
            var result = await controller.GetMovies(); // result is of type ActionResult<IEnumerable<Movie>>
            var movies = result.Value; // Access the actual list of movies

            // Assert
            Assert.IsNotNull(movies); // Ensure movies list is not null
            Assert.AreEqual(2, movies.Count()); // Validate the count of movies
            Assert.AreEqual("Movie 1", movies.First().Title); // Validate details of the first movie
        }
        [TestMethod]
        public async Task GetMovieAsync_ShouldReturnNotFound_WhenMovieDoesNotExist()
        {
            // Arrange
            using var context = new MovieContext(_contextOptions);

            // No movies seeded in the database
            var controller = new MoviesController(context);
            int nonExistentMovieId = 99;

            // Act
            var result = await controller.GetMovie(nonExistentMovieId);

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult)); // Ensure NotFoundResult is returned
        }


        [TestMethod]
        public async Task PutMovie_ShouldReturnBadRequest_WhenIdDoesNotMatchMovieId()
        {
            // Arrange
            using var context = new MovieContext(_contextOptions);
            var controller = new MoviesController(context);

            // Add a movie to the database
            var existingMovie = new Movie
            {
                Id = 1,
                Title = "Original Title",
                Genre = "Action",
                ReleaseDate = new DateTime(2023, 1, 1)
            };

            context.Movies.Add(existingMovie);
            await context.SaveChangesAsync();

            // Create a movie object with a different Id
            var updatedMovie = new Movie
            {
                Id = 2, // Does not match the route parameter
                Title = "Updated Title",
                Genre = "Drama",
                ReleaseDate = new DateTime(2024, 1, 1)
            };

            // Act
            var result = await controller.PutMovie(1, updatedMovie); // Route id = 1, Movie Id = 2

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


    }
}
