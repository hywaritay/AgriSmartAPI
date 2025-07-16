namespace AgriSmartAPI.Models;

public class ForumPost
{
    public int Id { get; set; }
    public string FarmerId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime PostedDate { get; set; }
    public List<string> Replies { get; set; } = new List<string>();
}

public class ExpertQA
{
    public int Id { get; set; }
    public string FarmerId { get; set; }
    public string Question { get; set; }
    public string Answer { get; set; }
    public string ExpertId { get; set; }
    public DateTime AnsweredDate { get; set; }
}

public class ReplyRequest
{
    public string Reply { get; set; }
}