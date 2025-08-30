using BroPilot.Models;
using BroPilot.Services;
using BroPilot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace BroPilot.ViewModels
{
    public class ChatWindowViewModel : BaseViewModel
    {
        private readonly AgentsViewModel agentsViewModel;
        private readonly SessionsViewModel sessionViewModel;
        private readonly OpenAIEndpointService openAIEndpointService;

        public ChatWindowViewModel(SessionsViewModel sessionViewModel, AgentsViewModel agentsViewModel, OpenAIEndpointService openAIEndpointService)
        {
            this.sessionViewModel = sessionViewModel;
            this.agentsViewModel = agentsViewModel;
            this.openAIEndpointService = openAIEndpointService;
            NewSessionHandler(null);
        }

        private ChatSessionViewModel chatSession;
        public ChatSessionViewModel ChatSession
        {
            get { return chatSession; }
            set
            {
                if (chatSession != value)
                {
                    chatSession = value;
                    OnPropertyChanged(nameof(ChatSession));
                }
            }
        }

        private string prompt = string.Empty;
        public string Prompt
        {
            get { return prompt; }
            set
            {
                if (prompt != value)
                {
                    prompt = value;
                    OnPropertyChanged(nameof(Prompt));
                }
            }
        }

        #region Commands
        private ICommand newSessionCommand;

        public ICommand NewSessionCommand
        {
            get
            {
                if (newSessionCommand == null)
                {
                    newSessionCommand = new RelayCommand(NewSessionHandler);
                }

                return newSessionCommand;
            }
        }

        private ICommand submitCommand;

        public ICommand SubmitCommand
        {
            get
            {
                if (submitCommand == null)
                {
                    submitCommand = new RelayCommand(SubmitHandler);
                }

                return submitCommand;
            }
        }

        public AgentsViewModel AgentsViewModel => agentsViewModel;

        #endregion

        private void NewSessionHandler(object obj)
        {
            var chatSession = sessionViewModel.CreateNewSession();
            ChatSession = chatSession;
        }

        private static string FormatTimespan(TimeSpan input)
        {
            var seconds = input.TotalSeconds;

            if (seconds < 60)
            {
                string formatted = seconds % 1 == 0
                    ? $"{(int)seconds} second{(seconds != 1 ? "s" : "")}"
                    : $"{seconds:0.0} seconds";

                return formatted;
            }
            else if (seconds < 3600)
            {
                int mins = (int)(seconds / 60);
                int sec = (int)(seconds % 60);
                return $"{mins} minute{(mins != 1 ? "s" : "")} and {sec} second{(sec != 1 ? "s" : "")}";
            }
            else
            {
                int hrs = (int)(seconds / 3600);
                int secs = (int)(seconds % 3600);
                return $"{hrs} hour{(hrs != 1 ? "s" : "")} and {secs} second{(secs != 1 ? "s" : "")}";
            }
        }

        private async void SubmitHandler(object obj)
        {
            var message = new MessageViewModel()
            {
                Role = "user",
                Content = Prompt
            };

            ChatSession.AddMessage(message);
            Prompt = string.Empty;

            var start = DateTime.UtcNow;

            message = new MessageViewModel()
            {
                Role = "assistant",
            };

            ChatSession.AddMessage(message);

           var x = await openAIEndpointService.ChatCompletionStream(AgentsViewModel.Agent, GetMessagePayload(ChatSession.Messages).ToArray(), (s) => message.Content += s);
            message.IsComplete = true;

            var time = DateTime.UtcNow - start;
            message.Time = $"generated in " + FormatTimespan(time);

           // if (ChatSession.Title == null || ChatSession.Messages.Count > 6)
            {
                var messages = ChatSession.Messages.ToList();
                messages.Add(new MessageViewModel()
                {
                    Role = "user",
                    Content = "Generate a short title for this conversation in less than 5 words. No other text, no punctuation, no quotes, no markdown. /no_think"
                });

                var newTitle = await openAIEndpointService.ChatCompletion(AgentsViewModel.Agent, GetMessagePayload(messages).ToArray());
                ChatSession.Title = newTitle.message.content;
                ChatSession.TokenCount = newTitle.tokenCount;
            }
        }

        private IEnumerable<Message> GetMessagePayload(IEnumerable<MessageViewModel> messages)
        {
            yield return new Message()
            {
                role = "system",
                content = "You are a coding assistant called BroPilot running on a PC with limited resources. Keep your responses and token use as short as possible. /no_think"
            };

            foreach (var message in messages)
            {
                yield return new Message()
                {
                    role = message.Role,
                    content = message.Content
                };
            }
        }
    }
}
