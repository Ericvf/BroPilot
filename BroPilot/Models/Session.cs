using BroPilot.Models;
using System;

namespace BroPilot.ViewModels
{
    public class Session
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public Message[] Messages { get; set; }

        public DateTime? Date { get; set; }

        public int TokenCount { get; set; }
    }
}
