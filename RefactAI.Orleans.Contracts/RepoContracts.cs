using Orleans;
using Orleans.Serialization;

namespace RefactAI.Orleans.Contracts
{
    public interface IRepoGrain : IGrainWithStringKey
    {
        Task EnqueueWork(RepoWorkItem item);
    }

    [GenerateSerializer]
    public record RepoWorkItem(
        [property: Id(0)] string RepoUrl,
        [property: Id(1)] string Branch,
        [property: Id(2)] string Sha,
        [property: Id(3)] string Kind
    );

    // Pull Request processing input
    [GenerateSerializer]
    public record PrRequest(
        [property: Id(0)] string RepoUrl,
        [property: Id(1)] string Sha,
        [property: Id(2)] string PrNumber
    );

    // Pull Request processing output/result
    [GenerateSerializer]
    public record PrResult(
        [property: Id(0)] string Status,
        [property: Id(1)] string Message
    );
}
