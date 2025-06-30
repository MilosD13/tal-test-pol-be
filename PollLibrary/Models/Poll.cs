namespace PollLibrary.Models;

public class Poll
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<Option> Options { get; set; } = new();
}
