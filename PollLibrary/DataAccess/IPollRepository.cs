using PollLibrary.Models;

namespace PollLibrary.DataAccess;

public interface IPollRepository
{
    Task CreatePollAsync(Poll poll);
    Task<Poll?> GetPollAsync(Guid id);
    Task<IEnumerable<PollSummary>> GetPollSummariesAsync();
    Task VoteAsync(Guid pollId, int optionId);
}
