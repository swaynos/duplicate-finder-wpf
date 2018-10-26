using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository.Utilities
{
    internal class SqlQuery
    {
        /// <summary>
        /// Will return a formatted sql query based on the provided query and location path.
        /// This will clean-up the location path if it ends in a '\' (or multiple '\'), i.e. C:\ or C:\\\\, so that 
        /// it will be used correctly in the sql query.
        /// <para>For example the query "This is my {0} query" will be returned as 
        /// "This is my C:\foo query"</para>
        /// </summary>
        /// <param name="query">Composite Format String in which {0} will be replaced with
        /// the provided location path.</param>
        /// <param name="locationPath">The location path which will be inserted into the query</param>
        /// <returns>The formatted query with the provided location path. 
        /// Returns an exception if the location path is invalid.</returns>
        public static string FormatSqlQuery(string query, string locationPath)
        {
            // Validate the locationPath
            if (locationPath.Length < 3)
            {
                throw new ArgumentException("The provided path is too short", "locationPath");
            }

            string modifiedLocationpath = locationPath.TrimEnd('\\');
            return string.Format(query, modifiedLocationpath);
        }
    }
}
