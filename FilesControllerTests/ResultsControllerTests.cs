using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi_2._0.Controllers;
using WebApi_2._0.Data;
using WebApi_2._0.Models;
using Xunit;

namespace WebApi_2._0.Tests.Controllers
{
    public class ResultsControllerTests
    {
        private DbContextOptions<ExperimentsAPIDbContext> GetInMemoryDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ExperimentsAPIDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task GetResults_ReturnsOkResult()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var dbContextOptions = GetInMemoryDbContextOptions(dbName);

            using (var dbContext = new ExperimentsAPIDbContext(dbContextOptions))
            {
                // Добавление тестовых данных в базу данных в памяти
                dbContext.Results.Add(new ResultModel { FileName = "test_file",
                    AverageIndicator = 42, AverageTimeExperiment = 3600 });
                await dbContext.SaveChangesAsync();

                var controller = new ResultsController(dbContext);

                // Act
                var result = await controller.GetResults("test_file",
                    minAverageIndicator: null, maxAverageIndicator: null,
                    minAverageTime: null, maxAverageTime: null);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var results = Assert.IsType<List<ResultModel>>(okResult.Value);
                Assert.Single(results);
            }
        }

        [Fact]
        public async Task GetResults_WithInvalidFileName_ReturnsNotFound()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var dbContextOptions = GetInMemoryDbContextOptions(dbName);

            using (var dbContext = new ExperimentsAPIDbContext(dbContextOptions))
            {
                // Добавление тестовых данных в базу данных в памяти
                dbContext.Results.Add(new ResultModel { FileName = "test_file",
                    AverageIndicator = 42, AverageTimeExperiment = 3600 });
                await dbContext.SaveChangesAsync();

                var controller = new ResultsController(dbContext);

                // Act
                var result = await controller.GetResults("invalid_file",
                    minAverageIndicator: null, maxAverageIndicator: null,
                    minAverageTime: null, maxAverageTime: null);

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal("No results found matching the criteria.", notFoundResult.Value);
            }
        }
    }
}
