using FluentMPC.Helpers;
using FluentMPC.Services;
using System.Linq;
using System.Threading.Tasks;

using Windows.ApplicationModel.Background;

namespace FluentMPC.BackgroundTasks
{
    public sealed class LiveTileUpdateTask : BackgroundTask
    {

        private volatile bool _cancelRequested = false;
        private BackgroundTaskDeferral _deferral;

        public override void Register()
        {
            var taskName = GetType().Name;
            var taskRegistration = BackgroundTaskRegistration.AllTasks.FirstOrDefault(t => t.Value.Name == taskName).Value;

            if (taskRegistration == null)
            {
                var builder = new BackgroundTaskBuilder()
                {
                    Name = taskName
                };

                builder.SetTrigger(new TimeTrigger(15, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.UserPresent));

                builder.Register();
            }
        }

        public override Task RunAsyncInternal(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null)
            {
                return null;
            }

            _deferral = taskInstance.GetDeferral();

            return Task.Run(async () =>
            {
                //// Documentation:
                ////      * General: https://docs.microsoft.com/windows/uwp/launch-resume/support-your-app-with-background-tasks
                ////      * Debug: https://docs.microsoft.com/windows/uwp/launch-resume/debug-a-background-task
                ////      * Monitoring: https://docs.microsoft.com/windows/uwp/launch-resume/monitor-background-task-progress-and-completion

                //// To show the background progress and message on any page in the application,
                //// subscribe to the Progress and Completed events.
                //// You can do this via "BackgroundTaskService.GetBackgroundTasksRegistration"
                try
                {
                    if (_cancelRequested) return;

                    var currentSong = await MPDConnectionService.GetCurrentSong();
                    if (currentSong == null) return;

                    Singleton<LiveTileService>.Instance.UpdatePlayingSong(new ViewModels.Items.TrackViewModel(currentSong));
                }
                catch
                {
                    // Whatever, this background task isn't very useful tbh
                }
                finally
                {
                    _deferral?.Complete();
                }
            });
        }

        public override void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _cancelRequested = true;
        }
    }
}
