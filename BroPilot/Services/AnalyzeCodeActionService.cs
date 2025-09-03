using BroPilot.Actions;
using System;

namespace BroPilot.Services
{
    public class AnalyzeCodeActionService
    {
        private readonly IContextProvider contextProvider;
        public AnalyzeCodeActionService(IContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;
        }

        public void ExecuteAction(BaseAction action)
        {
            switch (action)
            {
                case AddMethodAction addMethod:
                    contextProvider.AddMethod(addMethod.File, addMethod.Classname, addMethod.Code);
                    break;

                case AddClassAction addClass:
                    contextProvider.AddClass(addClass.File, addClass.Classname, addClass.Code);
                    break;

                case ReplaceMethodAction replaceMethod:
                    contextProvider.ReplaceMethod(replaceMethod.File, replaceMethod.Classname, replaceMethod.Method, replaceMethod.Code);
                    break;

                case ReplaceClassAction replaceClass:
                    contextProvider.ReplaceClass(replaceClass.File, replaceClass.Classname, replaceClass.Code);
                    break;

                default:
                    throw new InvalidOperationException("Unknown action type: " + action.GetType().Name);
            }
        }
    }
}
