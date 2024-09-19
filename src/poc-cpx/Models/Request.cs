namespace poc_cpx.Models;
public record Request
{
    public bool Stream { get; set; }
    public List<Message> Messages { get; set; } = [];
}