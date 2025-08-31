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
        private readonly SessionsViewModel sessionsViewModel;
        private readonly OpenAIEndpointService openAIEndpointService;
        private readonly IContextProvider contextProvider;
        private readonly ToolWindowState toolWindowState;

        public ModelsViewModel ModelsViewModel => modelsViewModel;
        public SessionsViewModel Sessions => sessionsViewModel;

        public ChatWindowViewModel(SessionsViewModel sessionViewModel, ModelsViewModel modelsViewModel, OpenAIEndpointService openAIEndpointService, IContextProvider contextProvider, ToolWindowState toolWindowState)
        {
            this.sessionsViewModel = sessionViewModel;
            this.modelsViewModel = modelsViewModel;
            this.openAIEndpointService = openAIEndpointService;
            this.contextProvider = contextProvider;
            this.toolWindowState = toolWindowState;
            //NewSessionHandler(null);
        }

        //private ChatSessionViewModel chatSession;
        //public ChatSessionViewModel ChatSession
        //{
        //    get { return chatSession; }
        //    set
        //    {
        //        if (chatSession != value)
        //        {
        //            chatSession = value;
        //            OnPropertyChanged(nameof(ChatSession));
        //        }
        //    }
        //}

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

        private ICommand openSessionsCommand;

        public ICommand OpenSessionsCommand
        {
            get
            {
                if (openSessionsCommand == null)
                {
                    openSessionsCommand = new RelayCommand(OpenSessionsHandler);
                }

                return openSessionsCommand;
            }
        }


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

        private void OpenSessionsHandler(object obj)
        {
            toolWindowState.ShowSessionsWindow();
        }

        private void NewSessionHandler(object obj)
        {
            sessionsViewModel.CreateNewSession();
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
            var chatSession = sessionsViewModel.ChatSession;

            var message = new MessageViewModel()
            {
                Role = "user",
                Content = Prompt
            };

            chatSession.AddMessage(message);
            Prompt = string.Empty;

            var start = DateTime.UtcNow;

            var reply = new MessageViewModel()
            {
                Role = "assistant",
            };

            chatSession.AddMessage(reply);

            var newMessages = await GetMessagePayloadAsync(chatSession.Messages, false);

            try
            {
                await openAIEndpointService.ChatCompletionStream(ModelsViewModel.Model, newMessages, (s) => reply.Content += s);
            reply.IsComplete = true;
            }
            catch (Exception ex)
            {
                reply.Content = "An error occurred: " + ex.Message;
            }

            var time = DateTime.UtcNow - start;
            reply.Time = $"generated in " + FormatTimespan(time);

            var messages = chatSession.Messages.ToList();
            messages.Add(new MessageViewModel()
            {
                Role = "user",
                Content = "Generate a short title for this conversation in less than 5 words. No other text, no punctuation, no quotes, no markdown. /no_think"
            });

            newMessages = await GetMessagePayloadAsync(messages, true);
            var newTitle = await openAIEndpointService.ChatCompletion(ModelsViewModel.Model, newMessages);
            chatSession.Title = newTitle.message.content;
            chatSession.TokenCount = newTitle.tokenCount;

            await sessionsViewModel.UpdateSession(chatSession);
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

                var activeMethod = await contextProvider.GetCurrentMethod();
                result.Add(new Message
                {
                    role = "system",
                    content = "This is the active method. Do not mention it unless asked. When the user speaks about code, they usually mean the active method:" + activeMethod
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
