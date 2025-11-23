using AMS.Application.Common.Models;

namespace AMS.Application.Interfaces;

public interface IDataSeederService
{
    Task<Result> GenerateDataAsync(int studentCount, int professorCount, Action<string> onProgress,
        CancellationToken ct);

    Task SeedAsync(CancellationToken ct);
}