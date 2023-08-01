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
                fileMock.Setup(f => f.FileName).Returns("example.csv");

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

        [Fact]
        public void TryParseLine_ValidLine_ReturnsTrue()
        {
            // Arrange
            var line = "2022-03-18_09-18-17;1744;1632,472";

            // Act
            var result = FilesController.TryParseLine(line, out var dateTime, out var duration, out var indicator);

            // Assert
            Assert.True(result);
            Assert.Equal(new DateTime(2022, 3, 18, 9, 18, 17), dateTime);
            Assert.Equal(1744, duration);
            Assert.Equal(1632.472, indicator, 3); // Ожидаемая дробная часть до 3 знака после запятой
        }

        [Fact]
        public void TryParseLine_InvalidLine_ReturnsFalse()
        {
            // Arrange
            var line = "Invalid line";

            // Act
            var result = FilesController.TryParseLine(line, out var dateTime, out var duration, out var indicator);

            // Assert
            Assert.False(result);
            Assert.Equal(DateTime.MinValue, dateTime);
            Assert.Equal(0, duration);
            Assert.Equal(0.0, indicator);
        }
    }
}
