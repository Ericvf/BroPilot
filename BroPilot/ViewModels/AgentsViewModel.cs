using BroPilot.Models;
using System.Collections.ObjectModel;

namespace BroPilot.ViewModels
{
    public class AgentsViewModel : BaseViewModel
    {
        private ObservableCollection<Agent> agents;
        public ObservableCollection<Agent> Agents
        {
            get => agents;
            set
            {
                if (agents != value)
                {
                    agents = value;
                    OnPropertyChanged(nameof(Agents));
                }
            }
        }

        private Agent agent;
        public Agent Agent
        {
            get => agent;
            set
            {
                if (agent != value)
                {
                    agent = value;
                    OnPropertyChanged(nameof(Agent));
                }
            }
        }

        public AgentsViewModel()
        {
            agents = new ObservableCollection<Agent>
            {
                new Agent { Address = "http://localhost:1234", Model = "qwen/qwen3-8b", Name = "qwen/qwen3-8b", Temperature = 0, MaxTokens = 8000 },
                new Agent { Address = "http://localhost:1234", Model = "qwen3-14b", Name = "qwen3-14b", Temperature = 0, MaxTokens = 8000 },
                new Agent { Address = "http://localhost:1234", Model = "meta-llama-3-8b-instruct", Name = "meta-llama-3-8b-instruct", Temperature = 0 },
                new Agent { Address = "http://localhost:1234", Model = "openai/gpt-oss-20b", Name = "openai/gpt-oss-20b", Temperature = 0 },
                new Agent { Address = "http://localhost:1234", Model = "qwen/qwen2.5-coder-14b", Name = "qwen/qwen2.5-coder-14b", Temperature = 0 }
            };
            agent = agents[0];
        }
    }
}
