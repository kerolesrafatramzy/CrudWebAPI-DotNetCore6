using Microsoft.AspNetCore.Mvc;


namespace movies_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMoviesService _moviesService;
        private readonly IGenresService _genresService;

        private List<string> _allowedExtentions = new() { ".jpg", ".png" };
        private long _maxAllowedPosterSize = 1048576;

        public MoviesController(IMoviesService moviesService, IGenresService genresService, IMapper mapper)
        {
            _moviesService = moviesService;
            _genresService = genresService;
            _mapper = mapper;
        }

        // Get All movies 
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movies = await _moviesService.GetAll();
            var dto = _mapper.Map<IEnumerable<MovieDetailsDto>>(movies);
            return Ok(dto);
        }


        // Get by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var movie = await _moviesService.GetById(id);

            if (movie == null)
                return NotFound("No movies was found with ID {id}");

            var dto = _mapper.Map<MovieDetailsDto>(movie);

            return Ok(dto);
        }

        // Get by GenreId
        [HttpGet("Category")]
        public async Task<IActionResult> GetByGenreId(byte genreId)
        {
            var movies = await _moviesService.GetAll(genreId);
            var dto = _mapper.Map<IEnumerable<MovieDetailsDto>>(movies);
            return Ok(dto);
        }

        // Add new movie
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MovieDto dto)
        {

            if (dto.Poster == null)
                return BadRequest("Poster is required!");

            if (!_allowedExtentions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                return BadRequest("Only .png and .jpg images are allowed!");

            if (dto.Poster.Length > _maxAllowedPosterSize)
                return BadRequest("Max allowed size for poster is 1 mb!");

            var IsValidGenre = await _genresService.IsvalidGenre(dto.GenreId);

            if (!IsValidGenre)
                return BadRequest("Invalid genra ID!");

            using var dataStream = new MemoryStream();

            await dto.Poster.CopyToAsync(dataStream);

            var movie = _mapper.Map<Movie>(dto);

            movie.Poster = dataStream.ToArray();

            await _moviesService.Add(movie);

            return Ok(movie);
        }

        // Update Movie
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MovieDto dto)
        {
            var movie = await _moviesService.GetById(id);

            if (movie == null)
                return NotFound($"No movies was found with ID {id}");

            var IsValidGenre = await _genresService.IsvalidGenre(dto.GenreId);

            if (!IsValidGenre)
                return BadRequest("Invalid genra ID!");

            if (dto.Poster != null)
            {
                if (!_allowedExtentions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                    return BadRequest("Only .png and .jpg images are allowed!");

                if (dto.Poster.Length > _maxAllowedPosterSize)
                    return BadRequest("Max allowed size for poster is 1 mb!");


                using var dataStream = new MemoryStream();

                await dto.Poster.CopyToAsync(dataStream);

                movie.Poster = dataStream.ToArray();
            }


            movie.Title = dto.Title;
            movie.GenreId = dto.GenreId;
            movie.Year = dto.Year;
            movie.Storyline = dto.Storyline; 
            movie.Rate = dto.Rate;

            _moviesService.Update(movie);

            return Ok(movie);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _moviesService.GetById(id);

            if (movie == null)
                return BadRequest($"No movies was found with ID {id}");

            _moviesService.Delete(movie);

            return Ok(movie);

        }

    }
}
