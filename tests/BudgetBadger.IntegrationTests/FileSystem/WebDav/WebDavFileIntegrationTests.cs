using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BudgetBadger.FileSystem.WebDav;
using NUnit.Framework;

namespace BudgetBadger.IntegrationTests.FileSystem.WebDav;

[Ignore("No credentials in GitHub")]
[TestFixture]
public class WebDavFileIntegrationTests
{
    [SetUp]
    public async Task Setup()
    {
        await TestWebDavFileBuilder.Setup(_rootDirectory);
        _webDavFile = new WebDavFile();
        _webDavFile.SetAuthentication(new Dictionary<string, string>
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
        await TestWebDavFileBuilder.Cleanup(_rootDirectory);
    }

    private const string _rootDirectory = "/WebDavFileIntegrationTests/";
    private WebDavFile _webDavFile;

    [Test]
    public async Task ExistsAsync_ExistingFile_ReturnsTrue()
    {
        // Arrange
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);

        // Act
        var result = await _webDavFile.ExistsAsync(existingFile.Path);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task ExistsAsync_NewFile_ReturnsFalse()
    {
        // Arrange
        var missingFile = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);

        // Act
        var result = await _webDavFile.ExistsAsync(missingFile.Path);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task ExistsAsync_DeletedFile_ReturnsFalse()
    {
        // Arrange
        var deletedFile = await TestWebDavFileBuilder.GetDeletedFile(_rootDirectory);

        // Act
        var result = await _webDavFile.ExistsAsync(deletedFile.Path);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task ExistsAsync_InvalidFile_ReturnsFalse()
    {
        // Arrange
        var invalidFile = await TestWebDavFileBuilder.GetInvalidFile();

        // Act
        var result = await _webDavFile.ExistsAsync(invalidFile.Path);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task ReadAllBytesAsync_NewFile_ThrowsIOException()
    {
        // Arrange
        var missingFile = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);

        // Act and Assert
        Assert.ThrowsAsync<IOException>(() => _webDavFile.ReadAllBytesAsync(missingFile.Path));
    }

    [Test]
    public async Task ReadAllBytesAsync_DeletedFile_ThrowsIOException()
    {
        // Arrange
        var deletedFile = await TestWebDavFileBuilder.GetDeletedFile(_rootDirectory);

        // Act and Assert
        Assert.ThrowsAsync<IOException>(() => _webDavFile.ReadAllBytesAsync(deletedFile.Path));
    }

    [Test]
    public async Task ReadAllBytesAsync_ExistingFile_ReturnsCorrectBytes()
    {
        // Arrange
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);

        // Act
        var result = await _webDavFile.ReadAllBytesAsync(existingFile.Path);

        // Assert
        Assert.AreEqual(existingFile.Data, result);
    }

    [Test]
    public async Task ReadAllBytesAsync_InvalidFile_ThrowsIOException()
    {
        // Arrange
        var invalidFile = await TestWebDavFileBuilder.GetInvalidFile();

        // Act and Assert
        Assert.ThrowsAsync<IOException>(() => _webDavFile.ReadAllBytesAsync(invalidFile.Path));
    }

    [Test]
    public async Task WriteAllBytesAsync_NewFile_WritesFile()
    {
        // Arrange
        var newFile = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);

        // Act
        await _webDavFile.WriteAllBytesAsync(newFile.Path, newFile.Data);

        // Assert
        Assert.Pass();
    }

    [Test]
    public async Task WriteAllBytesAsync_InvalidFile_IOException()
    {
        // Arrange
        var invalidFile = await TestWebDavFileBuilder.GetInvalidFile();

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavFile.WriteAllBytesAsync(invalidFile.Path, invalidFile.Data));
    }

    [Test]
    public async Task WriteAllBytesAsync_ExistingFile_OverwritesFile()
    {
        // Assemble
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);
        var newFile = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);

        // Act
        await _webDavFile.WriteAllBytesAsync(existingFile.Path, newFile.Data);

        // Assert
        var existingPathData = await _webDavFile.ReadAllBytesAsync(existingFile.Path);
        Assert.AreEqual(existingPathData, newFile.Data);
    }

    [Test]
    public async Task WriteAllBytesAsync_DeletedFile_WritesFile()
    {
        // Assemble
        var deletedFile = await TestWebDavFileBuilder.GetDeletedFile(_rootDirectory);
        var newFile = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);

        // Act
        await _webDavFile.WriteAllBytesAsync(deletedFile.Path, newFile.Data);

        // Assert
        var existingPathData = await _webDavFile.ReadAllBytesAsync(deletedFile.Path);
        Assert.AreEqual(newFile.Data, existingPathData);
    }

    [Test]
    public async Task DeleteAsync_DeletedFile_ThrowsIOException()
    {
        // Assemble
        var deletedFile = await TestWebDavFileBuilder.GetDeletedFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavFile.DeleteAsync(deletedFile.Path));
    }

    [Test]
    public async Task DeleteAsync_ExistingFile_DeletesFile()
    {
        // Assemble
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);

        // Act
        await _webDavFile.DeleteAsync(existingFile.Path);

        // Assert
        var exists = await _webDavFile.ExistsAsync(existingFile.Path);
        Assert.IsFalse(exists);
    }

    [Test]
    public async Task DeleteAsync_InvalidFile_IOException()
    {
        // Assemble
        var invalidFile = await TestWebDavFileBuilder.GetInvalidFile();

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavFile.DeleteAsync(invalidFile.Path));
    }

    [Test]
    public async Task DeleteAsync_NewFile_IOException()
    {
        // Assemble
        var newFile = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavFile.DeleteAsync(newFile.Path));
    }

    [Test]
    public async Task MoveAsync_SourceDeletedFile_IOException()
    {
        // Assemble
        var deletedFile = await TestWebDavFileBuilder.GetDeletedFile(_rootDirectory);
        var newFile = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavFile.MoveAsync(deletedFile.Path, newFile.Path));
    }

    [Test]
    public async Task MoveAsync_SourceExistingFileDestExistingFileOverwriteFalse_IOException()
    {
        // Assemble
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);
        var existingFile2 = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavFile.MoveAsync(existingFile.Path, existingFile2.Path));
    }

    [Test]
    public async Task MoveAsync_SourceExistingFileDestExistingFileOverwriteTrue_MovesFile()
    {
        // Assemble
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);
        var existingFile2 = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);

        // Act
        await _webDavFile.MoveAsync(existingFile.Path, existingFile2.Path, true);

        // Assert
        var movedFileData = await _webDavFile.ReadAllBytesAsync(existingFile2.Path);
        Assert.AreEqual(existingFile.Data, movedFileData);

        var existingFileExists = await _webDavFile.ExistsAsync(existingFile.Path);
        Assert.IsFalse(existingFileExists);
    }

    [Test]
    public async Task MoveAsync_SourceExistingFileDestNewFile_MovesFile()
    {
        // Assemble
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);
        var newFile = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);

        // Act
        await _webDavFile.MoveAsync(existingFile.Path, newFile.Path, true);

        // Assert
        var movedFileData = await _webDavFile.ReadAllBytesAsync(newFile.Path);
        Assert.AreEqual(existingFile.Data, movedFileData);

        var existingFileExists = await _webDavFile.ExistsAsync(existingFile.Path);
        Assert.IsFalse(existingFileExists);
    }

    [Test]
    public async Task MoveAsync_SourceNewFile_IOException()
    {
        // Assemble
        var newFile = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);
        var newFile2 = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavFile.MoveAsync(newFile.Path, newFile2.Path));
    }

    [Test]
    public async Task CopyAsync_SourceDeletedFile_IOException()
    {
        // Assemble
        var deletedFile = await TestWebDavFileBuilder.GetDeletedFile(_rootDirectory);
        var newFile = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavFile.CopyAsync(deletedFile.Path, newFile.Path));
    }

    [Test]
    public async Task CopyAsync_SourceExistingFileDestExistingFileOverwriteFalse_IOException()
    {
        // Assemble
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);
        var existingFile2 = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavFile.CopyAsync(existingFile.Path, existingFile2.Path));
    }

    [Test]
    public async Task CopyAsync_SourceExistingFileDestExistingFileOverwriteTrue_CopiesFile()
    {
        // Assemble
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);
        var existingFile2 = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);

        // Act
        await _webDavFile.CopyAsync(existingFile.Path, existingFile2.Path, true);

        // Assert
        var movedFileData = await _webDavFile.ReadAllBytesAsync(existingFile2.Path);
        Assert.AreEqual(existingFile.Data, movedFileData);

        var existingFileExists = await _webDavFile.ExistsAsync(existingFile.Path);
        Assert.IsTrue(existingFileExists);
    }

    [Test]
    public async Task CopyAsync_SourceExistingFileDestNewFile_CopiesFile()
    {
        // Assemble
        var existingFile = await TestWebDavFileBuilder.GetExistingFile(_rootDirectory);
        var newFile = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);

        // Act
        await _webDavFile.CopyAsync(existingFile.Path, newFile.Path, true);

        // Assert
        var movedFileData = await _webDavFile.ReadAllBytesAsync(newFile.Path);
        Assert.AreEqual(existingFile.Data, movedFileData);

        var existingFileExists = await _webDavFile.ExistsAsync(existingFile.Path);
        Assert.IsTrue(existingFileExists);
    }

    [Test]
    public async Task CopyAsync_SourceNewFile_IOException()
    {
        // Assemble
        var newFile = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);
        var newFile2 = await TestWebDavFileBuilder.GetNewFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _webDavFile.CopyAsync(newFile.Path, newFile2.Path));
    }
}
