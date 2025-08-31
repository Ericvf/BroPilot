using BroPilot.Models;
using BroPilot.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace BroPilot.ViewModels
{
    public class SessionsViewModel : BaseViewModel
    {
        private readonly ISessionManager sessionManager;
        private readonly ToolWindowState toolWindowState;
        private readonly ObservableCollection<ChatSessionViewModel> sessions = new ObservableCollection<ChatSessionViewModel>()
        {
        };


        public ICollectionView SessionsView { get; }

        public ObservableCollection<ChatSessionViewModel> Sessions => sessions;

        public SessionsViewModel(ISessionManager sessionManager, ToolWindowState toolWindowState)
        {
            this.sessionManager = sessionManager;
            this.toolWindowState = toolWindowState;


            SessionsView = CollectionViewSource.GetDefaultView(sessions);
            SessionsView.SortDescriptions.Add(new SortDescription(nameof(ChatSessionViewModel.Date), ListSortDirection.Descending));
        }

        public SessionsViewModel()
        {
            
        }

        public ChatSessionViewModel CreateNewSession()
        {
            var chatSession = new ChatSessionViewModel()
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.UtcNow,
                 Title = "New chat"
            };

            sessions.Add(chatSession);
            ChatSession = chatSession;
            return chatSession;
        }

        public void LoadSessions()
        {
            var sessions = sessionManager.LoadSessions();
            var sessionViewModels = sessions.Select(s => MapSessionToViewModel(s));

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var s in sessionViewModels)
                {
                    this.sessions.Add(s);
                }

            });
            
            ChatSession = this.sessions.OrderByDescending(s => s.Date).FirstOrDefault();
        }

        public async Task UpdateSession(ChatSessionViewModel chatSession)
        {
            var session = MapViewModelToSession(chatSession);
            await sessionManager.UpdateSession(session);
        }

        private Session MapViewModelToSession(ChatSessionViewModel chatSession)
        {
            return new Session()
            {
                Id = chatSession.Id,
                Title = chatSession.Title,
                Date = chatSession.Date,
                TokenCount = chatSession.TokenCount,
                Messages = chatSession.Messages.Select(m => new Message()
                {
                    content = m.Content,
                    role = m.Role
                }).ToArray()
            };
        }

        private ChatSessionViewModel MapSessionToViewModel(Session session)
        {
            var result = new ChatSessionViewModel()
            {
                Id = session.Id,
                Title = session.Title,
                Date = session.Date,
                TokenCount = session.TokenCount
            };

            if (session.Messages != null)
            {
                foreach (var m in session.Messages)
                {
                    result.AddMessage(new MessageViewModel()
                    {
                        Content = m.content,
                        Role = m.role,
                        IsComplete = true
                    });
                }
            }

            return result;
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

        #region Commands
        private ICommand openSessionCommand;

        public ICommand OpenSessionCommand
        {
            get
            {
                if (openSessionCommand == null)
                {
                    openSessionCommand = new RelayCommand<ChatSessionViewModel>(OpenSessionHandler);
                }

                return openSessionCommand;
            }
        }

        private void OpenSessionHandler(ChatSessionViewModel obj)
        {
            ChatSession = obj;
            toolWindowState.ShowChatWindow();
        }


        private ICommand newSessionCommand;

        public ICommand NewSessionCommand
        {
            get
            {
                if (newSessionCommand == null)
                {
                    newSessionCommand = new RelayCommand<ChatSessionViewModel>(NewSessionHandler);
                }

                return newSessionCommand;
            }
        }

        private void NewSessionHandler(ChatSessionViewModel obj)
        {
            CreateNewSession();
            toolWindowState.ShowChatWindow();
        }


        private ICommand closeSessionCommand;

        public ICommand CloseSessionCommand
        {
            get
            {
                if (closeSessionCommand == null)
                {
                    closeSessionCommand = new RelayCommand<ChatSessionViewModel>(CloseSessionHandler);
                }

                return closeSessionCommand;
            }
        }

        private void CloseSessionHandler(ChatSessionViewModel obj)
        {
            sessions.Remove(obj);
            sessionManager.DeleteSession(new Session { Id = obj.Id });
        }

        #endregion
    }
}
