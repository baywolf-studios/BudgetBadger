using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.FileSystem.Dropbox;
using BudgetBadger.IntegrationTests;
using BudgetBadger.IntegrationTests.FileSystem.Dropbox;
using NUnit.Framework;

namespace BudgetBadger.IntegrationTests.FileSystem.Dropbox;

[TestFixture]
public class DropboxDirectoryIntegrationTests
{
    [SetUp]
    public async Task Setup()
    {
        _dropboxDirectory = new DropboxDirectory();
        _dropboxDirectory.SetAuthentication(new Dictionary<string, string>
        {
            { DropboxSettings.RefreshToken, IntegrationTestSecrets.DropBoxRefreshToken },
            { DropboxSettings.AppKey, IntegrationTestSecrets.DropBoxAppKey },
            { DropboxSettings.AppSecret, IntegrationTestSecrets.DropBoxAppSecret }
        });
    }

    [TearDown]
    public async Task Teardown()
    {
        await TestDropboxFileBuilder.Cleanup(_rootDirectory);
    }

    private DropboxDirectory _dropboxDirectory;
    private const string _rootDirectory = "/DropboxDirectoryIntegrationTests";

    [Test]
    public async Task ExistsAsync_ExistingDirectory_ReturnsTrue()
    {
        // Arrange
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);

        // Act
        var result = await _dropboxDirectory.ExistsAsync(existingDirectory);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task ExistsAsync_NewDirectory_ReturnsFalse()
    {
        // Arrange
        var missingDirectory = await TestDropboxDirectoryBuilder.GetNewDirectory(_rootDirectory);

        // Act
        var result = await _dropboxDirectory.ExistsAsync(missingDirectory);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task ExistsAsync_DeletedDirectory_ReturnsFalse()
    {
        // Arrange
        var deletedDirectory = await TestDropboxDirectoryBuilder.GetDeletedDirectory(_rootDirectory);

        // Act
        var result = await _dropboxDirectory.ExistsAsync(deletedDirectory);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task ExistsAsync_InvalidDirectory_ReturnsFalse()
    {
        // Arrange
        var invalidDirectory = await TestDropboxDirectoryBuilder.GetInvalidDirectory();

        // Act
        var result = await _dropboxDirectory.ExistsAsync(invalidDirectory);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task DeleteAsync_DeletedDirectory_ThrowsIOException()
    {
        // Assemble
        var deletedDirectory = await TestDropboxDirectoryBuilder.GetDeletedDirectory(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxDirectory.DeleteAsync(deletedDirectory));
    }

    [Test]
    public async Task DeleteAsync_ExistingDirectoryWithFilesRecursiveFalse_IOException()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(existingDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxDirectory.DeleteAsync(existingDirectory));
    }

    [Test]
    public async Task DeleteAsync_ExistingDirectoryWithDirectoriesRecursiveFalse_IOException()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingDirectory2 = await TestDropboxDirectoryBuilder.GetExistingDirectory(existingDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxDirectory.DeleteAsync(existingDirectory));
    }

    [Test]
    public async Task DeleteAsync_ExistingDirectoryWithFilesRecursiveTrue_DeletesDirectory()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(existingDirectory);

        // Act
        await _dropboxDirectory.DeleteAsync(existingDirectory, true);

        // Assert
        var exists = await _dropboxDirectory.ExistsAsync(existingDirectory);
        Assert.IsFalse(exists);
    }

    [Test]
    public async Task DeleteAsync_ExistingDirectoryWithDirectoriesRecursiveTrue_DeletesDirectory()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingDirectory2 = await TestDropboxDirectoryBuilder.GetExistingDirectory(existingDirectory);

        // Act
        await _dropboxDirectory.DeleteAsync(existingDirectory, true);

        // Assert
        var exists = await _dropboxDirectory.ExistsAsync(existingDirectory);
        Assert.IsFalse(exists);
    }

    [Test]
    public async Task DeleteAsync_EmptyExistingDirectory_DeletesDirectories()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);

        // Act
        await _dropboxDirectory.DeleteAsync(existingDirectory);

        // Assert
        var exists = await _dropboxDirectory.ExistsAsync(existingDirectory);
        Assert.IsFalse(exists);
    }

    [Test]
    public async Task DeleteAsync_InvalidDirectory_IOException()
    {
        // Assemble
        var invalidDirectory = await TestDropboxDirectoryBuilder.GetInvalidDirectory();

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxDirectory.DeleteAsync(invalidDirectory));
    }

    [Test]
    public async Task DeleteAsync_NewDirectory_IOException()
    {
        // Assemble
        var newDirectory = await TestDropboxDirectoryBuilder.GetNewDirectory(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxDirectory.DeleteAsync(newDirectory));
    }

    [Test]
    public async Task MoveAsync_SourceDeletedDirectory_IOException()
    {
        // Assemble
        var deletedDirectory = await TestDropboxDirectoryBuilder.GetDeletedDirectory(_rootDirectory);
        var newDirectory = await TestDropboxDirectoryBuilder.GetNewDirectory(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxDirectory.MoveAsync(deletedDirectory, newDirectory));
    }

    [Test]
    public async Task MoveAsync_SourceExistingDirectoryDestExistingDirectory_IOException()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingDirectory2 = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxDirectory.MoveAsync(existingDirectory, existingDirectory2));
    }

    [Test]
    public async Task MoveAsync_SourceExistingDirectoryDestNewDirectory_MovesDirectory()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var newDirectory = await TestDropboxDirectoryBuilder.GetNewDirectory(_rootDirectory);

        // Act
        await _dropboxDirectory.MoveAsync(existingDirectory, newDirectory);

        // Assert
        var movedDirectoryExists = await _dropboxDirectory.ExistsAsync(newDirectory);
        Assert.IsTrue(movedDirectoryExists);

        var existingDirectoryExists = await _dropboxDirectory.ExistsAsync(existingDirectory);
        Assert.IsFalse(existingDirectoryExists);
    }

    [Test]
    public async Task MoveAsync_SourceNewDirectory_IOException()
    {
        // Assemble
        var newDirectory = await TestDropboxDirectoryBuilder.GetNewDirectory(_rootDirectory);
        var newDirectory2 = await TestDropboxDirectoryBuilder.GetNewDirectory(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxDirectory.MoveAsync(newDirectory, newDirectory2));
    }

    [Test]
    public async Task CreateDirectoryAsync_NewDirectory_CreatesDirectory()
    {
        // Assemble
        var newDirectory = await TestDropboxDirectoryBuilder.GetNewDirectory(_rootDirectory);

        // Act
        await _dropboxDirectory.CreateDirectoryAsync(newDirectory);

        // Assert
        var newDirectoryExists = await _dropboxDirectory.ExistsAsync(newDirectory);
        Assert.IsTrue(newDirectoryExists);
    }

    [Test]
    public async Task CreateDirectoryAsync_InvalidDirectory_IOException()
    {
        // Assemble
        var invalidDirectory = await TestDropboxDirectoryBuilder.GetInvalidDirectory();

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxDirectory.CreateDirectoryAsync(invalidDirectory));
    }

    [Test]
    public async Task CreateDirectoryAsync_ExistingDirectory_IOException()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxDirectory.CreateDirectoryAsync(existingDirectory));
    }

    [Test]
    public async Task CreateDirectoryAsync_DeletedDirectory_CreatesDirectory()
    {
        // Assemble
        var deletedDirectory = await TestDropboxDirectoryBuilder.GetDeletedDirectory(_rootDirectory);

        // Act
        await _dropboxDirectory.CreateDirectoryAsync(deletedDirectory);

        // Assert
        var deletedDirectoryExists = await _dropboxDirectory.ExistsAsync(deletedDirectory);
        Assert.IsTrue(deletedDirectoryExists);
    }

    [Test]
    public async Task GetFiles_DirectoryHasExistingFiles_ReturnsAll()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(existingDirectory);
        var existingFile2 = await TestDropboxFileBuilder.GetExistingFile(existingDirectory);

        // Act
        var result = await _dropboxDirectory.GetFilesAsync(existingDirectory);

        // Assert
        Assert.IsTrue(result.Any(r => r == existingFile.Path));
        Assert.IsTrue(result.Any(r => r == existingFile2.Path));
    }

    [Test]
    public async Task GetFiles_DirectoryHasDeletedFiles_ReturnsNone()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var deletedFile = await TestDropboxFileBuilder.GetDeletedFile(existingDirectory);
        var deletedFile2 = await TestDropboxFileBuilder.GetDeletedFile(existingDirectory);

        // Act
        var result = await _dropboxDirectory.GetFilesAsync(existingDirectory);

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task GetFiles_DirectoryHasNewFiles_ReturnsNone()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var newFile = await TestDropboxFileBuilder.GetNewFile(existingDirectory);
        var newFile2 = await TestDropboxFileBuilder.GetNewFile(existingDirectory);

        // Act
        var result = await _dropboxDirectory.GetFilesAsync(existingDirectory);

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task GetFiles_DirectoryHasMixedFiles_ReturnsOnlyExisting()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(existingDirectory);
        var newFile = await TestDropboxFileBuilder.GetNewFile(existingDirectory);
        var deletedFile = await TestDropboxFileBuilder.GetDeletedFile(existingDirectory);

        // Act
        var result = await _dropboxDirectory.GetFilesAsync(existingDirectory);

        // Assert
        Assert.IsTrue(result.Any(r => r == existingFile.Path));
        Assert.IsFalse(result.Any(r => r == newFile.Path));
        Assert.IsFalse(result.Any(r => r == deletedFile.Path));
    }


    [Test]
    public async Task GetDirectories_DirectoryHasExistingDirectories_ReturnsAll()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingDirectory2 = await TestDropboxDirectoryBuilder.GetExistingDirectory(existingDirectory);
        var existingDirectory3 = await TestDropboxDirectoryBuilder.GetExistingDirectory(existingDirectory);

        // Act
        var result = await _dropboxDirectory.GetDirectoriesAsync(existingDirectory);

        // Assert
        Assert.IsTrue(result.Any(r => r == existingDirectory2));
        Assert.IsTrue(result.Any(r => r == existingDirectory3));
    }

    [Test]
    public async Task GetDirectories_DirectoryHasDeletedDirectories_ReturnsNone()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var deletedDirectory = await TestDropboxDirectoryBuilder.GetDeletedDirectory(existingDirectory);
        var deletedDirectory2 = await TestDropboxDirectoryBuilder.GetDeletedDirectory(existingDirectory);

        // Act
        var result = await _dropboxDirectory.GetDirectoriesAsync(existingDirectory);

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task GetDirectories_DirectoryHasNewDirectories_ReturnsNone()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var newDirectory = await TestDropboxDirectoryBuilder.GetNewDirectory(existingDirectory);
        var newDirectory2 = await TestDropboxDirectoryBuilder.GetNewDirectory(existingDirectory);

        // Act
        var result = await _dropboxDirectory.GetDirectoriesAsync(existingDirectory);

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task GetDirectories_DirectoryHasMixedDirectories_ReturnsOnlyExisting()
    {
        // Assemble
        var existingDirectory = await TestDropboxDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingDirectory2 = await TestDropboxDirectoryBuilder.GetExistingDirectory(existingDirectory);
        var newDirectory = await TestDropboxDirectoryBuilder.GetNewDirectory(existingDirectory);
        var deletedDirectory = await TestDropboxDirectoryBuilder.GetDeletedDirectory(existingDirectory);

        // Act
        var result = await _dropboxDirectory.GetDirectoriesAsync(existingDirectory);

        // Assert
        Assert.IsTrue(result.Any(r => r == existingDirectory2));
        Assert.IsFalse(result.Any(r => r == newDirectory));
        Assert.IsFalse(result.Any(r => r == deletedDirectory));
    }
}
