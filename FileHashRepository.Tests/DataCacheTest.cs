using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileHashRepository.Tests
{
    [TestClass]
    public class DataCacheTest
    {
        [TestMethod]
        public void ListData_ReturnsAllData()
        {
            // ARRANGE
            List<string> data = new List<string>()
            {
                "foo",
                "bar"
            };
            DataCache<string> dataCache = new DataCache<string>(data);

            // ACT
            var result = dataCache.ListData().ToList();

            // ASSERT
            Assert.AreEqual(2, result.Count, "The incorrect count of data was returned.");
            Assert.AreEqual("foo", result[0]);
            Assert.AreEqual("bar", result[1]);
        }

        [TestMethod]
        public void InsertData_InsertsNewData()
        {
            // ARRANGE
            List<string> data = new List<string>();
            DataCache<string> dataCache = new DataCache<string>(data);

            // ACT
            dataCache.InsertData("foo");
            var result = dataCache.ListData().ToList();

            // ASSERT
            Assert.AreEqual(1, result.Count, "The incorrect count of data was returned.");
            Assert.AreEqual("foo", result[0]);
        }

        [TestMethod]
        public void PurgeData_PurgesQueriedRecords()
        {
            // ARRANGE
            List<string> data = new List<string>()
            {
                "foo",
                "bar",
                "foobar",
                "testing"
            };
            DataCache<string> dataCache = new DataCache<string>(data);
            IQueryable<string> dataQuery = dataCache.ListData().Where(t => t.Equals("testing"));

            // ACT
            dataCache.PurgeData(dataQuery);
            var result = dataCache.ListData().ToList();

            // ASSERT
            Assert.AreEqual(3, result.Count, "The incorrect count of data was returned.");
            //Assert.AreEqual("foo", result[0]);
            //Assert.AreEqual("bar", result[1]);
        }
    }
}
