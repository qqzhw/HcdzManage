using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jungo.wdapi_dotnet;
using wdc_err = Jungo.wdapi_dotnet.WD_ERROR_CODES;
using DWORD = System.UInt32;
using BOOL = System.Boolean;
using WDC_DRV_OPEN_OPTIONS = System.UInt32;
using System.Collections;

namespace Hcdz.PcieLib
{
    public class PCIE_DeviceList : ArrayList
    {
        private string NEWAMD86_DEFAULT_LICENSE_STRING = "12345abcde12345.abcde";
        // TODO: If you have renamed the WinDriver kernel module (windrvr6.sys),
        //  change the driver name below accordingly
        private string NEWAMD86_DEFAULT_DRIVER_NAME = "windrvr6";
        private DWORD NEWAMD86_DEFAULT_VENDOR_ID = 0x10EE;
        private DWORD NEWAMD86_DEFAULT_DEVICE_ID = 0x7028;

        private static PCIE_DeviceList instance;

        public static PCIE_DeviceList TheDeviceList()
        {
            if (instance == null)
            {
                instance = new PCIE_DeviceList();
            }
            return instance;
        }

        private PCIE_DeviceList() { }

        public DWORD Init()
        {
            if (windrvr_decl.WD_DriverName(NEWAMD86_DEFAULT_DRIVER_NAME) == null)
            {
                Log.ErrLog("NEWAMD86_DeviceList.Init: Failed to set driver name for the " +
                    "WDC library.");
                return (DWORD)wdc_err.WD_SYSTEM_INTERNAL_ERROR;
            }

            DWORD dwStatus = wdc_lib_decl.WDC_SetDebugOptions(wdc_lib_consts.WDC_DBG_DEFAULT,
                null);
            if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
            {
                Log.ErrLog("NEWAMD86_DeviceList.Init: Failed to initialize debug options for the " +
                    "WDC library. Error 0x" + dwStatus.ToString("X") +
                    utils.Stat2Str(dwStatus));
                return dwStatus;
            }

            dwStatus = wdc_lib_decl.WDC_DriverOpen(
                (WDC_DRV_OPEN_OPTIONS)wdc_lib_consts.WDC_DRV_OPEN_DEFAULT,
                NEWAMD86_DEFAULT_LICENSE_STRING);
            if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
            {
                Log.ErrLog("NEWAMD86_DeviceList.Init: Failed to initialize the WDC library. "
                    + "Error 0x" + dwStatus.ToString("X") + utils.Stat2Str(dwStatus));
                return dwStatus;
            }
            return Populate();
        }

        public PCIE_Device Get(int index)
        {
            if (index >= this.Count || index < 0)
                return null;
            return (PCIE_Device)this[index];
        }

        public PCIE_Device Get(WD_PCI_SLOT slot)
        {
            foreach (PCIE_Device device in this)
            {
                if (device.IsMySlot(ref slot))
                    return device;
            }
            return null;
        }

        private DWORD Populate()
        {
            DWORD dwStatus;
            WDC_PCI_SCAN_RESULT scanResult = new WDC_PCI_SCAN_RESULT();

            dwStatus = wdc_lib_decl.WDC_PciScanDevices(NEWAMD86_DEFAULT_VENDOR_ID,
                NEWAMD86_DEFAULT_DEVICE_ID, scanResult);

            if ((DWORD)wdc_err.WD_STATUS_SUCCESS != dwStatus)
            {
                Log.ErrLog("NEWAMD86_DeviceList.Populate: Failed scanning "
                    + "the PCI bus. Error 0x" + dwStatus.ToString("X") +
                    utils.Stat2Str(dwStatus));
                return dwStatus;
            }

            if (scanResult.dwNumDevices == 0)
            {
                Log.ErrLog("NEWAMD86_DeviceList.Populate: No matching PCI " +
                    "device was found for search criteria " + NEWAMD86_DEFAULT_VENDOR_ID.ToString("X")
                    + ", " + NEWAMD86_DEFAULT_DEVICE_ID.ToString("X"));
                return (DWORD)wdc_err.WD_INVALID_PARAMETER;
            }

            for (int i = 0; i < scanResult.dwNumDevices; ++i)
            {
                PCIE_Device device;
                WD_PCI_SLOT slot = scanResult.deviceSlot[i];

                device = new PCIE_Device(scanResult.deviceId[i].dwVendorId,
                    scanResult.deviceId[i].dwDeviceId, slot);

                this.Add(device);
            }
            return (DWORD)wdc_err.WD_STATUS_SUCCESS;
        }

        public void Dispose()
        {
            foreach (PCIE_Device device in this)
                device.Dispose();
            this.Clear();

            DWORD dwStatus = wdc_lib_decl.WDC_DriverClose();
            if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
            {
                Exception excp = new Exception("NEWAMD86_DeviceList.Dispose: " +
                    "Failed to uninit the WDC library. Error 0x" +
                    dwStatus.ToString("X") + utils.Stat2Str(dwStatus));
                throw excp;
            }
        }
    };
}

