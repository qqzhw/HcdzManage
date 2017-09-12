using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;  
using Jungo.wdapi_dotnet;
using DWORD = System.UInt32;

namespace Hcdz.PcieLib
{
    public struct WDC_REG
    {
        public DWORD dwAddrSpace;       /* Number of address space in which the register resides */
                                        /* For PCI configuration registers, use WDC_AD_CFG_SPACE */
        public DWORD dwOffset;          /* Offset of the register in the dwAddrSpace address space */
        public DWORD dwSize;            /* Register's size (in bytes) */
        public WDC_DIRECTION direction; /* Read/write access mode - see WDC_DIRECTION options */
        public string sName;           /* Register's name */
        public string sDesc;           /* Register's description */

        public WDC_REG(DWORD _dwAddrSpace, DWORD _dwOffset, DWORD _dwSize,
            WDC_DIRECTION _direction, string _sName, string _sDesc)
        {
            dwAddrSpace = _dwAddrSpace;
            dwOffset = _dwOffset;
            dwSize = _dwSize;
            direction = _direction;
            sName = _sName;
            sDesc = _sDesc;
        }
    };
    public class PCIE_Regs
    {
        private const uint WDC_AD_CFG_SPACE = 0xFF;
        public readonly WDC_REG[] gNEWAMD86_CfgRegs = new WDC_REG[]
        {
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_VID, wdc_lib_consts.WDC_SIZE_16, WDC_DIRECTION.WDC_READ_WRITE, "VID", "Vendor ID" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_DID, wdc_lib_consts.WDC_SIZE_16, WDC_DIRECTION.WDC_READ_WRITE, "DID", "Device ID" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_CR, wdc_lib_consts.WDC_SIZE_16, WDC_DIRECTION.WDC_READ_WRITE, "CMD", "Command" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_SR, wdc_lib_consts.WDC_SIZE_16, WDC_DIRECTION.WDC_READ_WRITE, "STS", "Status" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_REV, wdc_lib_consts.WDC_SIZE_32, WDC_DIRECTION.WDC_READ_WRITE, "RID_CLCD", "Revision ID & Class Code" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_CCSC, wdc_lib_consts.WDC_SIZE_8, WDC_DIRECTION.WDC_READ_WRITE, "SCC", "Sub Class Code" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_CCBC, wdc_lib_consts.WDC_SIZE_8, WDC_DIRECTION.WDC_READ_WRITE, "BCC", "Base Class Code" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_CLSR, wdc_lib_consts.WDC_SIZE_8, WDC_DIRECTION.WDC_READ_WRITE, "CALN", "Cache Line Size" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_LTR, wdc_lib_consts.WDC_SIZE_8, WDC_DIRECTION.WDC_READ_WRITE, "LAT", "Latency Timer" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_HDR, wdc_lib_consts.WDC_SIZE_8, WDC_DIRECTION.WDC_READ_WRITE, "HDR", "Header Type" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_BISTR, wdc_lib_consts.WDC_SIZE_8, WDC_DIRECTION.WDC_READ_WRITE, "BIST", "Built-in Self Test" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_BAR0, wdc_lib_consts.WDC_SIZE_32, WDC_DIRECTION.WDC_READ_WRITE, "BADDR0", "Base Address 0" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_BAR1, wdc_lib_consts.WDC_SIZE_32, WDC_DIRECTION.WDC_READ_WRITE, "BADDR1", "Base Address 1" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_BAR2, wdc_lib_consts.WDC_SIZE_32, WDC_DIRECTION.WDC_READ_WRITE, "BADDR2", "Base Address 2" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_BAR3, wdc_lib_consts.WDC_SIZE_32, WDC_DIRECTION.WDC_READ_WRITE, "BADDR3", "Base Address 3" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_BAR4, wdc_lib_consts.WDC_SIZE_32, WDC_DIRECTION.WDC_READ_WRITE, "BADDR4", "Base Address 4" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_BAR5, wdc_lib_consts.WDC_SIZE_32, WDC_DIRECTION.WDC_READ_WRITE, "BADDR5", "Base Address 5" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_CIS, wdc_lib_consts.WDC_SIZE_32, WDC_DIRECTION.WDC_READ_WRITE, "CIS", "CardBus CIS pointer" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_SVID, wdc_lib_consts.WDC_SIZE_16, WDC_DIRECTION.WDC_READ_WRITE, "SVID", "Sub-system Vendor ID" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_SDID, wdc_lib_consts.WDC_SIZE_16, WDC_DIRECTION.WDC_READ_WRITE, "SDID", "Sub-system Device ID" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_EROM, wdc_lib_consts.WDC_SIZE_32, WDC_DIRECTION.WDC_READ_WRITE, "EROM", "Expansion ROM Base Address" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_CAP, wdc_lib_consts.WDC_SIZE_8, WDC_DIRECTION.WDC_READ_WRITE, "NEW_CAP", "New Capabilities Pointer" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_ILR, wdc_lib_consts.WDC_SIZE_32, WDC_DIRECTION.WDC_READ_WRITE, "INTLN", "Interrupt Line" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_IPR, wdc_lib_consts.WDC_SIZE_32, WDC_DIRECTION.WDC_READ_WRITE, "INTPIN", "Interrupt Pin" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_MGR, wdc_lib_consts.WDC_SIZE_32, WDC_DIRECTION.WDC_READ_WRITE, "MINGNT", "Minimum Required Burst Period" ),
            new WDC_REG(WDC_AD_CFG_SPACE, (DWORD)PCI_CFG_REG.PCI_MLR, wdc_lib_consts.WDC_SIZE_32, WDC_DIRECTION.WDC_READ_WRITE, "MAXLAT", "Maximum Latency" )
        };

        /* run-time registers information array */
        public readonly WDC_REG[] gNEWAMD86_RT_Regs = { };
    };

}
