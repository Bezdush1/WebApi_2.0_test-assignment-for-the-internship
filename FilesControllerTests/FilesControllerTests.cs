using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApi_2._0.Controllers;
using WebApi_2._0.Data;
using WebApi_2._0.Models;
using Xunit;

namespace WebApi_2._0.Tests.Controllers
{
    public class FilesControllerTests
    {
        private DbContextOptions<ExperimentsAPIDbContext> GetInMemoryDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ExperimentsAPIDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task UploadFile_ValidFile_Success()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var dbContextOptions = GetInMemoryDbContextOptions(dbName);

            using (var dbContext = new ExperimentsAPIDbContext(dbContextOptions))
            {
                var controller = new FilesController(dbContext);
                var fileMock = new Mock<IFormFile>();
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write("2023-07-23_10-30-00;3600;42.0");
                writer.Flush();
                stream.Position = 0;
                fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
                fileMock.Setup(f => f.Length).Returns(stream.Length);
                fileMock.Setup(f => f.ContentType).Returns("text/csv");

                // Act
                var result = await controller.UploadFile(fileMock.Object);

                // Assert
                Assert.IsType<OkResult>(result);

                // Проверка данных, добавлены ли 
                Assert.Single(dbContext.Files);
                Assert.Single(dbContext.Values);
                Assert.Single(dbContext.Results);
            }
        }

        [Fact]
        public async Task UploadFile_InvalidFile_BadRequest()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var dbContextOptions = GetInMemoryDbContextOptions(dbName);

            using (var dbContext = new ExperimentsAPIDbContext(dbContextOptions))
            {
                var controller = new FilesController(dbContext);
                var fileMock = new Mock<IFormFile>();
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write("Invalid file content");
                writer.Flush();
                stream.Position = 0;
                fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
                fileMock.Setup(f => f.Length).Returns(stream.Length);
                fileMock.Setup(f => f.ContentType).Returns("text/csv");

                // Act
                var result = await controller.UploadFile(fileMock.Object);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);

                // Проверка данных, добавлены ли 
                Assert.Empty(dbContext.Files);
                Assert.Empty(dbContext.Values);
                Assert.Empty(dbContext.Results);
            }
        }
    }
}
