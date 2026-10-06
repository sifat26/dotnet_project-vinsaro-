using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Vinsaro.Application.Features.Auth.DTOs;
using Vinsaro.Application.Features.Auth.Interfaces;
using Vinsaro.Infrastructure.Identity;
using Vinsaro.Infrastructure.Identity.Options;
using Vinsaro.Infrastructure.Identity.Services;
using Vinsaro.Infrastructure.Persistence;

namespace Vinsaro.UnitTests.Auth;

public class IdentityServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly VinsaroDbContext _dbContext;
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly IdentityService _identityService;

    public IdentityServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(roleStore.Object, null, null, null, null);
        
        _tokenServiceMock = new Mock<ITokenService>();

        var dbOptions = new DbContextOptionsBuilder<VinsaroDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new VinsaroDbContext(dbOptions);

        _jwtOptions = Options.Create(new JwtOptions { ExpiryMinutes = 60, RefreshTokenExpiryDays = 7 });

        _identityService = new IdentityService(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _tokenServiceMock.Object,
            _dbContext,
            _jwtOptions);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = new RegisterRequest("John", "Doe", "test@test.com", "Password123!", "Password123!");
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(new ApplicationUser());

        // Act
        var result = await _identityService.RegisterAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Email already in use.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var request = new LoginRequest("test@test.com", "Password123!");
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _identityService.LoginAsync(request, null, null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid credentials.", result.Error);
    }

    [Fact]
    public async Task ProvisionInitialAdminAsync_ShouldSucceed_WhenNoUsersExist()
    {
        // Arrange
        var request = new RegisterRequest("Admin", "User", "admin@vinsaro.com", "Admin123!", "Admin123!");
        
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
            
        _roleManagerMock.Setup(x => x.RoleExistsAsync("Admin")).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _identityService.ProvisionInitialAdminAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
