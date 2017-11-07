using System;
using System.Collections.Generic;
using Jungo.wdapi_dotnet;
using wdc_err = Jungo.wdapi_dotnet.WD_ERROR_CODES;
using item_types = Jungo.wdapi_dotnet.ITEM_TYPE;
using UINT64 = System.UInt64;
using UINT32 = System.UInt32;
using DWORD = System.UInt32;
using WORD = System.UInt16;
using BYTE = System.Byte;
using BOOL = System.Boolean;
using WDC_DEVICE_HANDLE = System.IntPtr;
using HANDLE = System.IntPtr;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace Hcdz.PcieLib
{
    /* PCI diagnostics plug-and-play and power management events handler *
     * function type */
    public delegate void USER_EVENT_CALLBACK(ref WD_EVENT pEvent, PCIE_Device dev);
    /* PCI diagnostics interrupt handler function type */
    public delegate void USER_INTERRUPT_CALLBACK(PCIE_Device device);


    public partial class PCIE_Device
    {
        private WDC_DEVICE m_wdcDevice = new WDC_DEVICE();
        protected MarshalWdcDevice m_wdcDeviceMarshaler;
        public MarshalWdDma m_dmaMarshaler;
        private USER_EVENT_CALLBACK m_userEventHandler;
        private USER_INTERRUPT_CALLBACK m_userIntHandler;
        private EVENT_HANDLER_DOTNET m_eventHandler;
        private INT_HANDLER m_intHandler;
        protected string m_sDeviceLongDesc;
        protected string m_sDeviceShortDesc;
        private PCIE_Regs m_regs;
        public string Name { get; set; }
        public int Status { get; set; }
      public FileStream DeviceFile { get; set; }
        #region " constructors " 
        /* constructors & destructors */
       public PCIE_Device(WD_PCI_SLOT slot) : this(0, 0, slot) { }

		public PCIE_Device(DWORD dwVendorId, DWORD dwDeviceId,
            WD_PCI_SLOT slot)
        {
            m_wdcDevice = new WDC_DEVICE();
            m_wdcDevice.id.pciId.dwVendorId = dwVendorId;
            m_wdcDevice.id.pciId.dwDeviceId = dwDeviceId;
            m_wdcDevice.slot.pciSlot = slot;
            m_wdcDeviceMarshaler = new MarshalWdcDevice();
            m_eventHandler = new EVENT_HANDLER_DOTNET(NEWAMD86_EventHandler);
            m_regs = new PCIE_Regs();
            SetDescription();
            m_dmaMarshaler = new MarshalWdDma();
        }

        public void Dispose()
        {
            Close();
        }
        #endregion

        #region " properties " 
        /*********************
         *  properties       *
         *********************/

        public IntPtr Handle
        {
            get
            {
                return m_wdcDevice.hDev;
            }
            set
            {
                m_wdcDevice.hDev = value;
            }
        }

        protected WDC_DEVICE wdcDevice
        {
            get
            {
                return m_wdcDevice;
            }
            set
            {
                m_wdcDevice = value;
            }
        }

        public WD_PCI_ID id
        {
            get
            {
                return m_wdcDevice.id.pciId;
            }
            set
            {
                m_wdcDevice.id.pciId = value;
            }
        }

        public WD_PCI_SLOT slot
        {
            get
            {
                return m_wdcDevice.slot.pciSlot;
            }
            set
            {
                m_wdcDevice.slot.pciSlot = value;
            }
        }

        public WDC_ADDR_DESC[] AddrDesc
        {
            get
            {
                return m_wdcDevice.pAddrDesc;
            }
            set
            {
                m_wdcDevice.pAddrDesc = value;
            }
        }

        public PCIE_Regs Regs
        {
            get
            {
                return m_regs;
            }
        }

        #endregion

        #region " utilities " 
        /********************
         *     utilities    *
         *********************/

        /* public methods */

        public string[] AddrDescToString(bool bMemOnly)
        {
            string[] sAddr = new string[AddrDesc.Length];
            for (int i = 0; i < sAddr.Length; ++i)
            {
                sAddr[i] = "BAR " + AddrDesc[i].dwAddrSpace.ToString() +
                     ((AddrDesc[i].fIsMemory) ? " Memory " : " I/O ");

                if (wdc_lib_decl.WDC_AddrSpaceIsActive(Handle,
                    AddrDesc[i].dwAddrSpace))
                {
                    WD_ITEMS item =
                        m_wdcDevice.cardReg.Card.Item[AddrDesc[i].dwItemIndex];
                    UINT64 dwAddr = (UINT64)(AddrDesc[i].fIsMemory ?
                        item.I.Mem.dwPhysicalAddr : item.I.IO.dwAddr);

                    sAddr[i] += dwAddr.ToString("X") + " - " +
                        (dwAddr + AddrDesc[i].dwBytes - 1).ToString("X") +
                        " (" + AddrDesc[i].dwBytes.ToString("X") + " bytes)";
                }
                else
                    sAddr[i] += "Inactive address space";
            }
            return sAddr;
        }

        public string ToString(BOOL bLong)
        {
            return (bLong) ? m_sDeviceLongDesc : m_sDeviceShortDesc;
        }

        public bool IsMySlot(ref WD_PCI_SLOT slot)
        {
            if (m_wdcDevice.slot.pciSlot.dwBus == slot.dwBus &&
                m_wdcDevice.slot.pciSlot.dwSlot == slot.dwSlot &&
                m_wdcDevice.slot.pciSlot.dwFunction == slot.dwFunction)
                return true;

            return false;
        }

        /* protected methods */

        protected void SetDescription()
        {
            m_sDeviceLongDesc = string.Format("NEWAMD86 Device: Vendor ID 0x{0:X}, "
                + "Device ID 0x{1:X}, Physical Location {2:X}:{3:X}:{4:X}",
                id.dwVendorId, id.dwDeviceId, slot.dwBus, slot.dwSlot,
                slot.dwFunction);

            m_sDeviceShortDesc = string.Format("Device " +
                "{0:X},{1:X} {2:X}:{3:X}:{4:X}", id.dwVendorId,
                id.dwDeviceId, slot.dwBus, slot.dwSlot, slot.dwFunction);
            Name = m_sDeviceShortDesc;
        }

        /* private methods */

        private bool DeviceValidate()
        {
            DWORD i, dwNumAddrSpaces = m_wdcDevice.dwNumAddrSpaces;

            /* NOTE: You can modify the implementation of this function in     *
             * order to verify that the device has the resources you expect to *
             * find */

            /* Verify that the device has at least one active address space */
            for (i = 0; i < dwNumAddrSpaces; i++)
            {
                if (wdc_lib_decl.WDC_AddrSpaceIsActive(Handle, i))
                    return true;
            }

            Log.TraceLog("NEWAMD86_Device.DeviceValidate: Device does not have "
                + "any active memory or I/O address spaces " + "(" +
                this.ToString(false) + ")");
            return true;
        }

        #endregion

        #region " Device Open/Close " 
        /****************************
         *  Device Open & Close      *
         *****************************/

        /* public methods */

        public virtual DWORD Open()
        {
            DWORD dwStatus;
            WD_PCI_CARD_INFO deviceInfo = new WD_PCI_CARD_INFO();

            /* Retrieve the device's resources information */
            deviceInfo.pciSlot = slot;
            dwStatus = wdc_lib_decl.WDC_PciGetDeviceInfo(deviceInfo);
            if ((DWORD)wdc_err.WD_STATUS_SUCCESS != dwStatus)
            {
                Log.ErrLog("NEWAMD86_Device.Open: Failed retrieving the "
                    + "device's resources information. Error 0x" +
                    dwStatus.ToString("X") + ": " + utils.Stat2Str(dwStatus) +
                    "(" + this.ToString(false) + ")");
                return dwStatus;
            }

            /* NOTE: You can modify the device's resources information here, 
             * if necessary (mainly the deviceInfo.Card.Items array or the
             * items number - deviceInfo.Card.dwItems) in order to register
             * only some of the resources or register only a portion of a
             * specific address space, for example. */

            dwStatus = wdc_lib_decl.WDC_PciDeviceOpen(ref m_wdcDevice,
                deviceInfo, IntPtr.Zero, IntPtr.Zero, "", IntPtr.Zero);

            if ((DWORD)wdc_err.WD_STATUS_SUCCESS != dwStatus)
            {
                Log.ErrLog("NEWAMD86_Device.Open: Failed opening a " +
                    "WDC device handle. Error 0x" + dwStatus.ToString("X") +
                    ": " + utils.Stat2Str(dwStatus) + "(" +
                    this.ToString(false) + ")");
                goto Error;
            }

            Log.TraceLog("NEWAMD86_Device.Open: Opened a PCI device " +
                this.ToString(false));

            /* Validate device information */
            if (DeviceValidate() != true)
            {
                dwStatus = (DWORD)wdc_err.WD_NO_RESOURCES_ON_DEVICE;
                goto Error;
            }

            return dwStatus;
        Error:
            if (Handle != IntPtr.Zero)
                Close();

            return dwStatus;
        }

        public virtual bool Close()
        {
            DWORD dwStatus;

            if (Handle == IntPtr.Zero)
            {
                Log.ErrLog("NEWAMD86_Device.Close: Error - NULL "
                    + "device handle");
                return false;
            }

            /* unregister events*/
            dwStatus = EventUnregister();

            /* Disable interrupts */
            dwStatus = DisableInterrupts();

            /* Close the device */
            dwStatus = wdc_lib_decl.WDC_PciDeviceClose(Handle);
            if ((DWORD)wdc_err.WD_STATUS_SUCCESS != dwStatus)
            {
                Log.ErrLog("NEWAMD86_Device.Close: Failed closing a "
                    + "WDC device handle (0x" + Handle.ToInt64().ToString("X")
                    + ". Error 0x" + dwStatus.ToString("X") + ": " +
                    utils.Stat2Str(dwStatus) + this.ToString(false));
            }
            else
            {
                Log.TraceLog("NEWAMD86_Device.Close: " +
                    this.ToString(false) + " was closed successfully");
            }

            return ((DWORD)wdc_err.WD_STATUS_SUCCESS == dwStatus);
        }

        #endregion

        #region " Interrupts "
        /* public methods */
        public bool IsEnabledInt()
        {
            return wdc_lib_decl.WDC_IntIsEnabled(this.Handle);
        }

        protected virtual DWORD CreateIntTransCmds(out WD_TRANSFER[]
            pIntTransCmds, out DWORD dwNumCmds)
        {
            /* Define the number of interrupt transfer commands to use */
            DWORD NUM_TRANS_CMDS = 0;
            pIntTransCmds = new WD_TRANSFER[NUM_TRANS_CMDS];
            /*
            TODO: Your hardware has level sensitive interrupts, which must be
          acknowledged in the kernel immediately when they are received.
                  Since the information for acknowledging the interrupts is
            hardware-specific, YOU MUST ADD CODE to read/write the relevant
            register(s) in order to correctly acknowledge the interrupts
            on your device, as dictated by your hardware's specifications.
            When adding transfer commands, be sure to also modify the
            definition of NUM_TRANS_CMDS (above) accordingly.
             
            *************************************************************************   
            * NOTE: If you attempt to use this code without first modifying it in   *
            *       order to correctly acknowledge your device's interrupts, as     *
            *       explained above, the OS will HANG when an interrupt occurs!     *
            *************************************************************************
            */
            dwNumCmds = NUM_TRANS_CMDS;
            return (DWORD)wdc_err.WD_STATUS_SUCCESS;
        }

        protected virtual DWORD DisableCardInts()
        {
            /* TODO: You can add code here to write to the device in order
             * to physically disable the hardware interrupts */
            return (DWORD)wdc_err.WD_STATUS_SUCCESS;
        }

        protected BOOL IsItemExists(WDC_DEVICE Dev, DWORD item)
        {
            int i;
            DWORD dwNumItems = Dev.cardReg.Card.dwItems;

            for (i = 0; i < dwNumItems; i++)
            {
                if (Dev.cardReg.Card.Item[i].item == item)
                    return true;
            }

            return false;
        }


        public DWORD EnableInterrupts(USER_INTERRUPT_CALLBACK userIntCb, IntPtr pData)
        {
            DWORD dwStatus;
            WD_TRANSFER[] pIntTransCmds = null;
            DWORD dwNumCmds;
            if (userIntCb == null)
            {
                Log.TraceLog("NEWAMD86_Device.EnableInterrupts: "
                    + "user callback is invalid");
                return (DWORD)wdc_err.WD_INVALID_PARAMETER;
            }

            if (!IsItemExists(m_wdcDevice, (DWORD)item_types.ITEM_INTERRUPT))
            {
                Log.TraceLog("NEWAMD86_Device.EnableInterrupts: "
                    + "Device doesn't have any interrupts");
                return (DWORD)wdc_err.WD_OPERATION_FAILED;
            }

            m_userIntHandler = userIntCb;

            m_intHandler = new INT_HANDLER(NEWAMD86_IntHandler);
            if (m_intHandler == null)
            {
                Log.ErrLog("NEWAMD86_Device.EnableInterrupts: interrupt handler is " +
                    "null (" + this.ToString(false) + ")");
                return (DWORD)wdc_err.WD_INVALID_PARAMETER;
            }

            if (wdc_lib_decl.WDC_IntIsEnabled(Handle))
            {
                Log.ErrLog("NEWAMD86_Device.EnableInterrupts: "
                    + "interrupts are already enabled (" +
                    this.ToString(false) + ")");
                return (DWORD)wdc_err.WD_OPERATION_ALREADY_DONE;
            }

            dwStatus = CreateIntTransCmds(out pIntTransCmds, out dwNumCmds);
            if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
                return dwStatus;
            dwStatus = wdc_lib_decl.WDC_IntEnable(wdcDevice, pIntTransCmds,
                dwNumCmds, 0, m_intHandler, pData, wdc_defs_macros.WDC_IS_KP(wdcDevice));

            if ((DWORD)wdc_err.WD_STATUS_SUCCESS != dwStatus)
            {
                Log.ErrLog("NEWAMD86_Device.EnableInterrupts: Failed "
                    + "enabling interrupts. Error " + dwStatus.ToString("X") + ": "
                    + utils.Stat2Str(dwStatus) + "(" + this.ToString(false) + ")");
                m_intHandler = null;
                return dwStatus;
            }
            /* TODO: You can add code here to write to the device in order
                 to physically enable the hardware interrupts */

            Log.TraceLog("NEWAMD86_Device: enabled interrupts (" + this.ToString(false) + ")");
            return dwStatus;
        }

        public DWORD DisableInterrupts()
        {
            DWORD dwStatus;

            if (!wdc_lib_decl.WDC_IntIsEnabled(this.Handle))
            {
                Log.ErrLog("NEWAMD86_Device.DisableInterrupts: interrupts are already disabled... " +
                    "(" + this.ToString(false) + ")");
                return (DWORD)wdc_err.WD_OPERATION_ALREADY_DONE;
            }

            /* Physically disabling the hardware interrupts */
            dwStatus = DisableCardInts();

            dwStatus = wdc_lib_decl.WDC_IntDisable(m_wdcDevice);
            if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
            {
                Log.ErrLog("NEWAMD86_Device.DisableInterrupts: Failed to" +
                    "disable interrupts. Error " + dwStatus.ToString("X")
                    + ": " + utils.Stat2Str(dwStatus) + " (" +
                    this.ToString(false) + ")");
            }
            else
            {
                Log.TraceLog("NEWAMD86_Device.DisableInterrupts: Interrupts are disabled" +
                    "(" + this.ToString(false) + ")");
            }

            return dwStatus;
        }

        /* private methods */
        private void NEWAMD86_IntHandler(IntPtr pDev)
        {
            wdcDevice.Int =
                (WD_INTERRUPT)m_wdcDeviceMarshaler.MarshalDevWdInterrupt(pDev);

            /* to obtain the data that was read at interrupt use:
             * WD_TRANSFER[] transCommands;
             * transCommands = (WD_TRANSFER[])m_wdcDeviceMarshaler.MarshalDevpWdTrans(
             *     wdcDevice.Int.Cmd, wdcDevice.Int.dwCmds); */

            if (m_userIntHandler != null)
                m_userIntHandler(this);
        }

        #endregion

        #region " Events"
        /****************************
         *          Events          *
         * **************************/

        /* public methods */

        public bool IsEventRegistered()
        {
            if (Handle == IntPtr.Zero)
                return false;

            return wdc_lib_decl.WDC_EventIsRegistered(Handle);
        }

        public DWORD EventRegister(USER_EVENT_CALLBACK userEventHandler)
        {
            DWORD dwStatus;
            DWORD dwActions = (DWORD)windrvr_consts.WD_ACTIONS_ALL;
            /* TODO: Modify the above to set up the plug-and-play/power 
             * management events for which you wish to receive notifications.
             * dwActions can be set to any combination of the WD_EVENT_ACTION
             * flags defined in windrvr.h */

            if (userEventHandler == null)
            {
                Log.ErrLog("NEWAMD86_Device.EventRegister: user callback is "
                    + "null");
                return (DWORD)wdc_err.WD_INVALID_PARAMETER;
            }

            /* Check if event is already registered */
            if (wdc_lib_decl.WDC_EventIsRegistered(Handle))
            {
                Log.ErrLog("NEWAMD86_Device.EventRegister: Events are already "
                    + "registered ...");
                return (DWORD)wdc_err.WD_OPERATION_ALREADY_DONE;
            }

            m_userEventHandler = userEventHandler;

            /* Register event */
            dwStatus = wdc_lib_decl.WDC_EventRegister(m_wdcDevice, dwActions,
                m_eventHandler, Handle, wdc_defs_macros.WDC_IS_KP(wdcDevice));

            if ((DWORD)wdc_err.WD_STATUS_SUCCESS != dwStatus)
            {
                Log.ErrLog("NEWAMD86_Device.EventRegister: Failed to register "
                    + "events. Error 0x" + dwStatus.ToString("X")
                    + utils.Stat2Str(dwStatus));
                m_userEventHandler = null;
            }
            else
            {
                Log.TraceLog("NEWAMD86_Device.EventRegister: events are " +
                    " registered (" + this.ToString(false) + ")");
            }

            return dwStatus;
        }

        public DWORD EventUnregister()
        {
            DWORD dwStatus;

            if (!wdc_lib_decl.WDC_EventIsRegistered(Handle))
            {
                Log.ErrLog("NEWAMD86_Device.EventUnregister: No events " +
                    "currently registered ...(" + this.ToString(false) + ")");
                return (DWORD)wdc_err.WD_OPERATION_ALREADY_DONE;
            }

            dwStatus = wdc_lib_decl.WDC_EventUnregister(m_wdcDevice);

            if ((DWORD)wdc_err.WD_STATUS_SUCCESS != dwStatus)
            {
                Log.ErrLog("NEWAMD86_Device.EventUnregister: Failed to " +
                    "unregister events. Error 0x" + dwStatus.ToString("X") +
                    ": " + utils.Stat2Str(dwStatus) + "(" +
                    this.ToString(false) + ")");
            }
            else
            {
                Log.TraceLog("NEWAMD86_Device.EventUnregister: Unregistered " +
                    " events (" + this.ToString(false) + ")");
            }

            return dwStatus;
        }

        /** private methods **/

        /* event callback method */
        private void NEWAMD86_EventHandler(IntPtr pWdEvent, IntPtr pDev)
        {
            MarshalWdEvent wdEventMarshaler = new MarshalWdEvent();
            WD_EVENT wdEvent = (WD_EVENT)wdEventMarshaler.MarshalNativeToManaged(pWdEvent);
            m_wdcDevice.Event =
                (WD_EVENT)m_wdcDeviceMarshaler.MarshalDevWdEvent(pDev);
            if (m_userEventHandler != null)
                m_userEventHandler(ref wdEvent, this);
        }
        #endregion
        [DllImport("kernel32.dll", EntryPoint = "WriteFile", SetLastError = true, ThrowOnUnmappableChar = true)]
        public static extern unsafe bool WriteFile
   (
       System.IntPtr hFile,
       ref IntPtr lpBuffer,
       uint nNumberOfBytesToWrite,
       out uint lpNumberOfBytesWritten,
       int lpOverlapped
   );
        public IntPtr ppwDma;
        public IntPtr pScanpwDma;
        public IntPtr pWbuffer;
        public IntPtr pScanWbuffer;
        public IntPtr pReportWrBuffer;
        public IntPtr pScanReportWrBuffer;
        public WD_DMA WdDma;
        HANDLE? hev;
        HANDLE? hfile1;
        DWORD tranBlock, addrwpointer, addrrpointer;//变量1：传输的总块数，变量2：写缓冲区对应物理首地址索引，变量3：读缓冲物理地址索引
        DWORD tranBlock1, tranBlock2;
        DWORD rBlockSize, wBlockSize, rBNum, wBnum;//变量1：读缓冲区大小，变量2：写缓冲区大小，变量3：读缓冲区个数，变量4：写缓冲区个数
        public IntPtr pReportWrDMA;
        public IntPtr pScanReportWrDMA;
        public bool DMAWriteMenAlloc(DWORD index, DWORD menBlocknum, DWORD blocksize)
        {
            DWORD status;

            wBlockSize = blocksize;
            wBnum = menBlocknum;
            if (menBlocknum > 1024 || menBlocknum < 1)
            {
                //AfxMessageBox(_T("你输入的内存块数大于32或者小于1,系统拒绝执行内存分配"));
                //strcpy_s(infor,"你输入的内存块数大于2或者小于1，系统拒绝执行内存分配\n");
                Close();
                return false;
            }
            if (blocksize > 128 * 1024 * 1024 || blocksize < 1024)//这里被改变成1K的单位，最后必须要改过来
            {
                //AfxMessageBox(_T("你输入的每块内存大小超过128M或者小于1K,系统拒绝执行内存分配"));
                //strcpy_s(infor,"你输入的每块内存大小超过64M或者小于1K，系统拒绝执行内存分配\n");
                Close();
                return false;
            }

            //分配连续多个内存块空间
            for (DWORD i = 0; i < menBlocknum; i++)
            {
                if (wdc_lib_decl.WDC_DMAContigBufLock(Handle, ref pWbuffer,   0x20, blocksize, ref ppwDma) != 0)
                {

                    //AfxMessageBox(_T("物理内存映射失败"));
                    //strcpy_s(infor,"物理内存映射失败\n");
                    //strcat(infor,Stat2Str(status));
                    pWbuffer = ppwDma = IntPtr.Zero;
                    Close();
                    return false;
                }
                //  object obj = m_wdcDeviceDma.MarshalNativeToManaged(ppwDma);
                //  memset(pWbuffer[i],0,blocksize);
                //   wdapi_dotnet.wdc_lib_macros.e
                // Array.Clear(pWbuffer[i], 0, blocksize);
                //pwPhysicalAddr[i] = ppwDma[index][i]->Page[0].pPhysicalAddr;
                //if(lpvBase[index][i])
                //	VirtualFree(lpvBase[index][i],0,MEM_RELEASE);
                //lpvBase[index][i] = VirtualAlloc(NULL,blocksize, MEM_RESERVE | MEM_TOP_DOWN | MEM_COMMIT, PAGE_READWRITE);
            }

            return true;
        }

        public bool ScanDMAWriteMenAlloc(DWORD blocksize)
        {
            if (wdc_lib_decl.WDC_DMAContigBufLock(Handle, ref pScanWbuffer, 0x20, blocksize, ref pScanpwDma) != 0)
            { 
                pScanWbuffer = pScanpwDma = IntPtr.Zero;
                Close();
                return false;
            }
            return true;
        }
        public DWORD WDC_DMAContigBufLock()
        {

            DWORD dwstatus = wdc_lib_decl.WDC_DMAContigBufLock(Handle, ref pReportWrBuffer, (uint)WD_DMA_OPTIONS.DMA_READ_FROM_DEVICE, 0x100, ref pReportWrDMA);

            return dwstatus;
        }
        public DWORD WDCScan_DMAContigBufLock()
        {

            DWORD dwstatus = wdc_lib_decl.WDC_DMAContigBufLock(Handle, ref pScanReportWrBuffer, (uint)WD_DMA_OPTIONS.DMA_READ_FROM_DEVICE, 0x100, ref pScanReportWrDMA);

            return dwstatus;
        }
        public bool StartWrDMA(DWORD index, HANDLE? hf = null, HANDLE? finishEven = null, BOOL bDisc = true)
        {
            DWORD reg28 = 0;
            HANDLE hr = IntPtr.Zero;

            hev = finishEven;//挂上通知完成传输的事件句柄
            hfile1 = hf;


            if (!IntEnable(index, this.m_wdcDevice))
            {
                //关闭Windriver处理
                Close();
                return false;

            }

            return true;
        }
        public bool IntEnable(DWORD index, WDC_DEVICE funcIntHandler)
        {

            DWORD dwStatus;

            //WDC_ADDR_DESC *pAddrDesc = NULL;
            // var S =wdapi_dotnet.wdc_lib_decl.WDC  WDC_GetDevContext(pDev);
            //AfxMessageBox("IntEnable");
            /* Check if interrupts are already enabled */
            if (wdc_lib_decl.WDC_IntIsEnabled(funcIntHandler.hDev))
                return true;
            /////////5.10前版//////////////    #define NUM_TRANS_CMDS 1


            /////////////////////////////5.10前版本/////////////////////////////////////////
            //   pTrans = (WD_TRANSFER*)calloc(NUM_TRANS_CMDS, sizeof(WD_TRANSFER));
            //   if (!pTrans)
            //{
            //	strcpy_s(infor,"IntEnable函数中pTrans内存分配失败\n");
            //       return FALSE; 
            //}
            //   pAddrDesc = &pDev->pAddrDesc[0]; //BAR0 
            //   /* Define the number of interrupt transfer commands to use */
            //pTrans[0].dwPort           =   pAddrDesc->kptAddr + 0x34;
            //pTrans[0].cmdTrans       =   WM_DWORD;
            //pTrans[0].Data.Dword    =   1;
            ////////////////////////////////////////////////////////////////////////
            // funcDiagIntHandler = funcIntHandler;
            /* Enable the interrupts */

            DWORD dwStatus1 = EnableInterrupts(new
                     USER_INTERRUPT_CALLBACK(PC1220X64_IntHandler), Handle);
            //DWORD dwStatus2= wdc_lib_decl.WDC_IntEnable(m_wdcDevice, pTrans, 0, 2, FuncIntHandler,m_wdcDevice. pDev, false);
            if (dwStatus1 == 1)
            {
                //strcpy_s(infor, "中断处理函数注册失败\n");
                //strcat_s(infor, Stat2Str(dwStatus));
                return false;
            }
            return true;
        }
        public bool FPGAReset(DWORD index)
        {
            DWORD reg44 = 0, countt = 0;
            //WriteBAR0(0x40,0x80000000);
            WriteBAR0(index, 0x00, 0x1);
            Thread.Sleep(1);
            //WriteBAR0(0x40,0x00000000);
            WriteBAR0(index, 0x00, 0x0);
            Thread.Sleep(1);
            return true;
        }
        public  bool ReadBAR0(DWORD index, UINT64 offset, ref DWORD outdata)
        {
            DWORD dw;
            if (index >= 4)
            {
                //strcpy_s(infor,"不支持4块及以上板卡");
                return false;
            } 
            if (wdc_lib_decl.WDC_ReadAddr32(Handle, 0, offset, ref outdata) != 0)
            {
                //strcpy_s(infor,"读取BAR0空间错误\n");
                //strcat(infor,Stat2Str(dw));
                return false;
            }
            return true;        
        }
        public bool WriteBAR0(DWORD index, UINT64 offset, DWORD indata)
        {
            if (Handle==IntPtr.Zero)
            {
                return false;
            }
            var result = wdc_lib_decl.WDC_WriteAddr32(Handle, 0, offset, indata);
            if (result != 0)
            {
                //strcpy_s(infor,"写BAR0空间错误\n");
                //strcat(infor,Stat2Str(dw));
                return false;
            }
            return true;
        }
        private void PC1220X64_IntHandler(PCIE_Device device)
        {
           
        }
    }

}