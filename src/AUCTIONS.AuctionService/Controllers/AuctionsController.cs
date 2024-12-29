using AuctionService.Data;
using AuctionService.Entities;
using AuctionService.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
namespace AUCTIONS.AuctionService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _dbContext;
    private readonly IMapper _mapper;
    public AuctionsController(AuctionDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuctions()
    {
        var auctions = await _dbContext.Auctions.Include(x => x.Item).OrderBy(x => x.Item.Make).ToListAsync();
        return Ok(_mapper.Map<List<AuctionDto>>(auctions));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _dbContext.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<AuctionDto>(auction));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        //TODO: Add current user as seller
        auction.Seller = "user";
        await _dbContext.Auctions.AddAsync(auction);
        var result = await _dbContext.SaveChangesAsync() > 0;
        if (!result)
        {
            return BadRequest("Unable to create Auction");
        }
        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuction(Guid id, [FromBody] UpdateAuctionDto updateDto)
    {
        var auction = await _dbContext.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null)
        {
            return NotFound();
        }
        //TODO: Check if seller is equal to user
        //        var updatedAuction = _mapper.Map(updateDto, auction);
        auction.Item.Make = updateDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateDto.Model ?? auction.Item.Model;
        auction.Item.Year = updateDto.Year ?? auction.Item.Year;
        auction.Item.Color = updateDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateDto.Mileage ?? auction.Item.Mileage;

        var result = await _dbContext.SaveChangesAsync() > 0;
        if (!result)
        {
            return BadRequest("Unable to update auctioon");
        }
        return Ok();

    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuction(Guid id)
    {
        var auction = _dbContext.Auctions.FirstOrDefault(x => x.Id == id);
        if (auction == null)
        {
            return NotFound();
        }

        //TODO: Check if seller is equal to user
        _dbContext.Auctions.Remove(auction);
        var result = await _dbContext.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Unable to delete auction");
        return Ok();
    }
}
