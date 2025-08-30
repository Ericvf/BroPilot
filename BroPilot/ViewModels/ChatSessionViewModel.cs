using System.Collections.ObjectModel;

namespace BroPilot.ViewModels
{
    public class ChatSessionViewModel : BaseViewModel
    {

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
