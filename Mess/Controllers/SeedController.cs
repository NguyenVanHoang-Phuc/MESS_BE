using MESS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace MESS.Mess.Controllers;

[Route("api/seed")]
public class SeedController : ApiControllerBase
{
    private readonly DatabaseSeeder _databaseSeeder;

    public SeedController(DatabaseSeeder databaseSeeder)
    {
        _databaseSeeder = databaseSeeder;
    }

    [HttpPost]
    public async Task<IActionResult> SeedDatabase()
    {
        try
        {
            var message = await _databaseSeeder.SeedAsync();
            return Ok(new { Message = message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Có lỗi xảy ra khi Seed data", Error = ex.Message });
        }
    }
}
