namespace MakeMkv
{
    public class CommentLogLine : LogLine
    {
        public CommentLogLine() : base("#")
        {
        }

        public string Text { get; set; }

        public static CommentLogLine Parse(string line)
        {
            return new CommentLogLine
            {
                Text = line.Substring(1).Trim(),
                OriginalLine = line
            };
        }
    }
}
