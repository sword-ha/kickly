using FluentAssertions;
using Moq;
using SportsBooking.Application.Interfaces;
using SportsBooking.Application.Services;
using SportsBooking.Domain.Entities;
using SportsBooking.Domain.Exceptions;
using Xunit;

namespace SportsBooking.Tests;

public sealed class FavoriteServiceTests
{
    private readonly Mock<IFavoriteRepository> _favoriteRepo;
    private readonly Mock<IFieldRepository> _fieldRepo;
    private readonly FavoriteService _service;

    public FavoriteServiceTests()
    {
        _favoriteRepo = new Mock<IFavoriteRepository>();
        _fieldRepo = new Mock<IFieldRepository>();
        _service = new FavoriteService(_favoriteRepo.Object, _fieldRepo.Object);
    }

    [Fact]
    public async Task Add_NewFavorite_AddsSuccessfully()
    {
        _fieldRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Field { Id = 1 });
        _favoriteRepo.Setup(r => r.ExistsAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _favoriteRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _service.AddAsync(1, 1, CancellationToken.None);

        _favoriteRepo.Verify(r => r.AddAsync(It.IsAny<Favorite>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Add_DuplicateFavorite_ThrowsConflict()
    {
        _fieldRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Field { Id = 1 });
        _favoriteRepo.Setup(r => r.ExistsAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = async () => await _service.AddAsync(1, 1, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Add_FieldNotFound_ThrowsNotFound()
    {
        _fieldRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Field?)null);

        var act = async () => await _service.AddAsync(1, 99, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Remove_ExistingFavorite_RemovesSuccessfully()
    {
        _favoriteRepo.Setup(r => r.GetAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync(new Favorite { Id = 1, UserId = 1, FieldId = 1 });
        _favoriteRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _service.RemoveAsync(1, 1, CancellationToken.None);

        _favoriteRepo.Verify(r => r.Remove(It.IsAny<Favorite>()), Times.Once);
    }

    [Fact]
    public async Task Remove_NonExistentFavorite_ThrowsNotFound()
    {
        _favoriteRepo.Setup(r => r.GetAsync(1, 1, It.IsAny<CancellationToken>())).ReturnsAsync((Favorite?)null);

        var act = async () => await _service.RemoveAsync(1, 1, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}