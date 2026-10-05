using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using OmniCart.Infrustructure.Services;
using Xunit;

namespace OmniCart.UnitTests.Infrastructure;

public class CacheServiceTests
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly CacheService _sut;

    public CacheServiceTests()
    {
        _mockCache = new Mock<IDistributedCache>();
        _sut = new CacheService(_mockCache.Object);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ShouldReturnDeserializedObject()
    {
        // 1. Arrange
        var cacheKey = "sample_item_key";
        var expectedData = new DummyItem("Laptop", 1200.50m);
        var jsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(expectedData));

        _mockCache
            .Setup(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jsonBytes);

        // 2. Act
        var result = await _sut.GetAsync<DummyItem>(cacheKey);

        // 3. Assert
        Assert.NotNull(result);
        result.Name.Should().Be("Laptop");
        result.Price.Should().Be(1200.50m);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnNull()
    {
        // 1. Arrange
        var cacheKey = "missing_key";
        byte[]? emptyResponse = null;

        _mockCache
            .Setup(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResponse);

        // 2. Act
        var result = await _sut.GetAsync<DummyItem>(cacheKey);

        // 3. Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_WhenCalled_ShouldInvokeUnderlyingCacheRemove()
    {
        // 1. Arrange
        var cacheKey = "item_to_remove";

        // 2. Act
        await _sut.RemoveAsync(cacheKey);

        // 3. Assert
        _mockCache.Verify(
            c => c.RemoveAsync(cacheKey, It.IsAny<CancellationToken>()), 
            Times.Once
        );
    }

    private record DummyItem(string Name, decimal Price);
}