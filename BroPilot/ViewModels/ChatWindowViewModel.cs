using BroPilot.Actions;
using BroPilot.Models;
using BroPilot.Services;
using BroPilot.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        private readonly OllamaEndpointService ollamaEndpointService;
        private readonly IContextProvider contextProvider;
        private readonly ToolWindowState toolWindowState;
        private readonly ResourceHelper resourceHelper;
        private readonly AnalyzeCodeActionService analyzeCodeActionService;

        public ModelsViewModel ModelsViewModel => modelsViewModel;
        public SessionsViewModel Sessions => sessionsViewModel;

        public ChatWindowViewModel(SessionsViewModel sessionViewModel, ModelsViewModel modelsViewModel, OpenAIEndpointService openAIEndpointService, OllamaEndpointService ollamaEndpointService, IContextProvider contextProvider, ToolWindowState toolWindowState, ResourceHelper resourceHelper, AnalyzeCodeActionService analyzeCodeActionService)
        {
            this.sessionsViewModel = sessionViewModel;
            this.modelsViewModel = modelsViewModel;
            this.openAIEndpointService = openAIEndpointService;
            this.ollamaEndpointService = ollamaEndpointService;
            this.contextProvider = contextProvider;
            this.toolWindowState = toolWindowState;
            this.resourceHelper = resourceHelper;
            this.analyzeCodeActionService = analyzeCodeActionService;
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
        private ICommand openEditorAction;

        public ICommand OpenEditorActionCommand
        {
            get
            {
                if (openEditorAction == null)
                {
                    openEditorAction = new RelayCommand<string>(OpenEditorActionHandler);
                }

                return openEditorAction;
            }
        }

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

            var t = ModelsViewModel.Model.Type;

            var endpointService = GetEndpointService(t);

            try
            {
                await endpointService.ChatCompletionStream(ModelsViewModel.Model, newMessages, (s) => reply.Content += s);
                reply.IsComplete = true;
            }
            catch (Exception ex)
            {
                reply.Content = "An error occurred: " + ex.Message;
                return;
            }

            var time = DateTime.UtcNow - start;
            reply.Time = $"generated in " + FormatTimespan(time);

            try
            {
                var messages = chatSession.Messages.ToList();
                messages.Add(new MessageViewModel()
                {
                    Role = "user",
                    Content = "Generate a short title for this conversation in less than 5 words. No other text, no punctuation, no quotes, no markdown. /no_think"
                });


                newMessages = await GetMessagePayloadAsync(messages, true);
                var newTitle = await endpointService.ChatCompletion(ModelsViewModel.Model, newMessages);
                chatSession.Title = newTitle.message.content;
                chatSession.TokenCount = newTitle.tokenCount;
            }
            catch { }

            await sessionsViewModel.UpdateSession(chatSession);
        }

        private async void OpenEditorActionHandler(string code)
        {
            var start = DateTime.UtcNow;
            try
            {
                var currentFileName = contextProvider.GetCurrentFileName();
                var endpointService = GetEndpointService(ModelsViewModel.Model.Type);
                var activeDocument = await contextProvider.GetActiveDocument();

                var systemPrompt_AnalyzeCodeAction = resourceHelper.ReadResourceAsString("BroPilot.Resources.SystemPrompt_AnalyzeCodeAction.txt");
                var userPrompt_AnalyzeCodeAction = resourceHelper.ReadResourceAsString("BroPilot.Resources.UserPrompt_AnalyzeCodeAction.txt");
                userPrompt_AnalyzeCodeAction = userPrompt_AnalyzeCodeAction.Replace("{currentFileName}", currentFileName);
                userPrompt_AnalyzeCodeAction = userPrompt_AnalyzeCodeAction.Replace("{activeDocument}", activeDocument);
                userPrompt_AnalyzeCodeAction = userPrompt_AnalyzeCodeAction.Replace("{code}", code);

                var messages = new Message[]
                {
                new Message()
                {
                    role = "system",
                    content = systemPrompt_AnalyzeCodeAction
                },
                new Message()
                {
                    role = "user",
                    content = userPrompt_AnalyzeCodeAction
                }
                };
                var schema_AnalyzeCodeAction = resourceHelper.ReadResourceAsString("BroPilot.Resources.AnalyzeCodeAction_Schema.txt");
                var result = await endpointService.ChatCompletion(ModelsViewModel.Model, messages, schema_AnalyzeCodeAction);

                var analyzeCodeActionContainer = JsonSerializer.Deserialize<ActionContainer>(result.message.content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var results = new List<string>();
                foreach (var action in analyzeCodeActionContainer.Actions)
                {
                    analyzeCodeActionService.ExecuteAction(action);
                    results.Add($"{action.Summary}: {action.Motivation}");
                }

                if (results.Count > 0)
                {
                    Send(string.Join(Environment.NewLine, results));
                }
                else
                {
                    Send($"Sorry. Tried to determine how to change `{currentFileName}` but are unable to. 🤷");
                }
            }
            catch  (Exception ex) {
                Send($"Sorry. Tried to determine how to change the current file but are unable to. Something went wrong: " + ex.Message.ToString());
            }

            await sessionsViewModel.UpdateSession(sessionsViewModel.ChatSession);

            void Send(string content)
            {
                var reply = new MessageViewModel()
                {
                    Role = "assistant",
                    Content = content
                };
                var time = DateTime.UtcNow - start;
                reply.Time = $"generated in " + FormatTimespan(time);


                sessionsViewModel.ChatSession.AddMessage(reply);
            }
        }

        private void NewSessionHandler(object obj)
        {
            sessionsViewModel.CreateNewSession();
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

        private IEndpointService GetEndpointService(Model.ApiType t)
        {
            if (t == Model.ApiType.Ollama)
            {
                return ollamaEndpointService;
            }
            else
            {
                return openAIEndpointService;
            }
        }

        private async Task<Message[]> GetMessagePayloadAsync(IEnumerable<MessageViewModel> messages, bool skipPrompts)
        {
            var result = new List<Message>();

            if (!skipPrompts)
            {
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

                var activeDocument = await contextProvider.GetActiveDocument();
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
    }
}
