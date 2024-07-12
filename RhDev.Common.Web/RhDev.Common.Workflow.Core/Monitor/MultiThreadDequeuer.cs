using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RhDev.Common.Web.Core.DataAccess.SQL.Repository.Entities.Workflow;
using RhDev.Common.Web.Core.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RhDev.Common.Workflow.Monitor
{
    public abstract class MultiThreadDequeuer<V>
    {
        /// <summary>
        /// The thread that process the reading elements
        /// </summary>
        private Thread th = null;

        /// <summary>
        /// The multi-thread queue.
        /// </summary>
        private readonly IWorkflowRunnerQueue sourceQueue = null;

        /// <summary>
        /// TraceLogger
        /// </summary>
        protected ILogger<MultiThreadDequeuer<V>> _logger;

        /// <summary>
        /// Flag to exit from the thread
        /// </summary>
        private bool shutdown = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="queueRunner">The multi-thread queue</param>
        /// <param name="logger">TraceLogger</param>
        protected MultiThreadDequeuer(IWorkflowRunnerQueue queueRunner, ILogger<MultiThreadDequeuer<V>> traceLogger)
        {
            sourceQueue = queueRunner;
            _logger = traceLogger;
        }

        /// <summary>
        /// The thread state.
        /// </summary>
        /// <returns>The thread state enum.</returns>
        public ThreadState ThreadState
        {
            get
            {
                return th.ThreadState;
            }
        }

        /// <summary>
        /// The thread name.
        /// </summary>
        /// <returns>The thread name.</returns>
        public String ThreadName
        {
            get
            {
                return this.th.Name;
            }
        }

        /// <summary>
        /// Start the dequeuer thread.
        /// </summary>
        public virtual void Start()
        {
            if (th == null)
            {
                th = new Thread(this.Run);

                th.Name = "MultiThreadDequeuer_" + th.ManagedThreadId;

                LogEvent("Thread {0} created.", th.Name);
            }

            if (th.ThreadState == ThreadState.Unstarted)
            {
                th.Start();

                LogEvent("Thread {0} started.", th.Name);
            }

            if (shutdown)
            {
                shutdown = false;
            }
        }

        /// <summary>
        /// Thread loop. Wait for a new incoming element and in case the threag get the new element, the mehod OnNewElement is called.
        /// </summary>
        protected virtual void Run()
        {
            while (!shutdown)
            {
                QueueItem item = null;

                try
                {
                    item = this.sourceQueue.BeginDequeue();

                    if (item != null)
                    {
                        OnNewElement(item.Value);
                    }
                }
                catch (ThreadInterruptedException _)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An general error occured when running multithreaddequeuer for item ID : {item?.Value?.Id} : {ex}");

                    FailOnRequest(item.Value);
                }
                finally
                {
                    if (item != null)
                    {
                        sourceQueue.CompleteDequeue(item);
                    }
                }
            }
        }

        public virtual void Shutdown()
        {
            lock (this)
            {
                if (shutdown)
                {
                    return;
                }

                shutdown = true;

                LogEvent("Shutdown flag set on thread {0}.", th.Name);

                th = null;
            }
        }

        private void LogEvent(string message, params object[] parameters)
        {
            message = "MultiThreadDequeuer: " + message;

            _logger.LogInformation(string.Format( message, parameters));
        }

        protected void FailOnRequest(WorkflowInstance workflowInstance)
        {
            Guard.NotNull(workflowInstance, nameof(workflowInstance));

            //TODO process general errored failed instances
        }

        /// <summary>
        /// Override this abstact method in order to define the process to elaborate the received element.
        /// </summary>
        /// <param name="element">The new received element.</param>
        protected abstract void OnNewElement(WorkflowInstance element);
    }
}
