using CounterStrike.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public interface ICrudServiceAsync<T> where T : class
{
    Task<bool> CreateAsync(T element);
    Task<T> ReadAsync(Guid id);
    Task<IEnumerable<T>> ReadAllAsync();
    Task<IEnumerable<T>> ReadAllAsync(int page, int amount);
    Task<bool> UpdateAsync(T element);
    Task<bool> RemoveAsync(T element);
}

public class CrudServiceAsync<T> : ICrudServiceAsync<T> where T : class
{
    private readonly Repository<T> _repository;

    public CrudServiceAsync(Repository<T> repository)
    {
        _repository = repository;
    }

    public async Task<bool> CreateAsync(T element)
    {
        await _repository.AddAsync(element);
        return true;
    }

    public async Task<T> ReadAsync(Guid id)
    {
        var all = await _repository.GetAllAsync();
        var prop = typeof(T).GetProperty("Id");
        if (prop == null) throw new Exception("Клас T повинен мати Id");
        return all.FirstOrDefault(e => (Guid)prop.GetValue(e) == id);
    }

    public async Task<IEnumerable<T>> ReadAllAsync() => await _repository.GetAllAsync();

    public async Task<IEnumerable<T>> ReadAllAsync(int page, int amount)
    {
        var all = await _repository.GetAllAsync();
        return all.Skip((page - 1) * amount).Take(amount);
    }

    public async Task<bool> UpdateAsync(T element)
    {
        await _repository.UpdateAsync(element);
        return true;
    }

    public async Task<bool> RemoveAsync(T element)
    {
        await _repository.DeleteAsync(element);
        return true;
    }
}
