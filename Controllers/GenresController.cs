using Microsoft.AspNetCore.Mvc;


namespace movies_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly IGenresService _genresService;

        public GenresController(IGenresService genresService)
        {
            _genresService = genresService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var genres = await _genresService.GetAll();

            return Ok(genres);
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(GenreDto dto)
        {
            Genre genre = new() 
            {
                Name = dto.Name 
            };
            await _genresService.Add(genre);
            return Ok(genre);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(byte id, [FromBody] GenreDto dto)
        {
            var genre = await _genresService.GetById(id);

            if (genre == null)
                return NotFound($"No genres was found with ID {id}");

            genre.Name = dto.Name;

            _genresService.Update(genre);

            return Ok(genre);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(byte id)
        {
            var genre = await _genresService.GetById(id);

            if (genre == null)
                return NotFound($"No genres was found with ID {id}");

           _genresService.Delete(genre);

            return Ok(genre);

        }
    }

}
