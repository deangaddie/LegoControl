using LegoControl.Core.Models;

namespace LegoControl.Core.Services;

public interface ILegoModelService
{
    IReadOnlyList<LegoModel> Models { get; }
    Task InitializeAsync();
    Task AddAsync(LegoModel model);
    Task UpdateAsync(LegoModel model);
    Task RemoveAsync(string id);
    Task RevertAsync(string id);
}
