﻿using System;
namespace Xigadee
{
    /// <summary>
    /// This is the base abstract class used to implement different communication technologies.
    /// </summary>
    public abstract class CommunicationBridgeAgent: ICommunicationAgent
    {
        /// <summary>
        /// This event is called if there is an exception during transmission.
        /// </summary>
        public event EventHandler<CommunicationAgentEventArgs> OnException;
        /// <summary>
        /// This event is fired before a cloned payload is sent to a remote listener
        /// </summary>
        public event EventHandler<CommunicationAgentEventArgs> OnTransmit;
        /// <summary>
        /// This event is fired when a message is received from a sender and before it is resolved.
        /// </summary>
        public event EventHandler<CommunicationAgentEventArgs> OnReceive;

        /// <summary>
        /// This is the default constructor. 
        /// </summary>
        /// <param name="mode">The operational mode.</param>
        protected CommunicationBridgeAgent(FabricMode mode = FabricMode.NotSet)
        {
            Mode = mode;
        }
        /// <summary>
        /// This method is used to fire the event when an exception occurs.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="ex">The exception raised.</param>
        protected void OnExceptionInvoke(object sender, TransmissionPayload payload, Exception ex)
        {
            var e = new CommunicationAgentEventArgs(payload, ex);
            OnException?.Invoke(sender, e);
        }
        /// <summary>
        /// This event is thrown when a payload is transmitted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="payload">The payload.</param>
        protected void OnTransmitInvoke(object sender, TransmissionPayload payload)
        {
            var e = new CommunicationAgentEventArgs(payload);
            OnTransmit?.Invoke(sender, e);
        }
        /// <summary>
        /// This event is raised when a payload is received by a listener.
        /// </summary>
        /// <param name="sender">The listener.</param>
        /// <param name="payload">The payload.</param>
        protected void OnReceiveInvoke(object sender, TransmissionPayload payload)
        {
            var e = new CommunicationAgentEventArgs(payload);
            OnReceive?.Invoke(sender, e);
        }

        /// <summary>
        /// This is the communication bridge mode.
        /// </summary>
        public FabricMode Mode { get; }

        /// <summary>
        /// This method returns a new listener.
        /// </summary>
        /// <returns>The listener.</returns>
        public abstract IListener GetListener();

        /// <summary>
        /// This method returns a new sender.
        /// </summary>
        /// <returns>The sender.</returns>
        public abstract ISender GetSender();

        /// <summary>
        /// A boolean property indicating that all transmitted payloads have been successfully signalled.
        /// </summary>
        public virtual bool PayloadsAllSignalled { get { throw new NotSupportedException(); } }

        /// <summary>
        /// Gets a value indicating whether payload history is enabled.
        /// </summary>

        public virtual bool PayloadHistoryEnabled { get; protected set; } 

        /// <summary>
        /// This class holds a payload while it is being transmitted.
        /// </summary>
        protected class TransmissionPayloadHolder
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TransmissionPayloadHolder"/> class.
            /// </summary>
            /// <param name="payload">The payload.</param>
            /// <param name="listener">The listener.</param>
            /// <param name="retryCount">This is the permitted number of failure retries.</param>
            public TransmissionPayloadHolder(TransmissionPayload payload, IListener listener, int? retryCount = null)
            {
                Payload = payload;
                Listener = listener;
                RetryCount = retryCount;
            }

            /// <summary>
            /// This is the permitted number of retries for the payload if the message is signalled as failed.
            /// </summary>
            public int? RetryCount { get; }
            /// <summary>
            /// Gets the payload.
            /// </summary>
            public TransmissionPayload Payload { get; }
            /// <summary>
            /// Gets the listener.
            /// </summary>
            public IListener Listener { get; }
            /// <summary>
            /// Gets the payload identifier.
            /// </summary>
            public Guid Id { get { return Payload.Id; } }
        }
    }
}