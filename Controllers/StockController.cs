using api.Data;
using api.Dtos.Stock;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers 
{
    [ApiController]
    [Route("api/stock")]
    public class StockController : ControllerBase
    {
        private readonly IStockRepository _stockRepo ;
        public StockController(ApplicationDbContext applicationDbContext, IStockRepository stockRepository)
        {
            _stockRepo = stockRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] QueryObject query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stocks = await _stockRepo.GetAllAsync(query);

            var stockDto = stocks.Select(s => s.ToStockDto());

            return Ok(stocks);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stock = await _stockRepo.GetByIdAsync(id);
            if(stock==null)
            {
                return NotFound();
            }
            return Ok(stock.ToStockDto());
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockRequestDto stcokDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockModel = stcokDto.ToStockFromCreateDTO();

            await _stockRepo.CreateAsync(stockModel);

            return CreatedAtAction(nameof(GetById) , new {id = stockModel.Id} , stockModel.ToStockDto());
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id , [FromBody] UpdateStockRequestDto updateStockRequestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stock = await _stockRepo.UpdateAsync(id,updateStockRequestDto);
            if(stock == null)
            {
                return NotFound();
            } 

            return Ok(stock.ToStockDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id){
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stock = await _stockRepo.DeleteAsync(id);

            if(stock == null)
            {
                return NotFound();
            }
            
            return Ok(stock);
        }

    }
}