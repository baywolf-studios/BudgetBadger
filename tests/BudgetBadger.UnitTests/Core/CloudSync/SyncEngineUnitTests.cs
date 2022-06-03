using System;
using System.Threading.Tasks;
using BudgetBadger.Core.CloudSync;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.FileSystem;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Utilities;
using FakeItEasy;
using NUnit.Framework;

namespace BudgetBadger.UnitTests.Core.CloudSync;

[TestFixture]
public class SyncEngineUnitTests
{
    private SyncEngine SyncEngine { get; set; }
    private IMergeLogic _mergeLogic { get; set; }
    private IDataAccess _sourceDataAccess { get; set; }
    private IDataAccess _targetDataAccess { get; set; }
    private IDataAccess _tempDataAccess { get; set; }
    private IFileSystem _tempFileSystem { get; set; }
    private IFileSystem _cloudFileSystem { get; set; }

    [SetUp]
    public void Setup()
    {
        _sourceDataAccess = A.Fake<IDataAccess>();
        _targetDataAccess = A.Fake<IDataAccess>();
        _mergeLogic = A.Fake<IMergeLogic>();
        _tempDataAccess = A.Fake<IDataAccess>();
        _tempFileSystem = A.Fake<IFileSystem>();
        _cloudFileSystem = A.Fake<IFileSystem>();
        SyncEngine = new SyncEngine(_mergeLogic);
        A.CallTo(() => _tempFileSystem.File.ExistsAsync(A<string>._)).Returns(true);
        A.CallTo(() => _cloudFileSystem.File.ExistsAsync(A<string>._)).Returns(true);
    }
    
    [Test]
    public async Task Import_MergeLogicThrowsException_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => _mergeLogic.MergeAllAsync(A<IDataAccess>._, A<IDataAccess>._)).Throws(new Exception());

        // act
        var result = await SyncEngine.ImportAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        Assert.IsFalse(result.Success);
    }
    
    [Test]
    public async Task Export_MergeLogicThrowsException_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => _mergeLogic.MergeAllAsync(A<IDataAccess>._, A<IDataAccess>._)).Throws(new Exception());

        // act
        var result = await SyncEngine.ExportAsync(_sourceDataAccess, _targetDataAccess);

        // assert
        Assert.IsFalse(result.Success);
    }

    [Test]
    public async Task FileBasedImport_ImportFileSystemThrowsException_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => _cloudFileSystem.File.ReadAllBytesAsync(A<string>._)).Throws(new Exception());

        // act
        var result = await SyncEngine.FileBasedImportAsync(_cloudFileSystem, string.Empty, false, _tempFileSystem,
            string.Empty, _tempDataAccess, _targetDataAccess); 

        // assert
        Assert.IsFalse(result.Success);
    }
    
    [Test]
    public async Task FileBasedImport_ImportFileDoesNotExist_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => _cloudFileSystem.File.ExistsAsync(A<string>._)).Returns(false);

        // act
        var result = await SyncEngine.FileBasedImportAsync(_cloudFileSystem, string.Empty, false, _tempFileSystem,
            string.Empty, _tempDataAccess, _targetDataAccess); 

        // assert
        Assert.IsFalse(result.Success);
    }
    
    [Test]
    public async Task FileBasedImport_DecompressImportFileTrue_DecompressesImportFile()
    {
        // arrange
        var rnd = new Random();
        var b = new byte[10];
        rnd.NextBytes(b);
        var b2 = b.Compress();
        A.CallTo(() => _cloudFileSystem.File.ReadAllBytesAsync(A<string>._)).Returns(b2);
        var d = b2.Decompress();
        
        // act
        await SyncEngine.FileBasedImportAsync(_cloudFileSystem, string.Empty, true, _tempFileSystem,
            string.Empty, _tempDataAccess, _targetDataAccess); 

        // assert
        A.CallTo(() => _tempFileSystem.File.WriteAllBytesAsync(A<string>._, A<byte[]>.That.IsSameSequenceAs(d))).MustHaveHappened();
    }
    
    [Test]
    public async Task FileBasedImport_DecompressImportFileFalse_DoesNotDecompressImportFile()
    {
        // arrange
        var rnd = new Random();
        var b = new byte[10];
        rnd.NextBytes(b);
        A.CallTo(() => _cloudFileSystem.File.ReadAllBytesAsync(A<string>._)).Returns(b);

        // act
        await SyncEngine.FileBasedImportAsync(_cloudFileSystem, string.Empty, false, _tempFileSystem,
            string.Empty, _tempDataAccess, _targetDataAccess); 

        // assert
        A.CallTo(() => _tempFileSystem.File.WriteAllBytesAsync(A<string>._, b)).MustHaveHappened();
    }
    
    [Test]
    public async Task FileBasedImport_TempFileSystemThrowsException_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => _tempFileSystem.File.WriteAllBytesAsync(A<string>._, A<byte[]>._)).Throws(new Exception());

        // act
        var result = await SyncEngine.FileBasedImportAsync(_cloudFileSystem, string.Empty, false, _tempFileSystem,
            string.Empty, _tempDataAccess, _targetDataAccess); 

        // assert
        Assert.IsFalse(result.Success);
    }
    
    [Test]
    public async Task FileBasedImport_ValidImportFile_WritesToTempFile()
    {
        // arrange
        var rnd = new Random();
        var b = new byte[10];
        rnd.NextBytes(b);
        A.CallTo(() => _cloudFileSystem.File.ReadAllBytesAsync(A<string>._)).Returns(b);

        // act
        await SyncEngine.FileBasedImportAsync(_cloudFileSystem, string.Empty, false, _tempFileSystem,
            string.Empty, _tempDataAccess, _targetDataAccess); 

        // assert
        A.CallTo(() => _tempFileSystem.File.WriteAllBytesAsync(A<string>._, b)).MustHaveHappened();
    }
    
    [Test]
    public async Task FileBasedImport_NoExceptions_ReturnsSuccessful()
    {
        // arrange
        var rnd = new Random();
        var b = new byte[10];
        rnd.NextBytes(b);
        A.CallTo(() => _cloudFileSystem.File.ReadAllBytesAsync(A<string>._)).Returns(b);

        // act
        var result = await SyncEngine.FileBasedImportAsync(_cloudFileSystem, string.Empty, false, _tempFileSystem,
            string.Empty, _tempDataAccess, _targetDataAccess); 

        // assert
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task FileBasedExport_TempFileDoesNotExist_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => _tempFileSystem.File.ExistsAsync(A<string>._)).Returns(false);

        // act
        var result = await SyncEngine.FileBasedExportAsync(_sourceDataAccess, _tempDataAccess, _tempFileSystem,
            string.Empty, false, string.Empty,_cloudFileSystem);

        // assert
        Assert.IsFalse(result.Success);
    }
    
    [Test]
    public async Task FileBasedExport_TempFileSystemThrowsException_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => _tempFileSystem.File.ReadAllBytesAsync(A<string>._)).Throws(new Exception());

        // act
        var result = await SyncEngine.FileBasedExportAsync(_sourceDataAccess, _tempDataAccess, _tempFileSystem,
            string.Empty, false, string.Empty, _cloudFileSystem);

        // assert
        Assert.IsFalse(result.Success);
    }
    
    [Test]
    public async Task FileBasedExport_CompressTempFileTrue_CompressesTempFile()
    {
        // arrange
        var rnd = new Random();
        var b = new byte[10];
        rnd.NextBytes(b);
        A.CallTo(() => _tempFileSystem.File.ReadAllBytesAsync(A<string>._)).Returns(b);
        var d = b.Compress();
        
        // act
        await SyncEngine.FileBasedExportAsync(_sourceDataAccess, _tempDataAccess, _tempFileSystem,
            string.Empty, true, string.Empty,_cloudFileSystem); 

        // assert
        A.CallTo(() => _cloudFileSystem.File.WriteAllBytesAsync(A<string>._, A<byte[]>.That.IsSameSequenceAs(d))).MustHaveHappened();
    }
    
    [Test]
    public async Task FileBasedExport_CompressTempFileFalse_DoesNotCompressTempFile()
    {
        // arrange
        var rnd = new Random();
        var b = new byte[10];
        rnd.NextBytes(b);
        A.CallTo(() => _tempFileSystem.File.ReadAllBytesAsync(A<string>._)).Returns(b);

        // act
        await SyncEngine.FileBasedExportAsync(_sourceDataAccess, _tempDataAccess, _tempFileSystem,
            string.Empty, false, string.Empty,_cloudFileSystem); 

        // assert
        A.CallTo(() => _cloudFileSystem.File.WriteAllBytesAsync(A<string>._, b)).MustHaveHappened();
    }
    
    [Test]
    public async Task FileBasedExport_CloudFileSystemThrowsException_ReturnsUnsuccessful()
    {
        // arrange
        A.CallTo(() => _cloudFileSystem.File.WriteAllBytesAsync(A<string>._, A<byte[]>._)).Throws(new Exception());

        // act
        var result = await SyncEngine.FileBasedExportAsync(_sourceDataAccess, _tempDataAccess, _tempFileSystem,
            string.Empty, false, string.Empty,_cloudFileSystem); 

        // assert
        Assert.IsFalse(result.Success);
    }
    
    [Test]
    public async Task FileBasedExport_ValidTempFile_WritesToCloudFile()
    {
        // arrange
        var rnd = new Random();
        var b = new byte[10];
        rnd.NextBytes(b);
        A.CallTo(() => _tempFileSystem.File.ReadAllBytesAsync(A<string>._)).Returns(b);

        // act
        await SyncEngine.FileBasedExportAsync(_sourceDataAccess, _tempDataAccess, _tempFileSystem,
            string.Empty, false, string.Empty,_cloudFileSystem); 

        // assert
        A.CallTo(() => _cloudFileSystem.File.WriteAllBytesAsync(A<string>._, b)).MustHaveHappened();
    }
    
    [Test]
    public async Task FileBasedExport_NoExceptions_ReturnsSuccessful()
    {
        // act
        var result = await SyncEngine.FileBasedExportAsync(_sourceDataAccess, _tempDataAccess, _tempFileSystem,
            string.Empty, false, string.Empty,_cloudFileSystem);

        // assert
        Assert.IsTrue(result.Success);
    }
}