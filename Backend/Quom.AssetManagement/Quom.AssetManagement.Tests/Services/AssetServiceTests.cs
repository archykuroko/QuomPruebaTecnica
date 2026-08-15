using Moq;
using Quom.AssetManagement.Api.DTOs.Assets;
using Quom.AssetManagement.Api.Models;
using Quom.AssetManagement.Api.Repositories.Interfaces;
using Quom.AssetManagement.Api.Services.Implementations;

namespace Quom.AssetManagement.Tests.Services
{
    public class AssetServiceTests
    {
        private readonly Mock<IAssetRepository> _assetRepositoryMock;
        private readonly AssetService _assetService;

        public AssetServiceTests()
        {
            _assetRepositoryMock = new Mock<IAssetRepository>();

            _assetService = new AssetService(
                _assetRepositoryMock.Object);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenAssetDoesNotExist()
        {
            // Arrange
            var request = CreateValidUpdateRequest();

            _assetRepositoryMock
                .Setup(repository => repository.GetByIdAsync(999))
                .ReturnsAsync((Asset?)null);

            // Act
            var action = async () =>
                await _assetService.UpdateAsync(
                    999,
                    request,
                    1);

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(action);

            _assetRepositoryMock.Verify(
                repository => repository.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<UpdateAssetRequest>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenRetiredAssetIsReactivated()
        {
            // Arrange
            var currentAsset = new Asset
            {
                Id = 1,
                Status = "Retirado"
            };

            var request = CreateValidUpdateRequest();
            request.Status = "Disponible";

            _assetRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(currentAsset);

            // Act
            var action = async () =>
                await _assetService.UpdateAsync(
                    1,
                    request,
                    1);

            // Assert
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(action);

            Assert.Equal(
                "Un activo retirado no puede volver a activarse.",
                exception.Message);

            _assetRepositoryMock.Verify(
                repository => repository.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<UpdateAssetRequest>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenStatusIsAssigned()
        {
            // Arrange
            var currentAsset = new Asset
            {
                Id = 1,
                Status = "Disponible"
            };

            var request = CreateValidUpdateRequest();
            request.Status = "Asignado";

            _assetRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(currentAsset);

            // Act
            var action = async () =>
                await _assetService.UpdateAsync(
                    1,
                    request,
                    1);

            // Assert
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(action);

            Assert.Contains(
                "solo puede establecerse mediante el proceso de asignación",
                exception.Message);

            _assetRepositoryMock.Verify(
                repository => repository.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<UpdateAssetRequest>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenOwnershipTypeIsInvalid()
        {
            // Arrange
            var currentAsset = new Asset
            {
                Id = 1,
                Status = "Disponible"
            };

            var request = CreateValidUpdateRequest();
            request.OwnershipType = "Prestado";

            _assetRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(currentAsset);

            // Act
            var action = async () =>
                await _assetService.UpdateAsync(
                    1,
                    request,
                    1);

            // Assert
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(action);

            Assert.Equal(
                "El tipo de propiedad no es válido.",
                exception.Message);

            _assetRepositoryMock.Verify(
                repository => repository.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<UpdateAssetRequest>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenLeasedAssetHasNoSupplier()
        {
            // Arrange
            var currentAsset = new Asset
            {
                Id = 1,
                Status = "Disponible"
            };

            var request = CreateValidUpdateRequest();
            request.OwnershipType = "Arrendado";
            request.SupplierId = null;
            request.RentalEndDate = DateTime.UtcNow.AddYears(1);

            _assetRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(currentAsset);

            // Act
            var action = async () =>
                await _assetService.UpdateAsync(
                    1,
                    request,
                    1);

            // Assert
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(action);

            Assert.Equal(
                "Un activo arrendado debe tener proveedor.",
                exception.Message);

            _assetRepositoryMock.Verify(
                repository => repository.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<UpdateAssetRequest>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenLeasedAssetHasNoRentalEndDate()
        {
            // Arrange
            var currentAsset = new Asset
            {
                Id = 1,
                Status = "Disponible"
            };

            var request = CreateValidUpdateRequest();
            request.OwnershipType = "Arrendado";
            request.SupplierId = 1;
            request.RentalEndDate = null;

            _assetRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(currentAsset);

            // Act
            var action = async () =>
                await _assetService.UpdateAsync(
                    1,
                    request,
                    1);

            // Assert
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(action);

            Assert.Equal(
                "Un activo arrendado debe indicar la fecha de término.",
                exception.Message);

            _assetRepositoryMock.Verify(
                repository => repository.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<UpdateAssetRequest>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_CallsRepository_WhenRequestIsValid()
        {
            // Arrange
            var currentAsset = new Asset
            {
                Id = 1,
                Status = "Disponible"
            };

            var request = CreateValidUpdateRequest();

            _assetRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(currentAsset);

            _assetRepositoryMock
                .Setup(repository => repository.UpdateAsync(
                    1,
                    request,
                    1))
                .Returns(Task.CompletedTask);

            // Act
            await _assetService.UpdateAsync(
                1,
                request,
                1);

            // Assert
            _assetRepositoryMock.Verify(
                repository => repository.UpdateAsync(
                    1,
                    request,
                    1),
                Times.Once);




        }
        [Fact]
        public async Task AssignAsync_Throws_WhenAssetIdIsInvalid()
        {
            // Arrange
            var request = new AssignAssetRequest
            {
                EmployeeId = 1,
                Notes = "Prueba"
            };

            // Act
            var action = async () =>
                await _assetService.AssignAsync(
                    0,
                    request,
                    1);

            // Assert
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(action);

            Assert.Equal(
                "El identificador del activo no es válido.",
                exception.Message);

            _assetRepositoryMock.Verify(
                repository => repository.AssignAsync(
                    It.IsAny<int>(),
                    It.IsAny<AssignAssetRequest>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task AssignAsync_Throws_WhenEmployeeIdIsInvalid()
        {
            // Arrange
            var request = new AssignAssetRequest
            {
                EmployeeId = 0,
                Notes = "Prueba"
            };

            // Act
            var action = async () =>
                await _assetService.AssignAsync(
                    1,
                    request,
                    1);

            // Assert
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(action);

            Assert.Equal(
                "El identificador del colaborador no es válido.",
                exception.Message);

            _assetRepositoryMock.Verify(
                repository => repository.AssignAsync(
                    It.IsAny<int>(),
                    It.IsAny<AssignAssetRequest>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task AssignAsync_Throws_WhenUserIdIsInvalid()
        {
            // Arrange
            var request = new AssignAssetRequest
            {
                EmployeeId = 1,
                Notes = "Prueba"
            };

            // Act
            var action = async () =>
                await _assetService.AssignAsync(
                    1,
                    request,
                    0);

            // Assert
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(action);

            Assert.Equal(
                "El identificador del usuario no es válido.",
                exception.Message);

            _assetRepositoryMock.Verify(
                repository => repository.AssignAsync(
                    It.IsAny<int>(),
                    It.IsAny<AssignAssetRequest>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task AssignAsync_CallsRepository_WhenRequestIsValid()
        {
            // Arrange
            var request = new AssignAssetRequest
            {
                EmployeeId = 1,
                Notes = "Asignación válida"
            };

            _assetRepositoryMock
                .Setup(repository => repository.AssignAsync(
                    1,
                    request,
                    2))
                .Returns(Task.CompletedTask);

            // Act
            await _assetService.AssignAsync(
                1,
                request,
                2);

            // Assert
            _assetRepositoryMock.Verify(
                repository => repository.AssignAsync(
                    1,
                    request,
                    2),
                Times.Once);
        }

        [Fact]
        public async Task ReturnAsync_Throws_WhenReturnConditionIsEmpty()
        {
            // Arrange
            var request = new ReturnAssetRequest
            {
                ReturnCondition = "",
                Notes = "Prueba"
            };

            // Act
            var action = async () =>
                await _assetService.ReturnAsync(
                    1,
                    request,
                    1);

            // Assert
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(action);

            Assert.Equal(
                "La condición de devolución es obligatoria.",
                exception.Message);

            _assetRepositoryMock.Verify(
                repository => repository.ReturnAsync(
                    It.IsAny<int>(),
                    It.IsAny<ReturnAssetRequest>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task ReturnAsync_CallsRepository_WhenRequestIsValid()
        {
            // Arrange
            var request = new ReturnAssetRequest
            {
                ReturnCondition = "Buen estado",
                Notes = "Sin daños visibles"
            };

            _assetRepositoryMock
                .Setup(repository => repository.ReturnAsync(
                    1,
                    request,
                    2))
                .Returns(Task.CompletedTask);

            // Act
            await _assetService.ReturnAsync(
                1,
                request,
                2);

            // Assert
            _assetRepositoryMock.Verify(
                repository => repository.ReturnAsync(
                    1,
                    request,
                    2),
                Times.Once);
        }

        [Fact]
        public async Task SearchAsync_Throws_WhenStatusIsInvalid()
        {
            // Arrange
            var request = new AssetSearchRequest
            {
                Status = "Volando",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var action = async () =>
                await _assetService.SearchAsync(request);

            // Assert
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(action);

            Assert.Equal(
                "El estado del activo no es válido.",
                exception.Message);

            _assetRepositoryMock.Verify(
                repository => repository.SearchAsync(
                    It.IsAny<AssetSearchRequest>()),
                Times.Never);
        }

        [Fact]
        public async Task SearchAsync_LimitsPageSizeTo100()
        {
            // Arrange
            var request = new AssetSearchRequest
            {
                PageNumber = 1,
                PageSize = 500
            };

            _assetRepositoryMock
                .Setup(repository => repository.SearchAsync(
                    It.IsAny<AssetSearchRequest>()))
                .ReturnsAsync(
                    new Quom.AssetManagement.Api.DTOs.PagedResult<Asset>());

            // Act
            await _assetService.SearchAsync(request);

            // Assert
            Assert.Equal(100, request.PageSize);

            _assetRepositoryMock.Verify(
                repository => repository.SearchAsync(
                    It.Is<AssetSearchRequest>(
                        r => r.PageSize == 100)),
                Times.Once);
        }
        private static UpdateAssetRequest CreateValidUpdateRequest()
        {
            return new UpdateAssetRequest
            {
                AssetCode = "TI-TEST-001",
                SerialNumber = "SN-TEST-001",
                Category = "Laptop",
                Brand = "Dell",
                Model = "Latitude 5440",
                OwnershipType = "Propio",
                SupplierId = null,
                Status = "Disponible",
                CurrentLocation = "Almacén TI",
                PurchaseDate = new DateTime(2026, 8, 1),
                RentalEndDate = null
            };
        }
    }
}