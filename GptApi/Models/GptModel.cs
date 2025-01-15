namespace GptApi.Models
{
    public class BeginRequest
    {
        public string? Question { get; set; }

        public string? DesKey { get; set; }

        public string? DesIV { get; set; }
    }

    public class GetMessageRequest
    {
        public string key { get; set; }
    }

    public class GptResult
    {
        public bool IsComplete { get; set; }
        public string? Body { get; set; }
    }
}
