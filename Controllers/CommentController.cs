using api.Dtos.Comment;
using api.Interfaces;
using api.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/comment")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IStockRepository _stockRepo;
        public CommentController(ICommentRepository commentRepo , IStockRepository stockRepo)
        {
           _commentRepo = commentRepo;
           _stockRepo = stockRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var comments = await _commentRepo.GetAllAsync();
            var commentInDto = comments.Select(s=> s.ToCommentDto());
            return Ok(commentInDto);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comment = await _commentRepo.GetByIdAsync(id);
            if(comment==null)
            {
                return NotFound();
            }
            return Ok(comment.ToCommentDto());
        }

        [HttpPost("{stockId:int}")]
        public async Task<IActionResult> Create([FromRoute] int stockId, CreateCommentDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if(!await _stockRepo.StockExist(stockId))
            {
                return BadRequest("Stock not exist");
            }

            var commentModel = createDto.ToCreateCommentDtoFromComment(stockId);
            await _commentRepo.CreateAsync(commentModel);

            return CreatedAtAction(nameof(GetById) , new { id = commentModel} , commentModel.ToCommentDto());

        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var comment = await _commentRepo.DeleteAsync(id);
            if (comment==null)
            {
                return NotFound("comment not found");
            }

            return Ok(comment);
        }
    }
}