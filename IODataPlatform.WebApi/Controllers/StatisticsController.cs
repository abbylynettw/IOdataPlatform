using IODataPlatform.Models.DBModels;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace IODataPlatform.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly SqlSugarContext _context;

    public StatisticsController(SqlSugarContext context)
    {
        _context = context;
    }

    [HttpGet]
    public ActionResult GetStatistics()
    {
        try
        {
            // 统计项目数量
            int projectCount = _context.Db.Queryable<config_project>().Count();

            // 统计IO配置数量
            int ioCount = _context.Db.Queryable<publish_io>().Count();

            // 统计电缆数量
            int cableCount = _context.Db.Queryable<publish_cable>().Count();

            return Ok(new {
                ProjectCount = projectCount,
                IoCount = ioCount,
                CableCount = cableCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
