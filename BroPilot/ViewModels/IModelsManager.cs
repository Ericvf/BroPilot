using BroPilot.Models;

namespace BroPilot.ViewModels
{
    public interface IModelsManager
    {
        Model[] LoadModels();


        void SaveModels(Model[] models);
    }
}
