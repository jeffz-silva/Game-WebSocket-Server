using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Components.Events
{
    public sealed class GameEventMgr
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GameEventMgr));

        private static readonly HybridDictionary _gameObjectEventCollections = new HybridDictionary();
        private static GameEventHandlerCollection m_GlobalHandlerCollection = new GameEventHandlerCollection();

        private static readonly ReaderWriterLock _readerWriterLock = new ReaderWriterLock();

        public static int ACQUIRE_TIMEOUT = 3000;

        public static void AddHandler(ServerEvent e, ServerEventHandler del)
        {
            GameEventMgr.AddHandler(e, del, false);
        }

        private static void AddHandler(ServerEvent e, ServerEventHandler del, bool unique)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e), "No event type given!");
            if (del == null)
                throw new ArgumentNullException(nameof(del), "No event handler given!");
            if (unique)
                m_GlobalHandlerCollection.AddHandlerUnique(e, del);
            else
                m_GlobalHandlerCollection.AddHandler(e, del);
        }

        public static void AddHandler(object obj, ServerEvent e, ServerEventHandler del)
        {
            AddHandler(obj, e, del, false);
        }


        private static void AddHandler(object obj, ServerEvent e, ServerEventHandler del, bool unique)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "No object given!");
            if (e == null)
                throw new ArgumentNullException(nameof(e), "No event type given!");
            if (del == null)
                throw new ArgumentNullException(nameof(del), "No event handler given!");
            if (!e.isValidFor(obj))
                throw new ArgumentException("Object is not valid for this event type", nameof(obj));

            try
            {
                _readerWriterLock.AcquireReaderLock(ACQUIRE_TIMEOUT);
                try
                {
                    GameEventHandlerCollection handlerCollection = (GameEventHandlerCollection)_gameObjectEventCollections[obj];
                    if (handlerCollection == null)
                    {
                        handlerCollection = new GameEventHandlerCollection();
                        LockCookie writerLock = _readerWriterLock.UpgradeToWriterLock(ACQUIRE_TIMEOUT);
                        try
                        {
                            _gameObjectEventCollections[obj] = handlerCollection;
                        }
                        finally
                        {
                            _readerWriterLock.DowngradeFromWriterLock(ref writerLock);
                        }
                    }
                    if (unique) handlerCollection.AddHandlerUnique(e, del);
                    else handlerCollection.AddHandler(e, del);
                }
                finally
                {
                    _readerWriterLock.ReleaseLock();
                }
            }
            catch (ApplicationException ex)
            {
                if (log.IsErrorEnabled)
                    log.Error($"Failed to add local event handler! [{ex.Message}:{ex.StackTrace}]");
            }
        }

        public static void AddHandlerUnique(ServerEvent e, ServerEventHandler del)
        {
            GameEventMgr.AddHandler(e, del, true);
        }

        public static void AddHandlerUnique(object obj, ServerEvent e, ServerEventHandler del)
        {
            GameEventMgr.AddHandler(obj, e, del, true);
        }

        public static void Notify(ServerEvent e)
        {
            GameEventMgr.Notify(e, (object)null, (EventArgs)null);
        }

        public static void Notify(ServerEvent e, EventArgs args)
        {
            GameEventMgr.Notify(e, (object)null, args);
        }

        public static void Notify(ServerEvent e, object sender)
        {
            GameEventMgr.Notify(e, sender, (EventArgs)null);
        }

        public static void Notify(ServerEvent e, object sender, EventArgs eArgs)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e), "No event type given!");
            if (sender != null)
            {
                try
                {
                    GameEventHandlerCollection handlerCollection = (GameEventHandlerCollection)null;
                    GameEventMgr._readerWriterLock.AcquireReaderLock(ACQUIRE_TIMEOUT);
                    try
                    {
                        handlerCollection = (GameEventHandlerCollection)GameEventMgr._gameObjectEventCollections[sender];
                    }
                    finally
                    {
                        GameEventMgr._readerWriterLock.ReleaseReaderLock();
                    }
                    handlerCollection?.Notify(e, sender, eArgs);
                }
                catch (ApplicationException ex)
                {
                    if (GameEventMgr.log.IsErrorEnabled)
                        GameEventMgr.log.Error((object)"Failed to notify local event handler!", (Exception)ex);
                }
            }
            GameEventMgr.m_GlobalHandlerCollection.Notify(e, sender, eArgs);
        }

        public static void RegisterGlobalEvents(Assembly asm, Type attribute, ServerEvent e)
        {
            if (asm == (Assembly)null)
                throw new ArgumentNullException(nameof(asm), "No assembly given to search for global event handlers!");
            if (attribute == (Type)null)
                throw new ArgumentNullException(nameof(attribute), "No attribute given!");
            if (e == null)
                throw new ArgumentNullException(nameof(e), "No event type given!");
            foreach (Type type in asm.GetTypes())
            {
                if (type.IsClass)
                {
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    {
                        if ((uint)method.GetCustomAttributes(attribute, false).Length > 0U)
                        {
                            try
                            {
                                GameEventMgr.m_GlobalHandlerCollection.AddHandler(e, (ServerEventHandler)Delegate.CreateDelegate(typeof(ServerEventHandler), method));
                            }
                            catch (Exception ex)
                            {
                                if (GameEventMgr.log.IsErrorEnabled)
                                    GameEventMgr.log.Error((object)("Error registering global event. Method: " + type.FullName + "." + method.Name), ex);
                            }
                        }
                    }
                }
            }
        }

        public static void RemoveAllHandlers(bool deep)
        {
            if (deep)
            {
                try
                {
                    GameEventMgr._readerWriterLock.AcquireWriterLock(ACQUIRE_TIMEOUT);
                    try
                    {
                        GameEventMgr._gameObjectEventCollections.Clear();
                    }
                    finally
                    {
                        GameEventMgr._readerWriterLock.ReleaseWriterLock();
                    }
                }
                catch (ApplicationException ex)
                {
                    if (GameEventMgr.log.IsErrorEnabled)
                        GameEventMgr.log.Error((object)"Failed to remove all local event handlers!", (Exception)ex);
                }
            }
            GameEventMgr.m_GlobalHandlerCollection.RemoveAllHandlers();
        }

        public static void RemoveAllHandlers(ServerEvent e, bool deep)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e), "No event type given!");
            if (deep)
            {
                try
                {
                    GameEventMgr._readerWriterLock.AcquireReaderLock(ACQUIRE_TIMEOUT);
                    try
                    {
                        foreach (GameEventHandlerCollection handlerCollection in (IEnumerable)GameEventMgr._gameObjectEventCollections.Values)
                            handlerCollection.RemoveAllHandlers(e);
                    }
                    finally
                    {
                        GameEventMgr._readerWriterLock.ReleaseReaderLock();
                    }
                }
                catch (ApplicationException ex)
                {
                    if (GameEventMgr.log.IsErrorEnabled)
                        GameEventMgr.log.Error((object)"Failed to add local event handlers!", (Exception)ex);
                }
            }
            GameEventMgr.m_GlobalHandlerCollection.RemoveAllHandlers(e);
        }

        public static void RemoveAllHandlers(object obj, ServerEvent e)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "No object given!");
            if (e == null)
                throw new ArgumentNullException(nameof(e), "No event type given!");
            try
            {
                GameEventMgr._readerWriterLock.AcquireReaderLock(ACQUIRE_TIMEOUT);
                try
                {
                    ((GameEventHandlerCollection)GameEventMgr._gameObjectEventCollections[obj])?.RemoveAllHandlers(e);
                }
                finally
                {
                    GameEventMgr._readerWriterLock.ReleaseReaderLock();
                }
            }
            catch (ApplicationException ex)
            {
                if (!GameEventMgr.log.IsErrorEnabled)
                    return;
                GameEventMgr.log.Error((object)"Failed to remove local event handlers!", (Exception)ex);
            }
        }

        public static void RemoveHandler(ServerEvent e, ServerEventHandler del)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e), "No event type given!");
            if (del == null)
                throw new ArgumentNullException(nameof(del), "No event handler given!");
            GameEventMgr.m_GlobalHandlerCollection.RemoveHandler(e, del);
        }

        public static void RemoveHandler(object obj, ServerEvent e, ServerEventHandler del)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "No object given!");
            if (e == null)
                throw new ArgumentNullException(nameof(e), "No event type given!");
            if (del == null)
                throw new ArgumentNullException(nameof(del), "No event handler given!");
            try
            {
                GameEventMgr._readerWriterLock.AcquireReaderLock(ACQUIRE_TIMEOUT);
                try
                {
                    ((GameEventHandlerCollection)GameEventMgr._gameObjectEventCollections[obj])?.RemoveHandler(e, del);
                }
                finally
                {
                    GameEventMgr._readerWriterLock.ReleaseReaderLock();
                }
            }
            catch (ApplicationException ex)
            {
                if (!GameEventMgr.log.IsErrorEnabled)
                    return;
                GameEventMgr.log.Error((object)"Failed to remove local event handler!", (Exception)ex);
            }
        }
    }
}
