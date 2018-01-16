using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;

namespace Hasin.Taaghche.TaskScheduler.Core
{
    public class LogEverythingAttribute : JobFilterAttribute, IApplyStateFilter
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public void OnCreating(CreatingContext context)
        {
            Logger.InfoFormat($"Creating a job based on method `{context.Job?.Method?.Name}`...");
        }

        public void OnCreated(CreatedContext context)
        {
            Logger.InfoFormat($"Job that is based on method `{context.Job?.Method?.Name}` has been created with id `{context.BackgroundJob?.Id}`");
        }

        public void OnPerforming(PerformingContext context)
        {
            Logger.InfoFormat($"Starting to perform job `{context.BackgroundJob?.Id}`");
        }

        public void OnPerformed(PerformedContext context)
        {
            Logger.InfoFormat($"Job `{context.BackgroundJob?.Id}` has been performed");
        }

        public void OnStateElection(ElectStateContext context)
        {
            var failedState = context.CandidateState as FailedState;
            if (failedState != null)
            {
                Logger.WarnFormat($"Job `{context.BackgroundJob?.Id}` has been failed due to an exception `{failedState.Exception?.Message}`");
            }
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            Logger.InfoFormat($"Job `{context.BackgroundJob?.Id}` state was changed from `{context.OldStateName}` to `{context.NewState?.Name}`");
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            Logger.InfoFormat($"Job `{context.BackgroundJob?.Id}` state `{context.OldStateName}` was unapplied.");
        }
    }
}
