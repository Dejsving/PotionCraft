using PotionCraft.Contracts.Models;

namespace PotionCraft.Server.Services.Gathering;

public interface IGatheringService
{
    Task<GatheringResult> GatherHerbAsync(GatheringRequest request);
}
