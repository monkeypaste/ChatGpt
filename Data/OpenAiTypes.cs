using System.Collections.Generic;

namespace ChatGpt.Data {
    public class ChatGptRequest {
        public string model { get; set; }
        public List<OpenAiMessage> messages { get; set; }
        public double temperature { get; set; }
    }
    public class ChatGptResponse {
        public string id { get; set; }
        public string @object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public string system_fingerprint { get; set; }
        public List<ChatGptChoice> choices { get; set; }
        public OpenAiUsage usage { get; set; }
    }
    public class ChatGptChoice {
        public int index { get; set; }
        public OpenAiMessage message { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
    }

    public class OpenAiMessage {
        public string role { get; set; }
        public string content { get; set; }
    }


    public class OpenAiUsage {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }


}
