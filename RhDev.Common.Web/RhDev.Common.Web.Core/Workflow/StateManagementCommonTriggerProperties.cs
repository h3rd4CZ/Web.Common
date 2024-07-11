using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Workflow
{
    public class StateManagementCommonTriggerProperties
    {
        public const string TRIGGERPARAMETERDELIMITER = ";;";

        public string UserRespondedId { get; set; }
        public Guid TransitionId { get; set; }
        public List<WorkflowTriggerParameter> TriggerParameters { get; set; } = new List<WorkflowTriggerParameter> { };
        public string Trigger { get; set; }

        [JsonIgnore]
        public bool ValidUserTransitionData => !string.IsNullOrWhiteSpace(UserRespondedId) && !string.IsNullOrWhiteSpace(Trigger);
        [JsonIgnore]
        public bool ValidSystemTransitionData => !string.IsNullOrWhiteSpace(Trigger);
        public StateManagementCommonTriggerProperties()
        {

        }

        public StateManagementCommonTriggerProperties(string userRespondedId, Guid transitionId, IList<WorkflowTriggerParameter> triggerParameters, string trigger)
        {
            UserRespondedId = userRespondedId;
            TransitionId = transitionId;
            TriggerParameters = triggerParameters?.ToList();
            Trigger = trigger;
        }

        public static StateManagementCommonTriggerProperties Empty => new StateManagementCommonTriggerProperties(string.Empty, Guid.NewGuid(), new List<WorkflowTriggerParameter> { }, string.Empty);
        public static StateManagementCommonTriggerProperties System(string trigger) => new StateManagementCommonTriggerProperties(SystemUserNames.System.ToString(), Guid.NewGuid(), new List<WorkflowTriggerParameter> { }, trigger);

        public static StateManagementCommonTriggerProperties EmptyDataWithUserResponded(string usrId, string trigger) =>
            new StateManagementCommonTriggerProperties(usrId, Guid.NewGuid(), new List<WorkflowTriggerParameter> { }, trigger);

        public static StateManagementCommonTriggerProperties DataWithUserResponded(string usrId, IList<WorkflowTriggerParameter> triggerParameters, string trigger) =>
            new StateManagementCommonTriggerProperties(usrId, Guid.NewGuid(), triggerParameters, trigger);

        public override string ToString()
        {
            var parameters = Equals(null, TriggerParameters) ? string.Empty : string.Join(",", TriggerParameters.Select(p => $"{p.Name} = {p.Value}"));

            return $"User responded : {UserRespondedId}, trigger : {Trigger}, params : {parameters}";
        }
    }


}
