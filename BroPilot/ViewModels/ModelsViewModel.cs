using BroPilot.Models;
using BroPilot.Wpf;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BroPilot.ViewModels
{
    public class ModelsViewModel : BaseViewModel
    {
        private ObservableCollection<Model> models;
        public ObservableCollection<Model> Models
        {
            get => models;
            set
            {
                if (models != value)
                {
                    models = value;
                    OnPropertyChanged(nameof(Models));
                }
            }
        }

        private Model model;
        public Model Model
        {
            get => model;
            set
            {
                if (model != value)
                {
                    model = value;
                    OnPropertyChanged(nameof(Model));
                }
            }
        }

        public ModelsViewModel()
        {
            models = new ObservableCollection<Model>
            {
                new Model { Address = "http://localhost:1234", ModelName = "qwen/qwen3-8b", Name = "qwen/qwen3-8b", Temperature = 0, MaxTokens = 8000 },
                new Model { Address = "http://localhost:1234", ModelName = "qwen3-14b", Name = "qwen3-14b", Temperature = 0, MaxTokens = 8000 },
                new Model { Address = "http://localhost:1234", ModelName = "meta-llama-3-8b-instruct", Name = "meta-llama-3-8b-instruct", Temperature = 0 },
                new Model { Address = "http://localhost:1234", ModelName = "openai/gpt-oss-20b", Name = "openai/gpt-oss-20b", Temperature = 0 },
                new Model { Address = "http://localhost:1234", ModelName = "qwen/qwen2.5-coder-14b", Name = "qwen/qwen2.5-coder-14b", Temperature = 0 }
            };
            model = models[0];
        }

        #region Commands

        private ICommand addModelCommand;

        public ICommand AddModelCommand
        {
            get
            {
                if (addModelCommand == null)
                {
                    addModelCommand = new RelayCommand(AddModelHandler);
                }

                return addModelCommand;
            }
        }


        private ICommand deleteModelCommand;

        public ICommand DeleteModelCommand
        {
            get
            {
                if (deleteModelCommand == null)
                {
                    deleteModelCommand = new RelayCommand<Model>(DeleteModelHandler);
                }

                return deleteModelCommand;
            }
        }


        #endregion

        private void AddModelHandler(object obj)
        {
            this.models.Insert(0, new Model { Address = "http://localhost:1234", ModelName = "Local model", Name = "Lodel model", Temperature = 0 });
        }

        private void DeleteModelHandler(Model agent)
        {
            this.models.Remove(agent);
        }
    }
}
