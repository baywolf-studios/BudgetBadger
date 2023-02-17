using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BudgetBadger.FileSystem.Dropbox;
using NUnit.Framework;

namespace BudgetBadger.IntegrationTests.FileSystem.Dropbox;

[Ignore("No credentials in GitHub")]
[TestFixture]
public class DropboxFileIntegrationTests
{
    [SetUp]
    public async Task Setup()
    {
        _dropboxFile = new DropboxFile();
        _dropboxFile.SetAuthentication(new Dictionary<string, string>
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

    private DropboxFile _dropboxFile;
    private const string _rootDirectory = "/DropboxFileIntegrationTests";

    [Test]
    public async Task ExistsAsync_ExistingFile_ReturnsTrue()
    {
        // Arrange
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);

        // Act
        var result = await _dropboxFile.ExistsAsync(existingFile.Path);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task ExistsAsync_NewFile_ReturnsFalse()
    {
        // Arrange
        var missingFile = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);

        // Act
        var result = await _dropboxFile.ExistsAsync(missingFile.Path);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task ExistsAsync_DeletedFile_ReturnsFalse()
    {
        // Arrange
        var deletedFile = await TestDropboxFileBuilder.GetDeletedFile(_rootDirectory);

        // Act
        var result = await _dropboxFile.ExistsAsync(deletedFile.Path);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task ExistsAsync_InvalidFile_ReturnsFalse()
    {
        // Arrange
        var invalidFile = await TestDropboxFileBuilder.GetInvalidFile();

        // Act
        var result = await _dropboxFile.ExistsAsync(invalidFile.Path);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public async Task ReadAllBytesAsync_NewFile_ThrowsIOException()
    {
        // Arrange
        var missingFile = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);

        // Act and Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxFile.ReadAllBytesAsync(missingFile.Path));
    }

    [Test]
    public async Task ReadAllBytesAsync_DeletedFile_ThrowsIOException()
    {
        // Arrange
        var deletedFile = await TestDropboxFileBuilder.GetDeletedFile(_rootDirectory);

        // Act and Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxFile.ReadAllBytesAsync(deletedFile.Path));
    }

    [Test]
    public async Task ReadAllBytesAsync_ExistingFile_ReturnsCorrectBytes()
    {
        // Arrange
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);

        // Act
        var result = await _dropboxFile.ReadAllBytesAsync(existingFile.Path);

        // Assert
        Assert.AreEqual(existingFile.Data, result);
    }

    [Test]
    public async Task ReadAllBytesAsync_InvalidFile_ThrowsIOException()
    {
        // Arrange
        var invalidFile = await TestDropboxFileBuilder.GetInvalidFile();

        // Act and Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxFile.ReadAllBytesAsync(invalidFile.Path));
    }

    [Test]
    public async Task WriteAllBytesAsync_NewFile_WritesFile()
    {
        // Arrange
        var newFile = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);

        // Act
        await _dropboxFile.WriteAllBytesAsync(newFile.Path, newFile.Data);

        // Assert
        Assert.Pass();
    }

    [Test]
    public async Task WriteAllBytesAsync_InvalidFile_IOException()
    {
        // Arrange
        var invalidFile = await TestDropboxFileBuilder.GetInvalidFile();

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxFile.WriteAllBytesAsync(invalidFile.Path, invalidFile.Data));
    }

    [Test]
    public async Task WriteAllBytesAsync_ExistingFile_OverwritesFile()
    {
        // Assemble
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);
        var newFile = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);

        // Act
        await _dropboxFile.WriteAllBytesAsync(existingFile.Path, newFile.Data);

        // Assert
        var existingPathData = await _dropboxFile.ReadAllBytesAsync(existingFile.Path);
        Assert.AreEqual(existingPathData, newFile.Data);
    }

    [Test]
    public async Task WriteAllBytesAsync_DeletedFile_WritesFile()
    {
        // Assemble
        var deletedFile = await TestDropboxFileBuilder.GetDeletedFile(_rootDirectory);
        var newFile = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);

        // Act
        await _dropboxFile.WriteAllBytesAsync(deletedFile.Path, newFile.Data);

        // Assert
        var existingPathData = await _dropboxFile.ReadAllBytesAsync(deletedFile.Path);
        Assert.AreEqual(newFile.Data, existingPathData);
    }

    [Test]
    public async Task DeleteAsync_DeletedFile_ThrowsIOException()
    {
        // Assemble
        var deletedFile = await TestDropboxFileBuilder.GetDeletedFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxFile.DeleteAsync(deletedFile.Path));
    }

    [Test]
    public async Task DeleteAsync_ExistingFile_DeletesFile()
    {
        // Assemble
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);

        // Act
        await _dropboxFile.DeleteAsync(existingFile.Path);

        // Assert
        var exists = await _dropboxFile.ExistsAsync(existingFile.Path);
        Assert.IsFalse(exists);
    }

    [Test]
    public async Task DeleteAsync_InvalidFile_IOException()
    {
        // Assemble
        var invalidFile = await TestDropboxFileBuilder.GetInvalidFile();

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxFile.DeleteAsync(invalidFile.Path));
    }

    [Test]
    public async Task DeleteAsync_NewFile_IOException()
    {
        // Assemble
        var newFile = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxFile.DeleteAsync(newFile.Path));
    }

    [Test]
    public async Task MoveAsync_SourceDeletedFile_IOException()
    {
        // Assemble
        var deletedFile = await TestDropboxFileBuilder.GetDeletedFile(_rootDirectory);
        var newFile = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxFile.MoveAsync(deletedFile.Path, newFile.Path));
    }

    [Test]
    public async Task MoveAsync_SourceExistingFileDestExistingFileOverwriteFalse_IOException()
    {
        // Assemble
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);
        var existingFile2 = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxFile.MoveAsync(existingFile.Path, existingFile2.Path));
    }

    [Test]
    public async Task MoveAsync_SourceExistingFileDestExistingFileOverwriteTrue_MovesFile()
    {
        // Assemble
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);
        var existingFile2 = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);

        // Act
        await _dropboxFile.MoveAsync(existingFile.Path, existingFile2.Path, true);

        // Assert
        var movedFileData = await _dropboxFile.ReadAllBytesAsync(existingFile2.Path);
        Assert.AreEqual(existingFile.Data, movedFileData);

        var existingFileExists = await _dropboxFile.ExistsAsync(existingFile.Path);
        Assert.IsFalse(existingFileExists);
    }

    [Test]
    public async Task MoveAsync_SourceExistingFileDestNewFile_MovesFile()
    {
        // Assemble
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);
        var newFile = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);

        // Act
        await _dropboxFile.MoveAsync(existingFile.Path, newFile.Path, true);

        // Assert
        var movedFileData = await _dropboxFile.ReadAllBytesAsync(newFile.Path);
        Assert.AreEqual(existingFile.Data, movedFileData);

        var existingFileExists = await _dropboxFile.ExistsAsync(existingFile.Path);
        Assert.IsFalse(existingFileExists);
    }

    [Test]
    public async Task MoveAsync_SourceNewFile_IOException()
    {
        // Assemble
        var newFile = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);
        var newFile2 = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxFile.MoveAsync(newFile.Path, newFile2.Path));
    }

    [Test]
    public async Task CopyAsync_SourceDeletedFile_IOException()
    {
        // Assemble
        var deletedFile = await TestDropboxFileBuilder.GetDeletedFile(_rootDirectory);
        var newFile = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxFile.CopyAsync(deletedFile.Path, newFile.Path));
    }

    [Test]
    public async Task CopyAsync_SourceExistingFileDestExistingFileOverwriteFalse_IOException()
    {
        // Assemble
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);
        var existingFile2 = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxFile.CopyAsync(existingFile.Path, existingFile2.Path));
    }

    [Test]
    public async Task CopyAsync_SourceExistingFileDestExistingFileOverwriteTrue_CopiesFile()
    {
        // Assemble
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);
        var existingFile2 = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);

        // Act
        await _dropboxFile.CopyAsync(existingFile.Path, existingFile2.Path, true);

        // Assert
        var movedFileData = await _dropboxFile.ReadAllBytesAsync(existingFile2.Path);
        Assert.AreEqual(existingFile.Data, movedFileData);

        var existingFileExists = await _dropboxFile.ExistsAsync(existingFile.Path);
        Assert.IsTrue(existingFileExists);
    }

    [Test]
    public async Task CopyAsync_SourceExistingFileDestNewFile_CopiesFile()
    {
        // Assemble
        var existingFile = await TestDropboxFileBuilder.GetExistingFile(_rootDirectory);
        var newFile = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);

        // Act
        await _dropboxFile.CopyAsync(existingFile.Path, newFile.Path, true);

        // Assert
        var movedFileData = await _dropboxFile.ReadAllBytesAsync(newFile.Path);
        Assert.AreEqual(existingFile.Data, movedFileData);

        var existingFileExists = await _dropboxFile.ExistsAsync(existingFile.Path);
        Assert.IsTrue(existingFileExists);
    }

    [Test]
    public async Task CopyAsync_SourceNewFile_IOException()
    {
        // Assemble
        var newFile = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);
        var newFile2 = await TestDropboxFileBuilder.GetNewFile(_rootDirectory);

        // Act Assert
        Assert.ThrowsAsync<IOException>(() => _dropboxFile.CopyAsync(newFile.Path, newFile2.Path));
    }
}