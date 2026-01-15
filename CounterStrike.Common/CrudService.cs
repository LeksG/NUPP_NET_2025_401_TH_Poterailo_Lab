using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CounterStrike.Common
{
    public interface ICrudServiceAsync<T> : IEnumerable<T>
    {
        Task<bool> CreateAsync(T element);
        Task<T> ReadAsync(Guid id);
        Task<IEnumerable<T>> ReadAllAsync();
        Task<IEnumerable<T>> ReadAllAsync(int page, int amount);
        Task<bool> UpdateAsync(T element);
        Task<bool> RemoveAsync(T element);
        Task<bool> SaveAsync();
    }

    public class CrudServiceAsync<T> : ICrudServiceAsync<T> where T : class
    {
        private readonly ConcurrentDictionary<Guid, T> _data = new();
        private readonly SemaphoreSlim _saveLock = new(1, 1);
        private readonly string _filePath;

        public CrudServiceAsync(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<bool> CreateAsync(T element)
        {
            var prop = typeof(T).GetProperty("Id") ?? throw new Exception("Клас T повинен мати властивість Id типу Guid");
            Guid id = (Guid)prop.GetValue(element);
            bool added = _data.TryAdd(id, element);
            await Task.Yield();
            return added;
        }

        public async Task<T> ReadAsync(Guid id)
        {
            _data.TryGetValue(id, out T element);
            await Task.Yield();
            return element;
        }

        public async Task<IEnumerable<T>> ReadAllAsync()
        {
            await Task.Yield();
            return _data.Values;
        }

        public async Task<IEnumerable<T>> ReadAllAsync(int page, int amount)
        {
            await Task.Yield();
            return _data.Values.Skip((page - 1) * amount).Take(amount);
        }

        public async Task<bool> UpdateAsync(T element)
        {
            var prop = typeof(T).GetProperty("Id") ?? throw new Exception("Клас T повинен мати властивість Id типу Guid");
            Guid id = (Guid)prop.GetValue(element);

            bool updated = false;
            if (_data.ContainsKey(id))
            {
                _data[id] = element;
                updated = true;
            }
            await Task.Yield();
            return updated;
        }

        public async Task<bool> RemoveAsync(T element)
        {
            var prop = typeof(T).GetProperty("Id") ?? throw new Exception("Клас T повинен мати властивість Id типу Guid");
            Guid id = (Guid)prop.GetValue(element);

            bool removed = _data.TryRemove(id, out _);
            await Task.Yield();
            return removed;
        }

        public async Task<bool> SaveAsync()
        {
            await _saveLock.WaitAsync();
            try
            {
                string json = JsonSerializer.Serialize(_data.Values);
                await File.WriteAllTextAsync(_filePath, json);
                return true;
            }
            finally
            {
                _saveLock.Release();
            }
        }

        public IEnumerator<T> GetEnumerator() => _data.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _data.Values.GetEnumerator();
    }
}