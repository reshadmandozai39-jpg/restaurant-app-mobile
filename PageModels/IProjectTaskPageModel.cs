using CommunityToolkit.Mvvm.Input;
using NET_MAUI_App.Models;

namespace NET_MAUI_App.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}