using System;
using System.Collections.Generic;
using System.Text;
using DrveTerm;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


namespace DrveTerm
{
    [ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public sealed class DrvCtxEventSinkHelper : _IDrvCtxEvents
    {
        // Fields
        public int m_dwCookie = 0;
        public _IDrvCtxEvents_OnAsyncDataEventHandler m_OnAsyncDataDelegate = null;
        public _IDrvCtxEvents_OnNotifyEventHandler m_OnNotifyDelegate = null;
        public _IDrvCtxEvents_OnStatusEventHandler m_OnStatusDelegate = null;
        public _IDrvCtxEvents_OnSyncDataEventHandler m_OnSyncDataDelegate = null;

        // Methods
        [DispId(1)]
        public void OnAsyncData(string bstrData, string bstrProperties)
        {
            if (this.m_OnAsyncDataDelegate != null)
            {
                this.m_OnAsyncDataDelegate(bstrData, bstrProperties);
            }
        }

        [DispId(2)]
        public void OnSyncData(string bstrCommand, string bstrCacheResponse, ref int lpdwRemainMultiResponse, ref string lpbstrResponse, string bstrProperties)
        {
            if (this.m_OnSyncDataDelegate != null)
            {
                this.m_OnSyncDataDelegate(bstrCommand, bstrCacheResponse, ref lpdwRemainMultiResponse, ref lpbstrResponse, bstrProperties);
            }
        }

        [DispId(3)]
        public void OnStatus(int nStatus, string bstrStatusMessage)
        {
            if (this.m_OnStatusDelegate != null)
            {
                this.m_OnStatusDelegate(nStatus, bstrStatusMessage);
            }
        }

        [DispId(4)]
        public void OnNotify(int nNotify, string bstrNotifyMessage)
        {
            if (this.m_OnNotifyDelegate != null)
            {
                this.m_OnNotifyDelegate(nNotify, bstrNotifyMessage);
            }
        }
    }

    public class DrvCtxWrap : IDisposable
    {
        // Fields
        // Track whether Dispose has been called.
        private bool disposed = false;

        private IDrvCtx _drvCtx = null;
        private DrvCtxEventSinkHelper _eventSinkHelper = null;
        private IConnectionPoint _connectionPoint = null;
        private IConnectionPointContainer _connectionPointContainer = null;

        // Events
        public event _IDrvCtxEvents_OnAsyncDataEventHandler _IDrvCtxEvents_Event_OnAsyncData;
        public event _IDrvCtxEvents_OnNotifyEventHandler _IDrvCtxEvents_Event_OnNotify;
        public event _IDrvCtxEvents_OnStatusEventHandler _IDrvCtxEvents_Event_OnStatus;
        public event _IDrvCtxEvents_OnSyncDataEventHandler _IDrvCtxEvents_Event_OnSyncData;

        public void OnAsyncData(string bstrData, string bstrProperties)
        {
            if (this._IDrvCtxEvents_Event_OnAsyncData != null)
            {
                this._IDrvCtxEvents_Event_OnAsyncData(bstrData, bstrProperties);
            }
        }

        public void OnSyncData(string bstrCommand, string bstrCacheResponse, ref int lpdwRemainMultiResponse, ref string lpbstrResponse, string bstrProperties)
        {
            if (this._IDrvCtxEvents_Event_OnSyncData != null)
            {
                this._IDrvCtxEvents_Event_OnSyncData(bstrCommand, bstrCacheResponse, ref lpdwRemainMultiResponse, ref lpbstrResponse, bstrProperties);
            }
        }

        public void OnStatus(int nStatus, string bstrStatusMessage)
        {
            if (this._IDrvCtxEvents_Event_OnStatus != null)
            {
                this._IDrvCtxEvents_Event_OnStatus(nStatus, bstrStatusMessage);
            }
        }

        public void OnNotify(int nNotify, string bstrNotifyMessage)
        {
            if (this._IDrvCtxEvents_Event_OnNotify != null)
            {
                this._IDrvCtxEvents_Event_OnNotify(nNotify, bstrNotifyMessage);
            }
        }


        // Methods
        public DrvCtxWrap(IDrvCtx ctx)
        {
            try
            {
                this._drvCtx = ctx;
                this._connectionPointContainer = (IConnectionPointContainer)ctx;

                IConnectionPoint ppCP = null;
                byte[] b = new byte[] { 0xe5, 0x26, 0xd6, 0x63, 0x58, 0x20, 0x18, 0x4d, 0xa2, 190, 0x52, 0x3f, 0xf5, 0x3d, 0x44, 0x34 };
                Guid riid = new Guid(b);

                this._connectionPointContainer.FindConnectionPoint(ref riid, out ppCP);
                this._connectionPoint = (IConnectionPoint)ppCP;

                this._eventSinkHelper = new DrvCtxEventSinkHelper();
                this._eventSinkHelper.m_OnAsyncDataDelegate = new _IDrvCtxEvents_OnAsyncDataEventHandler(this.OnAsyncData);
                this._eventSinkHelper.m_OnNotifyDelegate = new _IDrvCtxEvents_OnNotifyEventHandler(this.OnNotify);
                this._eventSinkHelper.m_OnStatusDelegate = new _IDrvCtxEvents_OnStatusEventHandler(this.OnStatus);
                this._eventSinkHelper.m_OnSyncDataDelegate = new _IDrvCtxEvents_OnSyncDataEventHandler(this.OnSyncData);


                int pdwCookie = 0;
                this._connectionPoint.Advise((object)this._eventSinkHelper, out pdwCookie);
                this._eventSinkHelper.m_dwCookie = pdwCookie;
            }
            catch (Exception theException)
            {
                String errorMessage;
                errorMessage = "Error: ";
                errorMessage = String.Concat(errorMessage, theException.Message);
                errorMessage = String.Concat(errorMessage, " Line: ");
                errorMessage = String.Concat(errorMessage, theException.Source);

                //MessageBox.Show(errorMessage, "Error");

                throw theException;
            }
        }

        public IDrvCtx GetCtx()
        {
            return this._drvCtx;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~DrvCtxWrap()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    this._connectionPointContainer = null;
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here.
                // If disposing is false, 
                // only the following code is executed.
                try
                {
                    if (this._connectionPoint != null)
                    {
                        this._connectionPoint.Unadvise(this._eventSinkHelper.m_dwCookie);
                        Marshal.ReleaseComObject(this._connectionPoint);
                        this._connectionPoint = null;
                        this._eventSinkHelper = null;
                    }

                    if (this._drvCtx != null)
                    {
                        Marshal.ReleaseComObject(this._drvCtx);
                        this._drvCtx = null;
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                }
            }
            disposed = true;
        }
    }
}
