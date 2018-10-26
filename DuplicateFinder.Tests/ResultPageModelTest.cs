using System;
using System.Collections.Generic;
using DuplicateFinder.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DuplicateFinder.Tests
{
    [TestClass]
    public class ResultPageModelTest
    {
        [TestMethod]
        public void FlattenFoundDuplicates_ReturnsAllFileModelLocationPaths()
        {
            // ARRANGE
            ResultPageModel model = ConstructModel(3);
            model.FoundDuplicates[0].DuplicateFiles.Add(ConstructFileModel("1"));
            model.FoundDuplicates[0].DuplicateFiles.Add(ConstructFileModel("2"));
            model.FoundDuplicates[0].DuplicateFiles.Add(ConstructFileModel("3"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("4"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("5"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("6"));
            model.FoundDuplicates[2].DuplicateFiles.Add(ConstructFileModel("7"));
            model.FoundDuplicates[2].DuplicateFiles.Add(ConstructFileModel("8"));

            // ACT
            List<string> results = model.FlattenFoundDuplicates();

            // ASSERT
            Assert.AreEqual(8, results.Count);
            Assert.AreEqual("C:\\foo\\bar\\1.txt", results[0]);
        }

        [TestMethod]
        public void RemoveFileModelFromModel_SingularLocationPath_IsRemoved()
        {
            // ARRANGE
            ResultPageModel model = ConstructModel(3);
            model.FoundDuplicates[0].DuplicateFiles.Add(ConstructFileModel("1", "dir", "png"));
            model.FoundDuplicates[0].DuplicateFiles.Add(ConstructFileModel("2"));
            model.FoundDuplicates[0].DuplicateFiles.Add(ConstructFileModel("3"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("4"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("5"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("6"));
            model.FoundDuplicates[2].DuplicateFiles.Add(ConstructFileModel("7"));
            model.FoundDuplicates[2].DuplicateFiles.Add(ConstructFileModel("8"));

            // ACT
            model.RemoveFileModelFromModel("dir\\1.png");

            // ASSERT
            Assert.AreEqual(2, model.FoundDuplicates[0].DuplicateFiles.Count);
            Assert.AreEqual(3, model.FoundDuplicates[1].DuplicateFiles.Count);
            Assert.AreEqual(2, model.FoundDuplicates[2].DuplicateFiles.Count);
        }

        [TestMethod]
        public void RemoveFileModelFromModel_NoDuplicateFilesRemain_RemovesFoundDuplicate()
        {
            // ARRANGE
            ResultPageModel model = ConstructModel(3);
            model.FoundDuplicates[0].DuplicateFiles.Add(ConstructFileModel("1", "dir", "png"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("4"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("5"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("6"));
            model.FoundDuplicates[2].DuplicateFiles.Add(ConstructFileModel("7"));
            model.FoundDuplicates[2].DuplicateFiles.Add(ConstructFileModel("8"));

            // ACT
            model.RemoveFileModelFromModel("dir\\1.png");

            // ASSERT
            Assert.AreEqual(2, model.FoundDuplicates.Count);
        }

        [TestMethod]
        public void RemoveFileModelFromModel_ManyFoundLocationPaths_AreRemoved()
        {
            // ARRANGE
            ResultPageModel model = ConstructModel(3);
            model.FoundDuplicates[0].DuplicateFiles.Add(ConstructFileModel("file", "dir", "txt"));
            model.FoundDuplicates[0].DuplicateFiles.Add(ConstructFileModel("file", "dir", "txt"));
            model.FoundDuplicates[0].DuplicateFiles.Add(ConstructFileModel("3"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("4"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("5"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("6"));
            model.FoundDuplicates[2].DuplicateFiles.Add(ConstructFileModel("7"));
            model.FoundDuplicates[2].DuplicateFiles.Add(ConstructFileModel("8"));

            // ACT
            model.RemoveFileModelFromModel("dir\\file.txt");

            // ASSERT
            Assert.AreEqual(1, model.FoundDuplicates[0].DuplicateFiles.Count);
            Assert.AreEqual(3, model.FoundDuplicates[1].DuplicateFiles.Count);
            Assert.AreEqual(2, model.FoundDuplicates[2].DuplicateFiles.Count);
        }

        [TestMethod]
        public void RemoveFileModelFromModel_NoFoundLocationPaths_NoneRemoved()
        {
            // ARRANGE
            ResultPageModel model = ConstructModel(3);
            model.FoundDuplicates[0].DuplicateFiles.Add(ConstructFileModel("1"));
            model.FoundDuplicates[0].DuplicateFiles.Add(ConstructFileModel("2"));
            model.FoundDuplicates[0].DuplicateFiles.Add(ConstructFileModel("3"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("4"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("5"));
            model.FoundDuplicates[1].DuplicateFiles.Add(ConstructFileModel("6"));
            model.FoundDuplicates[2].DuplicateFiles.Add(ConstructFileModel("7"));
            model.FoundDuplicates[2].DuplicateFiles.Add(ConstructFileModel("8"));

            // ACT
            model.RemoveFileModelFromModel("dir\\file.txt");

            // ASSERT
            Assert.AreEqual(3, model.FoundDuplicates[0].DuplicateFiles.Count);
            Assert.AreEqual(3, model.FoundDuplicates[1].DuplicateFiles.Count);
            Assert.AreEqual(2, model.FoundDuplicates[2].DuplicateFiles.Count);
        }

        /// <summary>
        /// Helper method to construct a ResultPageModel
        /// </summary>
        private ResultPageModel ConstructModel(int numberOfDuplicateResultModels)
        {
            ResultPageModel model = new ResultPageModel();
            model.FoundDuplicates = new List<DuplicateResultModel>();
            for (int i  = 0; i < numberOfDuplicateResultModels; i++)
            {
                model.FoundDuplicates.Add(new DuplicateResultModel()
                {
                    DuplicateFiles = new List<FileModel>()
                });
            }
            return model;
        }

        /// <summary>
        /// Helper method to construct a new FileModel
        /// </summary>
        private FileModel ConstructFileModel(string name, string directory = "C:\\foo\\bar", string extension = "txt")
        {
            Random random = new Random();
            string fileName = string.Format("{0}.{1}", name, extension);
            string locationPath = string.Format("{0}\\{1}", directory, fileName);
            return new FileModel()
            {
                FileName = fileName,
                LocationPath = string.Format("{0}\\{1}", directory, fileName)
            };
        }
           
    }
}
