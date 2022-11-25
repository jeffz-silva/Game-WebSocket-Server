using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Components.Events
{
    public class GameEventHandlerCollection
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GameEventHandlerCollection));
        protected readonly HybridDictionary m_events = new HybridDictionary();
        protected readonly ReaderWriterLock m_lock = new ReaderWriterLock();
        protected const int TIMEOUT = 3000;

        public void AddHandler(ServerEvent e, ServerEventHandler del)
        {
            try
            {
                this.m_lock.AcquireWriterLock(TIMEOUT);
                try
                {
                    WeakMulticastDelegate weakDelegate = (WeakMulticastDelegate)this.m_events[(object)e];
                    if (weakDelegate == null)
                        this.m_events[(object)e] = (object)new WeakMulticastDelegate((Delegate)del);
                    else
                        this.m_events[(object)e] = (object)WeakMulticastDelegate.Combine(weakDelegate, (Delegate)del);
                }
                finally
                {
                    this.m_lock.ReleaseWriterLock();
                }
            }
            catch (ApplicationException ex)
            {
                if (!GameEventHandlerCollection.log.IsErrorEnabled)
                    return;
                GameEventHandlerCollection.log.Error((object)"Failed to add event handler!", (Exception)ex);
            }
        }

        public void AddHandlerUnique(ServerEvent e, ServerEventHandler del)
        {
            try
            {
                this.m_lock.AcquireWriterLock(TIMEOUT);
                try
                {
                    WeakMulticastDelegate weakDelegate = (WeakMulticastDelegate)this.m_events[(object)e];
                    if (weakDelegate == null)
                        this.m_events[(object)e] = (object)new WeakMulticastDelegate((Delegate)del);
                    else
                        this.m_events[(object)e] = (object)WeakMulticastDelegate.CombineUnique(weakDelegate, (Delegate)del);
                }
                finally
                {
                    this.m_lock.ReleaseWriterLock();
                }
            }
            catch (ApplicationException ex)
            {
                if (!GameEventHandlerCollection.log.IsErrorEnabled)
                    return;
                GameEventHandlerCollection.log.Error((object)"Failed to add event handler!", (Exception)ex);
            }
        }

        public void Notify(ServerEvent e)
        {
            this.Notify(e, (object)null, (EventArgs)null);
        }

        public void Notify(ServerEvent e, EventArgs args)
        {
            this.Notify(e, (object)null, args);
        }

        public void Notify(ServerEvent e, object sender)
        {
            this.Notify(e, sender, (EventArgs)null);
        }

        public void Notify(ServerEvent e, object sender, EventArgs eArgs)
        {
            try
            {
                this.m_lock.AcquireReaderLock(TIMEOUT);
                WeakMulticastDelegate multicastDelegate;
                try
                {
                    multicastDelegate = (WeakMulticastDelegate)this.m_events[(object)e];
                }
                finally
                {
                    this.m_lock.ReleaseReaderLock();
                }
                multicastDelegate?.InvokeSafe(new object[3]
                {
          (object) e,
          sender,
          (object) eArgs
                });
            }
            catch (ApplicationException ex)
            {
                if (!GameEventHandlerCollection.log.IsErrorEnabled)
                    return;
                GameEventHandlerCollection.log.Error((object)"Failed to notify event handler!", (Exception)ex);
            }
        }

        public void RemoveAllHandlers()
        {
            try
            {
                this.m_lock.AcquireWriterLock(TIMEOUT);
                try
                {
                    this.m_events.Clear();
                }
                finally
                {
                    this.m_lock.ReleaseWriterLock();
                }
            }
            catch (ApplicationException ex)
            {
                if (!GameEventHandlerCollection.log.IsErrorEnabled)
                    return;
                GameEventHandlerCollection.log.Error((object)"Failed to remove all event handlers!", (Exception)ex);
            }
        }

        public void RemoveAllHandlers(ServerEvent e)
        {
            try
            {
                this.m_lock.AcquireWriterLock(TIMEOUT);
                try
                {
                    this.m_events.Remove((object)e);
                }
                finally
                {
                    this.m_lock.ReleaseWriterLock();
                }
            }
            catch (ApplicationException ex)
            {
                if (!GameEventHandlerCollection.log.IsErrorEnabled)
                    return;
                GameEventHandlerCollection.log.Error((object)"Failed to remove event handlers!", (Exception)ex);
            }
        }

        public void RemoveHandler(ServerEvent e, ServerEventHandler del)
        {
            try
            {
                this.m_lock.AcquireWriterLock(TIMEOUT);
                try
                {
                    WeakMulticastDelegate weakDelegate = (WeakMulticastDelegate)this.m_events[(object)e];
                    if (weakDelegate == null)
                        return;
                    WeakMulticastDelegate multicastDelegate = WeakMulticastDelegate.Remove(weakDelegate, (Delegate)del);
                    if (multicastDelegate == null)
                        this.m_events.Remove((object)e);
                    else
                        this.m_events[(object)e] = (object)multicastDelegate;
                }
                finally
                {
                    this.m_lock.ReleaseWriterLock();
                }
            }
            catch (ApplicationException ex)
            {
                if (!GameEventHandlerCollection.log.IsErrorEnabled)
                    return;
                GameEventHandlerCollection.log.Error((object)"Failed to remove event handler!", (Exception)ex);
            }
        }
    }
}
