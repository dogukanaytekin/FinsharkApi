using System.IdentityModel.Tokens.Jwt;
using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/portfolio")]
    public class PortfolioController: ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IStockRepository _stockRepo;
        
        public PortfolioController(ApplicationDbContext context, UserManager<AppUser> userManager, IStockRepository stockRepo)
        {
            _context = context;
            _userManager = userManager;
            _stockRepo = stockRepo;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult > GetPortfolios()
        {
            var userName = User.Claims.SingleOrDefault(c=> c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"))?.Value;
            var UserObject = await _userManager.Users.FirstOrDefaultAsync(u=> u.UserName.Equals(userName));
            var userId = UserObject.Id;
            var UserPortfolios = await _context.Portfolios.Where(p=> p.AppUserId == userId).ToListAsync();
            var StockId = UserPortfolios.Select(p=> p.StockId);
            
            var stockList = new List<Stock>();

            foreach (var stockId in StockId){
                var stock = await _context.Stocks.FirstOrDefaultAsync(s=> s.Id == stockId);
                if (stock != null)
                {
                    stockList.Add(stock);
                }
            }


            return Ok(stockList);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePortfolio(string symbol)
        {
            var userName = User.Claims.SingleOrDefault(c=> c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"))?.Value;
            var userId = _userManager.Users.FirstOrDefault(u => u.UserName.Equals(userName))?.Id;
            var stock = await _stockRepo.GetBySymbol(symbol);
            if (stock == null) return BadRequest("Stock not found");

            var portfolio = await _context.Portfolios.FirstOrDefaultAsync(p=> p.AppUserId == userId);
            if(portfolio != null)
            {
                var stockExists = await _context.Stocks.AnyAsync(s => s.Id == stock.Id);
                if(stockExists)
                {
                    return BadRequest("Stock already exist in Portfolio");
                }
            }

            var portfolioToAdd = new Portfolio 
            {
                StockId = stock.Id,
                AppUserId = userId
            };

            await _context.Portfolios.AddAsync(portfolioToAdd);
            await _context.SaveChangesAsync();
            return Ok(portfolioToAdd);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeletePortfolio(string symbol)
        {
            var userName = User.Claims.SingleOrDefault(c=> c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"))?.Value;
            var userId = _userManager.Users.FirstOrDefault(u => u.UserName.Equals(userName))?.Id;
            var stock = await _stockRepo.GetBySymbol(symbol);
            if (stock == null) return BadRequest("Stock not found");

            var portfolio = await _context.Portfolios.FirstOrDefaultAsync(p=> p.AppUserId == userId && p.StockId == stock.Id);
            if (portfolio == null) return BadRequest("This stock is not in your portfolio");

            _context.Portfolios.Remove(portfolio);
            await _context.SaveChangesAsync();
            return Ok(portfolio);
        }
    }
}