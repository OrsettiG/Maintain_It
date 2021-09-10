using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Maintain_it.Services
{
    public interface IDataStore<T>
    {
        Task Init();
        Task AddItemAsync( T item );
        Task UpdateItemAsync( T item );
        Task DeleteItemAsync( int id );
        Task<T> GetItemAsync( int id );
        Task<IEnumerable<T>> GetAllItemsAsync( bool forceRefresh = false );
    }
}
