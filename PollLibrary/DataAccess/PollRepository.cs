using System.Data;
using Dapper;
using PollLibrary.Models;

namespace PollLibrary.DataAccess;

public class PollRepository : IPollRepository
{
    private readonly IDbConnection _db;
    public PollRepository(IDbConnection db) => _db = db;

    public async Task CreatePollAsync(Poll poll)
    {
        const string insertPoll = @"
                INSERT INTO Poll (Id, Question)
                VALUES (@Id, @Question);
            ";
        const string insertOption = @"
                INSERT INTO Option (PollId, Text, Votes)
                VALUES (@PollId, @Text, 0);
            ";

        await _db.ExecuteAsync(insertPoll, new { poll.Id, poll.Question });

        foreach (var opt in poll.Options)
        {
            await _db.ExecuteAsync(insertOption, new
            {
                PollId = poll.Id,
                Text = opt.Text
            });
        }
    }

    public async Task<Poll?> GetPollAsync(Guid id)
    {
        const string sqlPoll = @"
                SELECT Id, Question
                  FROM Poll
                 WHERE Id = @Id;
            ";
        const string sqlOptions = @"
                SELECT Id, PollId, Text, Votes
                  FROM Option
                 WHERE PollId = @Id;
            ";

        var poll = await _db.QuerySingleOrDefaultAsync<Poll>(sqlPoll, new { Id = id });
        if (poll == null) return null;

        var opts = (await _db.QueryAsync<Option>(sqlOptions, new { Id = id }))
                      .ToList();

        poll.Options = opts;
        return poll;
    }

    public async Task VoteAsync(Guid pollId, int optionId)
    {
        const string sql = @"
                UPDATE Option
                   SET Votes = Votes + 1
                 WHERE Id = @OptionId
                   AND PollId = @PollId;
            ";
        await _db.ExecuteAsync(sql, new { PollId = pollId, OptionId = optionId });
    }

    public async Task<IEnumerable<PollSummary>> GetPollSummariesAsync()
    {
        const string sql = "SELECT Id, Question FROM Poll;";
        return await _db.QueryAsync<PollSummary>(sql);
    }
}
