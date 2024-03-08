using api.Interfaces;
using api.Models;
using api.Data;
using Microsoft.EntityFrameworkCore;
using api.Dtos.Stock;
using Microsoft.AspNetCore.Http.HttpResults;
using api.Helpers;

namespace api.Repository
{
    public class StockRepository : IStockRepository

    {
        private readonly ApplicationDbContext _context;
        
        public StockRepository(ApplicationDbContext dbContext)
        {
            _context = dbContext;    
        }

        public async Task<List<Stock>> GetAllAsync(QueryObject query)
        {
            var stocks = _context.Stocks.Include(c=> c.Comments).AsQueryable();

            if(!string.IsNullOrWhiteSpace(query.Symbol))
            {
                stocks = stocks.Where(c=> c.Symbol.Contains(query.Symbol));
            }
            if(!string.IsNullOrWhiteSpace(query.CompanyName))
            {
                stocks = stocks.Where(c=> c.Symbol.Contains(query.CompanyName));
            }

            if(!string.IsNullOrWhiteSpace(query.Sortby))
            {
                if(query.Sortby.Equals("Symbol",StringComparison.OrdinalIgnoreCase))
                {
                    stocks = query.IsDescending ? stocks.OrderByDescending(s=> s.Symbol) : stocks.OrderBy(s=> s.Symbol);
                }
            }

            var skipNumber = (query.PageNumber-1) * query.PageSize;

            return await stocks.Skip(skipNumber).Take(query.PageSize).ToListAsync();

        }

        public async Task<Stock> CreateAsync(Stock stockModel)
        {
            await _context.Stocks.AddAsync(stockModel);
            await _context.SaveChangesAsync();

            return stockModel;
        }

        public async Task<Stock?> DeleteAsync(int id)
        {
           var stock = _context.Stocks.FirstOrDefault(s=> s.Id ==id);
           
           if (stock == null)
           return null;

           _context.Stocks.Remove(stock);
           await _context.SaveChangesAsync();
           return stock;
        }
        
        public async Task<Stock?> GetByIdAsync(int id)
        {
            var stock = await _context.Stocks.Include(c=> c.Comments).FirstOrDefaultAsync(s => s.Id==id);
           
            if (stock == null)
           return null;

            return stock;
        }

        public async Task<Stock?> UpdateAsync(int id, UpdateStockRequestDto stockDto)
        {
            var stock = await _context.Stocks.FirstOrDefaultAsync(s=> s.Id == id);
            
            if (stock == null)
            return null;

            stock.Industry = stockDto.Industry;
            stock.Purchase = stockDto.Purchase;
            stock.MarketCap = stockDto.MarketCap;
            stock.LastDiv = stockDto.LastDiv;
            stock.Symbol = stockDto.Symbol;
            stock.CompanyName = stockDto.CompanyName;

            await _context.SaveChangesAsync();
            
            return stock ;

        }

        public async Task<bool> StockExist(int id)
        {
            return await _context.Stocks.AnyAsync(s=> s.Id==id);
        }

        public async Task<Stock?> GetBySymbol(string symbol)
        {
            return await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol.Equals(symbol));
        }
    }
}