using System;
using System.Diagnostics;

namespace PIAdaptMRP
{
    internal static class CHere
    {
        internal static String FunctionName()
        {
            var stackFrames = new StackTrace(0, true).GetFrames();
            return stackFrames != null ? stackFrames[1].GetMethod().Name : null;
        }

        internal static String CallingFunctionName()
        {
            var stackFrames = new StackTrace(0, true).GetFrames();
            return stackFrames != null ? stackFrames[2].GetMethod().Name : null;
        }

        internal static String FileName()
        {
            var stackFrames = new StackTrace(0, true).GetFrames();
            return stackFrames != null ? stackFrames[1].GetFileName() : null;
        }

        internal static Int32 LineNumber()
        {
            var stackFrames = new StackTrace(0, true).GetFrames();
            return stackFrames != null ? stackFrames[1].GetFileLineNumber() : 0;
        }

        internal static Int32 ColumnNumber()
        {
            var stackFrames = new StackTrace(0, true).GetFrames();
            return stackFrames != null ? stackFrames[1].GetFileColumnNumber() : 0;
        }

        internal static String Module()
        {
            var stackFrames = new StackTrace(0, true).GetFrames();
            return stackFrames != null ? stackFrames[1].GetMethod().Module.FullyQualifiedName : null;
        }

        internal static String FullName()
        {
            var stackFrames = new StackTrace(0, true).GetFrames();
            if (stackFrames == null) return null;
            var declaringType = stackFrames[1].GetMethod().DeclaringType;
            return declaringType != null ? declaringType.FullName : null;
        }

        internal static String AssemblyFullName()
        {
            var stackFrames = new StackTrace(0, true).GetFrames();
            return stackFrames != null ? stackFrames[1].GetMethod().Module.Assembly.FullName : null;
        }
    }
}