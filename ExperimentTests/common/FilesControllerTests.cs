using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using WebApi_2._0.Controllers;
using WebApi_2._0.Data;
using Microsoft.EntityFrameworkCore;
using WebApi_2._0.Models;
using Xunit;

namespace WebApi_2._0.Tests.Controllers
{
    public class FilesControllerTests
    {
        // Фейковый контекст базы данных
        public interface IExperimentsAPIDbContext
        {
            Task<int> SaveChangesAsync();
            DbSet<FileModel> Files { get; set; }
            DbSet<ValueModel> Values { get; set; }
            DbSet<ResultModel> Results { get; set; }
        }

        public class FakeExperimentsAPIDbContext : IExperimentsAPIDbContext
        {
            public List<ValueModel> Values { get; set; }
            public List<ResultModel> Results { get; set; }

            public FakeExperimentsAPIDbContext()
            {
                Values = new List<ValueModel>();
                Results = new List<ResultModel>();
                Files = new Mock<DbSet<FileModel>>().Object;
            }

            public Task<int> SaveChangesAsync()
            {
                return Task.FromResult(0);
            }

            public DbSet<FileModel> Files { get; set; }
            public DbSet<ValueModel> Values { get; set; }
            public DbSet<ResultModel> Results { get; set; }
        }

        [Fact]
        public async Task UploadFile_ValidFile_Success()
        {
            // Arrange
            var dbContext = new FakeExperimentsAPIDbContext();
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

            // Check if data is added to the context
            Assert.Single(dbContext.Files);
            Assert.Single(dbContext.Values);
            Assert.Single(dbContext.Results);
        }

        [Fact]
        public async Task UploadFile_InvalidFile_BadRequest()
        {
            // Arrange
            var dbContext = new FakeExperimentsAPIDbContext();
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

            // Check if data is not added to the context
            Assert.Empty(dbContext.Files);
            Assert.Empty(dbContext.Values);
            Assert.Empty(dbContext.Results);
        }

        // Add more test cases to cover different scenarios (e.g., empty file, incorrect content type, etc.)
    }
}
