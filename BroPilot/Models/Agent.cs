namespace BroPilot.Models
{
    public class Agent
    {
        public string Name { get; set; }

        public string Model { get; set; }

        public int Temperature { get; set; }

        public string Address { get; set; }

        public int MaxTokens { get; set; }
    }
}
