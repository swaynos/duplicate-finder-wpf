using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace FileHashRepository.Tests.Mocks
{
    // https://msdn.microsoft.com/en-us/library/dn314429(v=vs.113).aspx
    class MockDbAsyncEnumerable<T> : EnumerableQuery<T>, IDbAsyncEnumerable<T>, IQueryable<T>
    {
        public MockDbAsyncEnumerable(IEnumerable<T> enumerable) 
            : base(enumerable) 
        { }

        public MockDbAsyncEnumerable(Expression expression) 
            : base(expression) 
        { }

        public IDbAsyncEnumerator<T> GetAsyncEnumerator()
        {
            return new MockDbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new MockDbAsyncQueryProvider<T>(this); }
        }
    }
}
