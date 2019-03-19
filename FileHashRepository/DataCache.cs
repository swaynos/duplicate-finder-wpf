using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository
{
    public class DataCache<T> : IDataCache<T>
    {
        private List<T> _data;

        public DataCache(List<T> data)
        {
            _data = data;
        }

        // ToDo: Rename to "Add"?
        public void InsertData(T data)
        {
            _data.Add(data);
        }

        public IQueryable<T> ListData()
        {
            return _data.AsQueryable();
        }

        public void PurgeData(IQueryable<T> purged)
        {
            _data = _data.Except(purged).ToList();
        }
    }
}
