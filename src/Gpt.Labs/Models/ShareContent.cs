using System.Collections.Generic;

namespace Gpt.Labs.Models
{
    public class ShareContent
    {
        public string Title { get; set; }

        public string Message { get; set; }

        public List<string> Files { get; set; } = new List<string>();
    }
}
