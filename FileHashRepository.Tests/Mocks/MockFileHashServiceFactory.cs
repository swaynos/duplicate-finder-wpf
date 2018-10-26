using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository.Tests.Mocks
{
    class MockFileHashServiceFactory : IFileHashServiceFactory
    {
        public List<ScannedFile> ScannedFiles { get; set; }

        public Mock<IFileHashService> MockFileHashService { get; set; }

        public MockFileHashServiceFactory()
        {
            ScannedFiles = new List<ScannedFile>();

            // Create a MockFileHashService that add's the ScannedFile to the ScannedFiles
            // property when InsertScannedFileAsync is called
            MockFileHashService = new Mock<IFileHashService>();
            MockFileHashService.Setup(t => t.InsertScannedFileAsync(It.IsAny<ScannedFile>()))
                .Callback((ScannedFile s) => { ScannedFiles.Add(s); })
                .Returns(Task.CompletedTask);
            MockFileHashService.Setup(t => t.ListScannedFilePathsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync((List<string> locations) => 
                    // Mock the expected behavior that if the locations are empty a new empty list is returned
                    {
                        if (locations.Count == 0)
                        {
                            return new List<string>();
                        }
                        return ScannedFiles.Select(t => t.Path).ToList();
                    });
            MockFileHashService.Setup(t => t.RemoveScannedFilesByFilePathAsync(It.IsAny<string>()))
                .ReturnsAsync((string path) => { return RemoveScannedFilesByFilePath(path); });
            MockFileHashService.Setup(t => t.ReturnDuplicatesAsync())
                .ReturnsAsync(() =>
                {
                    return ScannedFiles;
                });
        }

        public IFileHashService GetFileHashService()
        {
            return MockFileHashService.Object;
        }

        public int RemoveScannedFilesByFilePath(string filePath)
        {
            Stack<ScannedFile> remove = new Stack<ScannedFile>();
            for (int i = 0; i < ScannedFiles.Count; i++)
            {
                ScannedFile file = ScannedFiles[i];
                if (file.Path.Equals(filePath))
                {
                    remove.Push(file);
                }
            }
            int result = remove.Count;
            while (remove.Count > 0)
            {
                ScannedFiles.Remove(remove.Pop());
            }
            return result;
        }
    }
}
