using System;
using System.Collections.Generic;
using System.Reflection;
using TPRandomizer.SSettings.Enums;

namespace TPRandomizer
{
    /// <summary>
    /// summary text.
    /// </summary>
    public class DataFunctions
    {
        public static int ASM_NOP()
        {
            return 0x60000000;
        }

        public static int ASM_LOAD_IMMEDIATE(int register, int value)
        {
            return ((0x38000000 + (register * 0x200000)) | (value & 0xFFFF));
        }

        public static int ASM_COMPARE_WORD_IMMEDIATE(int register, int value)
        {
            return ((0x2C000000 + (register * 0x10000)) | (value & 0xFFFF));
        }

        public static int ASM_COMPARE_LOGICAL_WORD_IMMEDIATE(int register, int value)
        {
            return ((0x28000000 + (register * 0x10000)) | (value & 0xFFFF));
        }

        public static int ASM_BRANCH(int length)
        {
            return (0x48000000 + (length & 0x3FFFFFC));
        }

        public static int ASM_BRANCH_LINK_REGISTER()
        {
            return 0x4E800020;
        }

        public static int ASM_BRANCH_CONDITIONAL(int branchOption, int conditionBit, int value)
        {
            return (
                0x40000000 + (branchOption * 0x200000) + (conditionBit * 0x10000) + (value & 0xFFFF)
            );
        }

        public static int ASM_BRANCH_EQUAL_MINUS(int value)
        {
            int tempValue = value & 0xFFFF;
            if (tempValue >= 0)
            {
                return ASM_BRANCH_CONDITIONAL(12, 2, tempValue);
            }
            else
            {
                return ASM_BRANCH_CONDITIONAL(13, 2, tempValue);
            }
        }
    }
}
