namespace HRSystem.API.Services;

public interface IReferenceNumberService
{
    Task<string> NextAsync(string sequenceKey, CancellationToken cancellationToken = default);
    Task<string> PreviewAsync(string sequenceKey, CancellationToken cancellationToken = default);
}
