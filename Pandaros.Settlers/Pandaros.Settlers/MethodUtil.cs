using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Pandaros.Settlers
{
    public static class MethodUtil
    {
        /// <summary>
        ///     Replaces the method.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        public static void ReplaceMethod(MethodBase source, MethodBase dest)
        {
            if (!MethodSignaturesEqual(source, dest))
                throw new ArgumentException("The method signatures are not the same.", "source");

            ReplaceMethod(GetMethodAddress(source), dest);
        }

        /// <summary>
        ///     Replaces the method.
        /// </summary>
        /// <param name="srcAdr">The SRC adr.</param>
        /// <param name="dest">The dest.</param>
        public static void ReplaceMethod(IntPtr srcAdr, MethodBase dest)
        {
            var destAdr = GetMethodAddress(dest);

            unsafe
            {
                if (IntPtr.Size == 8)
                {
                    var d = (ulong*) destAdr.ToPointer();
                    *d = *((ulong*) srcAdr.ToPointer());
                }
                else
                {
                    var d = (uint*) destAdr.ToPointer();
                    *d = *((uint*) srcAdr.ToPointer());
                }
            }
        }

        /// <summary>
        ///     Gets the address of the method stub
        /// </summary>
        /// <param name="methodHandle">The method handle.</param>
        /// <returns></returns>
        public static IntPtr GetMethodAddress(MethodBase method)
        {
            if (method is DynamicMethod) return GetDynamicMethodAddress(method);

            // Prepare the method so it gets jited
            RuntimeHelpers.PrepareMethod(method.MethodHandle);

            // If 3.5 sp1 or greater than we have a different layout in memory.

            return GetMethodAddress20SP2(method);
        }

        private static IntPtr GetDynamicMethodAddress(MethodBase method)
        {
            unsafe
            {
                var handle = GetDynamicMethodRuntimeHandle(method);
                var ptr    = (byte*) handle.Value.ToPointer();
                RuntimeHelpers.PrepareMethod(handle);

                if (IntPtr.Size == 8)
                {
                    var address = (ulong*) ptr;
                    address = (ulong*) *(address + 5);
                    return new IntPtr(address + 12);
                }
                else
                {
                    var address = (uint*) ptr;
                    address = (uint*) *(address + 5);
                    return new IntPtr(address + 12);
                }
            }
        }

        private static RuntimeMethodHandle GetDynamicMethodRuntimeHandle(MethodBase method)
        {
            if (method is DynamicMethod)
            {
                var fieldInfo =
                    typeof(DynamicMethod).GetField("m_method", BindingFlags.NonPublic | BindingFlags.Instance);

                var handle = (RuntimeMethodHandle) fieldInfo.GetValue(method);

                return handle;
            }

            return method.MethodHandle;
        }

        private static IntPtr GetMethodAddress20SP2(MethodBase method)
        {
            unsafe
            {
                return new IntPtr((int*) method.MethodHandle.Value.ToPointer() + 2);
            }
        }

        private static bool MethodSignaturesEqual(MethodBase x, MethodBase y)
        {
            if (x.CallingConvention != y.CallingConvention) return false;
            Type returnX = GetMethodReturnType(x), returnY = GetMethodReturnType(y);
            if (returnX != returnY) return false;
            ParameterInfo[] xParams = x.GetParameters(), yParams = y.GetParameters();
            if (xParams.Length != yParams.Length) return false;

            for (var i = 0; i < xParams.Length; i++)
                if (xParams[i].ParameterType != yParams[i].ParameterType)
                    return false;

            return true;
        }

        private static Type GetMethodReturnType(MethodBase method)
        {
            var methodInfo = method as MethodInfo;

            if (methodInfo == null)
                throw new ArgumentException("Unsupported MethodBase : " + method.GetType().Name, "method");

            return methodInfo.ReturnType;
        }
    }
}