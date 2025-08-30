namespace BroPilot.ViewModels
{
    public class SessionsViewModel : BaseViewModel
    {
    
        public ChatSessionViewModel CreateNewSession()
        {
            var chatSession = new ChatSessionViewModel()
            {
            };

            return chatSession;
        }
    }
}
