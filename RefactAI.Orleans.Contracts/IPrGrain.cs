using Orleans;

namespace RefactAI.Orleans.Contracts
{
    public interface IPrGrain : IGrainWithStringKey
    {
        Task<PrResult> ProcessPr(PrRequest request);
    }
}
