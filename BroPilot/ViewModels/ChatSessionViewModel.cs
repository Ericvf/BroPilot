using System;
using System.Collections.ObjectModel;

namespace BroPilot.ViewModels
{
    public class ChatSessionViewModel : BaseViewModel
    {
        public string Id { get; set; }
        public DateTime? Date { get; set; }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        private ObservableCollection<MessageViewModel> messages = new ObservableCollection<MessageViewModel>();
        public ObservableCollection<MessageViewModel> Messages => messages;

        public void AddMessage(MessageViewModel message)
        {
            messages.Add(message);
        }

        private int tokenCount;
        public int TokenCount
        {
            get { return tokenCount; }
            set
            {
                if (tokenCount != value)
                {
                    tokenCount = value;
                    OnPropertyChanged(nameof(TokenCount));
                }
            }
        }

    }
}
