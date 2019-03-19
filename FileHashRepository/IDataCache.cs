using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashRepository
{
    public interface IDataCache<T>
    {
        IQueryable<T> ListData();

        void InsertData(T data);

        void PurgeData(IQueryable<T> purged);
    }
}
