using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.FileSystem.WebDav;
using BudgetBadger.IntegrationTests.FileSystem.Dropbox;
using NUnit.Framework;

namespace BudgetBadger.IntegrationTests.FileSystem.WebDav;

[TestFixture]
public class WebDavDirectoryIntegrationTests
{
    [SetUp]
    public async Task Setup()
    {
        await TestWebDavDirectoryBuilder.Setup(_rootDirectory);
        await TestWebDavFileBuilder.Setup(_rootDirectory);
        _webDavDirectory = new WebDavDirectory();
        _webDavDirectory.SetAuthentication(new Dictionary<string, string>
        {
            { WebDavSettings.Server, IntegrationTestSecrets.WebDavServer },
            { WebDavSettings.Directory, IntegrationTestSecrets.WebDavDirectory },
            { WebDavSettings.Username, IntegrationTestSecrets.WebDavUsername },
            { WebDavSettings.Password, IntegrationTestSecrets.WebDavPassword }
        });
    }

    [TearDown]
    public async Task Cleanup()
    {
        await TestWebDavDirectoryBuilder.Cleanup(_rootDirectory);
    }

    private const string _rootDirectory = "/WebDavDirectoryIntegrationTests/";
    private WebDavDirectory _webDavDirectory;

    [Test]
    public async Task ExistsAsync_ExistingDirectory_ReturnsTrue()
    {
        // Arrange
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);

        // Act
        var result = await _webDavDirectory.ExistsAsync(existingDirectory);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task ExistsAsync_NewDirectory_ReturnsFalse()
    {
        // Arrange
        var missingDirectory = await TestWebDavDirectoryBuilder.GetNewDirectory(_rootDirectory);

        // Act
        var result = await _webDavDirectory.ExistsAsync(missingDirectory);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task ExistsAsync_DeletedDirectory_ReturnsFalse()
    {
        // Arrange
        var deletedDirectory = await TestWebDavDirectoryBuilder.GetDeletedDirectory(_rootDirectory);

        // Act
        var result = await _webDavDirectory.ExistsAsync(deletedDirectory);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task ExistsAsync_InvalidDirectory_ReturnsFalse()
    {
        // Arrange
        var invalidDirectory = await TestWebDavDirectoryBuilder.GetInvalidDirectory();

        // Act
        var result = await _webDavDirectory.ExistsAsync(invalidDirectory);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task DeleteAsync_DeletedDirectory_ThrowsIOException()
    {
        // Assemble
        var deletedDirectory = await TestWebDavDirectoryBuilder.GetDeletedDirectory(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavDirectory.DeleteAsync(deletedDirectory));
    }

    [Test]
    public async Task DeleteAsync_ExistingDirectoryWithFilesRecursiveFalse_IOException()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(existingDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavDirectory.DeleteAsync(existingDirectory));
    }

    [Test]
    public async Task DeleteAsync_ExistingDirectoryWithDirectoriesRecursiveFalse_IOException()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingDirectory2 = await TestWebDavDirectoryBuilder.GetExistingDirectory(existingDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavDirectory.DeleteAsync(existingDirectory));
    }

    [Test]
    public async Task DeleteAsync_ExistingDirectoryWithFilesRecursiveTrue_DeletesDirectory()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(existingDirectory);

        // Act
        await _webDavDirectory.DeleteAsync(existingDirectory, true);

        // Assert
        var exists = await _webDavDirectory.ExistsAsync(existingDirectory);
        Assert.IsFalse(exists);
    }

    [Test]
    public async Task DeleteAsync_ExistingDirectoryWithDirectoriesRecursiveTrue_DeletesDirectory()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingDirectory2 = await TestWebDavDirectoryBuilder.GetExistingDirectory(existingDirectory);

        // Act
        await _webDavDirectory.DeleteAsync(existingDirectory, true);

        // Assert
        var exists = await _webDavDirectory.ExistsAsync(existingDirectory);
        Assert.IsFalse(exists);
    }

    [Test]
    public async Task DeleteAsync_EmptyExistingDirectory_DeletesDirectories()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);

        // Act
        await _webDavDirectory.DeleteAsync(existingDirectory);

        // Assert
        var exists = await _webDavDirectory.ExistsAsync(existingDirectory);
        Assert.IsFalse(exists);
    }

    [Test]
    public async Task DeleteAsync_InvalidDirectory_IOException()
    {
        // Assemble
        var invalidDirectory = await TestWebDavDirectoryBuilder.GetInvalidDirectory();

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavDirectory.DeleteAsync(invalidDirectory));
    }

    [Test]
    public async Task DeleteAsync_NewDirectory_IOException()
    {
        // Assemble
        var newDirectory = await TestWebDavDirectoryBuilder.GetNewDirectory(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavDirectory.DeleteAsync(newDirectory));
    }

    [Test]
    public async Task MoveAsync_SourceDeletedDirectory_IOException()
    {
        // Assemble
        var deletedDirectory = await TestWebDavDirectoryBuilder.GetDeletedDirectory(_rootDirectory);
        var newDirectory = await TestWebDavDirectoryBuilder.GetNewDirectory(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavDirectory.MoveAsync(deletedDirectory, newDirectory));
    }

    [Test]
    public async Task MoveAsync_SourceExistingDirectoryDestExistingDirectory_IOException()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingDirectory2 = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavDirectory.MoveAsync(existingDirectory, existingDirectory2));
    }

    [Test]
    public async Task MoveAsync_SourceExistingDirectoryDestNewDirectory_MovesDirectory()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var newDirectory = await TestWebDavDirectoryBuilder.GetNewDirectory(_rootDirectory);

        // Act
        await _webDavDirectory.MoveAsync(existingDirectory, newDirectory);

        // Assert
        var movedDirectoryExists = await _webDavDirectory.ExistsAsync(newDirectory);
        Assert.IsTrue(movedDirectoryExists);

        var existingDirectoryExists = await _webDavDirectory.ExistsAsync(existingDirectory);
        Assert.IsFalse(existingDirectoryExists);
    }

    [Test]
    public async Task MoveAsync_SourceNewDirectory_IOException()
    {
        // Assemble
        var newDirectory = await TestWebDavDirectoryBuilder.GetNewDirectory(_rootDirectory);
        var newDirectory2 = await TestWebDavDirectoryBuilder.GetNewDirectory(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavDirectory.MoveAsync(newDirectory, newDirectory2));
    }

    [Test]
    public async Task CreateDirectoryAsync_NewDirectory_CreatesDirectory()
    {
        // Assemble
        var newDirectory = await TestWebDavDirectoryBuilder.GetNewDirectory(_rootDirectory);

        // Act
        await _webDavDirectory.CreateDirectoryAsync(newDirectory);

        // Assert
        var newDirectoryExists = await _webDavDirectory.ExistsAsync(newDirectory);
        Assert.IsTrue(newDirectoryExists);
    }

    [Test]
    public async Task CreateDirectoryAsync_InvalidDirectory_IOException()
    {
        // Assemble
        var invalidDirectory = await TestWebDavDirectoryBuilder.GetInvalidDirectory();

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavDirectory.CreateDirectoryAsync(invalidDirectory));
    }

    [Test]
    public async Task CreateDirectoryAsync_ExistingDirectory_IOException()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavDirectory.CreateDirectoryAsync(existingDirectory));
    }

    [Test]
    public async Task CreateDirectoryAsync_DeletedDirectory_CreatesDirectory()
    {
        // Assemble
        var deletedDirectory = await TestWebDavDirectoryBuilder.GetDeletedDirectory(_rootDirectory);

        // Act
        await _webDavDirectory.CreateDirectoryAsync(deletedDirectory);

        // Assert
        var deletedDirectoryExists = await _webDavDirectory.ExistsAsync(deletedDirectory);
        Assert.IsTrue(deletedDirectoryExists);
    }

    [Test]
    public async Task GetFiles_DirectoryHasExistingFiles_ReturnsAll()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(existingDirectory);
        var existingFile2 = await TestWebDavFileBuilder.GetExistingFile(existingDirectory);

        // Act
        var result = await _webDavDirectory.GetFilesAsync(existingDirectory);

        // Assert
        Assert.IsTrue(result.Any(r => r == existingFile.Path));
        Assert.IsTrue(result.Any(r => r == existingFile2.Path));
    }

    [Test]
    public async Task GetFiles_DirectoryHasDeletedFiles_ReturnsNone()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var deletedFile = await TestWebDavFileBuilder.GetDeletedFile(existingDirectory);
        var deletedFile2 = await TestWebDavFileBuilder.GetDeletedFile(existingDirectory);

        // Act
        var result = await _webDavDirectory.GetFilesAsync(existingDirectory);

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task GetFiles_DirectoryHasNewFiles_ReturnsNone()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var newFile = await TestWebDavFileBuilder.GetNewFile(existingDirectory);
        var newFile2 = await TestWebDavFileBuilder.GetNewFile(existingDirectory);

        // Act
        var result = await _webDavDirectory.GetFilesAsync(existingDirectory);

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task GetFiles_DirectoryHasMixedFiles_ReturnsOnlyExisting()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(existingDirectory);
        var newFile = await TestWebDavFileBuilder.GetNewFile(existingDirectory);
        var deletedFile = await TestWebDavFileBuilder.GetDeletedFile(existingDirectory);

        // Act
        var result = await _webDavDirectory.GetFilesAsync(existingDirectory);

        // Assert
        Assert.IsTrue(result.Any(r => r == existingFile.Path));
        Assert.IsFalse(result.Any(r => r == newFile.Path));
        Assert.IsFalse(result.Any(r => r == deletedFile.Path));
    }


    [Test]
    public async Task GetDirectories_DirectoryHasExistingDirectories_ReturnsAll()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingDirectory2 = await TestWebDavDirectoryBuilder.GetExistingDirectory(existingDirectory);
        var existingDirectory3 = await TestWebDavDirectoryBuilder.GetExistingDirectory(existingDirectory);

        // Act
        var result = await _webDavDirectory.GetDirectoriesAsync(existingDirectory);

        // Assert
        Assert.IsTrue(result.Any(r => r.TrimEnd('/') == existingDirectory2.TrimEnd('/')));
        Assert.IsTrue(result.Any(r => r.TrimEnd('/') == existingDirectory3.TrimEnd('/')));
    }

    [Test]
    public async Task GetDirectories_DirectoryHasDeletedDirectories_ReturnsNone()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var deletedDirectory = await TestWebDavDirectoryBuilder.GetDeletedDirectory(existingDirectory);
        var deletedDirectory2 = await TestWebDavDirectoryBuilder.GetDeletedDirectory(existingDirectory);

        // Act
        var result = await _webDavDirectory.GetDirectoriesAsync(existingDirectory);

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task GetDirectories_DirectoryHasNewDirectories_ReturnsNone()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var newDirectory = await TestWebDavDirectoryBuilder.GetNewDirectory(existingDirectory);
        var newDirectory2 = await TestWebDavDirectoryBuilder.GetNewDirectory(existingDirectory);

        // Act
        var result = await _webDavDirectory.GetDirectoriesAsync(existingDirectory);

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task GetDirectories_DirectoryHasMixedDirectories_ReturnsOnlyExisting()
    {
        // Assemble
        var existingDirectory = await TestWebDavDirectoryBuilder.GetExistingDirectory(_rootDirectory);
        var existingDirectory2 = await TestWebDavDirectoryBuilder.GetExistingDirectory(existingDirectory);
        var newDirectory = await TestWebDavDirectoryBuilder.GetNewDirectory(existingDirectory);
        var deletedDirectory = await TestWebDavDirectoryBuilder.GetDeletedDirectory(existingDirectory);

        // Act
        var result = await _webDavDirectory.GetDirectoriesAsync(existingDirectory);

        // Assert
        Assert.IsTrue(result.Any(r => r.TrimEnd('/') == existingDirectory2.TrimEnd('/')));
        Assert.IsFalse(result.Any(r => r.TrimEnd('/') == newDirectory.TrimEnd('/')));
        Assert.IsFalse(result.Any(r => r.TrimEnd('/') == deletedDirectory.TrimEnd('/')));
    }
}
