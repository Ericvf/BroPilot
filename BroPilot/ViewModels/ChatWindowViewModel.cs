using BroPilot.Models;
using BroPilot.Services;
using BroPilot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BroPilot.ViewModels
{
    public class ChatWindowViewModel : BaseViewModel
    {
        private readonly ModelsViewModel modelsViewModel;
        private readonly SessionsViewModel sessionViewModel;
        private readonly OpenAIEndpointService openAIEndpointService;
        private readonly IContextProvider contextProvider;

        public ModelsViewModel ModelsViewModel => modelsViewModel;

        public ChatWindowViewModel(SessionsViewModel sessionViewModel, ModelsViewModel modelsViewModel, OpenAIEndpointService openAIEndpointService, IContextProvider contextProvider)
        {
            this.sessionViewModel = sessionViewModel;
            this.modelsViewModel = modelsViewModel;
            this.openAIEndpointService = openAIEndpointService;
            this.contextProvider = contextProvider;
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

        private ICommand configureCommand;

        public ICommand ConfigureCommand
        {
            get
            {
                if (configureCommand == null)
                {
                    configureCommand = new RelayCommand(ConfigureHandler);
                }

                return configureCommand;
            }
        }

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

        private void ConfigureHandler(object obj)
        {
            var newWindow = new Window
            {
                Title = "BroPilot - Configure models",
            };

            newWindow.Content = new ModelsWindow(modelsViewModel);
            newWindow.Height = 600;
            newWindow.Width = 700;
            newWindow.ShowDialog();
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

            var newMessages = await GetMessagePayloadAsync(ChatSession.Messages, false);
            var x = await openAIEndpointService.ChatCompletionStream(ModelsViewModel.Model, newMessages, (s) => message.Content += s);
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

                newMessages = await GetMessagePayloadAsync(messages, true);
                var newTitle = await openAIEndpointService.ChatCompletion(ModelsViewModel.Model, newMessages);
                ChatSession.Title = newTitle.message.content;
                ChatSession.TokenCount = newTitle.tokenCount;
            }
        }

        private async Task<Message[]> GetMessagePayloadAsync(IEnumerable<MessageViewModel> messages, bool skipPrompts)
        {
            var result = new List<Message>();

            if (!skipPrompts)
            {
                var activeDocument = await contextProvider.GetActiveDocument();
                result.Add(new Message
                {
                    role = "system",
                    content = "You are a coding assistant called BroPilot running on a PC with limited resources. Keep your responses and token use as short as possible. /no_think"
                });
                
                result.Add(new Message
                {
                    role = "system",
                    content = "This is the active document. Do not mention it unless asked. When the user speaks about code, they usually mean the active document: " + Environment.NewLine + activeDocument
                });
            }

            foreach (var message in messages)
            {
                result.Add(new Message
                {
                    role = message.Role,
                    content = message.Content
                });
            }

            return result.ToArray();
        }
    }
}
