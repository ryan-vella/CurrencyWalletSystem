using CurrencyWalletSystem.Infrastructure.Data;
using CurrencyWalletSystem.Infrastructure.Interfaces;
using CurrencyWalletSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;

namespace CurrencyWalletSystem.Tests.Infrastructure.Services
{
    public class SqlExecutorTests
    {
        [Fact]
        public async Task ExecuteAsync_CallsRawSqlExecutor_WithCorrectParameters()
        {
            // Arrange
            var mockRawSqlExecutor = new Mock<IRawSqlExecutor>();
            var sql = "DELETE FROM Wallets WHERE Id = @p0";
            var parameters = new object[] { 1 };

            mockRawSqlExecutor
                .Setup(x => x.ExecuteAsync(sql, parameters))
                .ReturnsAsync(1);

            var executor = new SqlExecutor(mockRawSqlExecutor.Object);

            // Act
            await executor.ExecuteAsync(sql, parameters);

            // Assert
            mockRawSqlExecutor.Verify(x => x.ExecuteAsync(sql, parameters), Times.Once);
        }
    }
}