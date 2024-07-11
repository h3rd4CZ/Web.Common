using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Web.Core.Workflow;

namespace RhDev.Common.Web.Core.DataAccess.Workflow
{
    [Serializable]
    public class WorkflowDocumentIdentifier
    {
        public SectionDesignation SectionDesignation { get; set; }
        public WorkflowDocumentIdentificator Identificator { get; set; }
        /// <summary>
        /// Entity ID of remote workflow document representation in WF engine
        /// </summary>
        public int WorkflowDocumentEntityId { get; set; }
        public string DocumentReference { get; set; }

        public void Validate()
        {
            Guard.NotNull(SectionDesignation, nameof(SectionDesignation));
            Guard.NotDefault(Identificator, nameof(Identificator));
        }

        public WorkflowDocumentIdentifier(SectionDesignation sectionDesignation, WorkflowDocumentIdentificator identificator, string documentReference, int workflowDocumentEntityId)
        {
            SectionDesignation = sectionDesignation;
            Identificator = identificator;
            DocumentReference = documentReference;
            WorkflowDocumentEntityId = workflowDocumentEntityId;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            WorkflowDocumentIdentifier documentIdentifier = (WorkflowDocumentIdentifier)obj;

            return
                 Identificator.entityId.Equals(documentIdentifier.Identificator.entityId) &&
                 Identificator.typeName.Equals(documentIdentifier.Identificator.typeName);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int? hashCode = default;
                if (!Equals(null, SectionDesignation)) hashCode = SectionDesignation.GetHashCode();
                hashCode = hashCode * 397 ^ Identificator.entityId.GetHashCode();
                hashCode = hashCode * 397 ^ Identificator.typeName.GetHashCode();
                return hashCode!.Value;
            }
        }

        public override string ToString()
        => $"[Section {SectionDesignation} | Id: {Identificator} | Reference: {DocumentReference}]";
    }
}
