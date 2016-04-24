using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace RuntimeComponents
{
    public sealed class LoadSitiesBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += TaskInstance_Canceled;
            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
            await Task.Delay(1500);
            _deferral.Complete();
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            
        }
    }
}
