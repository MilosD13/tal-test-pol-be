namespace PollLibrary.Models;

public class Option
{
    public int Id { get; set; }
    public Guid PollId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Votes { get; set; }
}
