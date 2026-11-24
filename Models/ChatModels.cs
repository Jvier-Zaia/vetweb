namespace VetWeb.Models
{
    public class ChatMessageDto
    {
        public string Role { get; set; } = "user"; // "user" or "model"
        public string Text { get; set; } = string.Empty;
    }

    public class ChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatMessageDto>? History { get; set; }
    }

    public class ChatResponseDto
    {
        public bool Success { get; set; } = true;
        public string Reply { get; set; } = string.Empty;
        public string? Error { get; set; }
    }
}
