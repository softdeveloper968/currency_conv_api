using Backend.Application.Dtos;
using Backend.Application.Interfaces;
using Backend.Application.Services;
using Backend.Domain.Entities;
using Moq;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class ConvertCurrencyServiceTests
{
    [Fact]
    public async Task ConvertAsync_ReturnsExpectedToAmount()
    {
        var rateProvider = new Mock<IRateProvider>();
        rateProvider.Setup(r => r.GetRatesAsync("USD", default)).ReturnsAsync(new Dictionary<string, decimal> { { "INR", 82m } });


        var repo = new Mock<IConversionRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<ConversionHistory>(), default)).Returns(Task.CompletedTask);


        var svc = new ConvertCurrencyService(rateProvider.Object, repo.Object);
        var dto = new ConversionRequestDto("USD", "INR", 10, "user-1");


        var result = await svc.ConvertAsync(dto);


        result.ToAmount.Should().Be(820m);
        repo.Verify(r => r.AddAsync(It.IsAny<ConversionHistory>(), default), Times.Once);
    }
}