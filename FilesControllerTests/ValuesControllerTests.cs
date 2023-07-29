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
    public class ValuesControllerTests
    {
        private DbContextOptions<ExperimentsAPIDbContext> GetInMemoryDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ExperimentsAPIDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task GetValues_WithValidFileName_ReturnsOkResult()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var dbContextOptions = GetInMemoryDbContextOptions(dbName);

            using (var dbContext = new ExperimentsAPIDbContext(dbContextOptions))
            {
                // Добавление тестовых данных в базу данных в памяти
                dbContext.Values.Add(new ValueModel { FileName = "test_file",
                    Date = DateTime.Now, ExperimentTime = 3600, Indicators = 42.0 });
                await dbContext.SaveChangesAsync();

                var controller = new ValuesController(dbContext);

                // Act
                var result = await controller.GetValues("test_file");

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result);
                var values = Assert.IsType<List<ValueModel>>(okResult.Value);
                Assert.Single(values);
            }
        }

        [Fact]
        public async Task GetValues_WithInvalidFileName_ReturnsNotFound()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var dbContextOptions = GetInMemoryDbContextOptions(dbName);

            using (var dbContext = new ExperimentsAPIDbContext(dbContextOptions))
            {
                // Добавление тестовых данных в базу данных в памяти
                dbContext.Values.Add(new ValueModel { FileName = "test_file",
                    Date = DateTime.Now, ExperimentTime = 3600, Indicators = 42.0 });
                await dbContext.SaveChangesAsync();

                var controller = new ValuesController(dbContext);

                // Act
                var result = await controller.GetValues("invalid_file");

                // Assert
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal("No values found for the specified file name", notFoundResult.Value);
            }
        }
    }
}
