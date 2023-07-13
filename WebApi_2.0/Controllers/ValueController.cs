using Microsoft.AspNetCore.Mvc;
using WebApi_2._0.Data;

namespace WebApi_2._0.Controllers
{
    [ApiController]
    [Route("post /science/value/{fileName}")]

    public class ValueController
    {
        private readonly ExperimentsAPIDbContext dbContext;

        public ValueController(ExperimentsAPIDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
