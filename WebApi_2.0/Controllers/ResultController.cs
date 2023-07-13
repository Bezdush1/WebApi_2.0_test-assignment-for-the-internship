using Microsoft.AspNetCore.Mvc;
using WebApi_2._0.Data;

namespace WebApi_2._0.Controllers
{
    [ApiController]
    [Route("post /science/results/")]

    public class ResultController
    {
        private readonly ExperimentsAPIDbContext dbContext;

        public ResultController(ExperimentsAPIDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
