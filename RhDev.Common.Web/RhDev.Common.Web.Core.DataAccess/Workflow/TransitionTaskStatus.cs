using System;
using System.Collections.Generic;
using System.Text;

namespace RhDev.Common.Web.Core.DataAccess.Workflow
{
    public enum TransitionTaskStatus
    {
        Unknown = 0,
        /// <summary>
        /// Action was created
        /// </summary>
        Planned = 1 << 1,

        /// <summary>
        /// Action was completed
        /// </summary>
        Completed = 1 << 2,

        /// <summary>
        /// Executing action failed
        /// </summary>
        Failed = 1 << 3,

        /// <summary>
        /// Action is in progress
        /// </summary>
        InProgress = 1 << 4
    }
}
