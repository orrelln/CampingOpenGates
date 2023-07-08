namespace Scanner.Structures
{
    public class CassieMessage
    {
        public string CassieText { get; set; } = "UNKNOWN";
        public string CaptionText { get; set; } = "UNKNOWN";

        public CassieMessage()
            : this("UNKNOWN", "UNKNOWN")
        {
        }

        public CassieMessage(string cassieText, string captionText)
        {
            CassieText = cassieText;
            CaptionText = captionText;
        }
    }
}