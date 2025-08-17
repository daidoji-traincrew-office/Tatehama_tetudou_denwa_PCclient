namespace Tatehama_tetudou_denwa_PCclient.Models;

public class CallListItem
{
    public string DisplayName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public override string ToString()
    {
        return DisplayName;
    }
}
