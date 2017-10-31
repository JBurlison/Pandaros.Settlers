//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.Emit;
//using System.Runtime.CompilerServices;
//using System.Text;

//namespace Pandaros.Settlers
//{
//    public static class MethodUtil
//    {
//        /// <summary>
//        /// Replaces the method.
//        /// </summary>
//        /// <param name="source">The source.</param>
//        /// <param name="dest">The dest.</param>
//        public static void ReplaceMethod(MethodBase source, MethodBase dest)
//        {
//            if (!MethodSignaturesEqual(source, dest))
//            {
//                throw new ArgumentException("The method signatures are not the same.", "source");
//            }
//            ReplaceMethod(GetMethodAddress(source), dest);
//        }

//        /// <summary>
//        /// Replaces the method.
//        /// </summary>
//        /// <param name="srcAdr">The SRC adr.</param>
//        /// <param name="dest">The dest.</param>
//        public static void ReplaceMethod(IntPtr srcAdr, MethodBase dest)
//        {
//            IntPtr destAdr = GetMethodAddress(dest);
//            unsafe
//            {
//                if (IntPtr.Size == 8)
//                {
//                    ulong* d = (ulong*)destAdr.ToPointer();
//                    *d = *((ulong*)srcAdr.ToPointer());
//                }
//                else
//                {
//                    uint* d = (uint*)destAdr.ToPointer();
//                    *d = *((uint*)srcAdr.ToPointer());
//                }
//            }
//        }

//        /// <summary>
//        /// Gets the address of the method stub
//        /// </summary>
//        /// <param name="methodHandle">The method handle.</param>
//        /// <returns></returns>
//        public static IntPtr GetMethodAddress(MethodBase method)
//        {
//            if ((method is DynamicMethod))
//            {
//                return GetDynamicMethodAddress(method);
//            }

//            // Prepare the method so it gets jited
//            RuntimeHelpers.PrepareMethod(method.MethodHandle);

//            // If 3.5 sp1 or greater than we have a different layout in memory.

//            return GetMethodAddress20SP2(method);
//        }

//        private static IntPtr GetDynamicMethodAddress(MethodBase method)
//        {
//            unsafe
//            {
//                RuntimeMethodHandle handle = GetDynamicMethodRuntimeHandle(method);
//                byte* ptr = (byte*)handle.Value.ToPointer();
//                RuntimeHelpers.PrepareMethod(handle);

//                if (IntPtr.Size == 8)
//                {
//                    ulong* address = (ulong*)ptr;
//                    address = (ulong*)*(address + 5);
//                    return new IntPtr(address + 12);
//                }
//                else
//                {
//                    uint* address = (uint*)ptr;
//                    address = (uint*)*(address + 5);
//                    return new IntPtr(address + 12);
//                }
//            }
//        }
//        private static RuntimeMethodHandle GetDynamicMethodRuntimeHandle(MethodBase method)
//        {
//            if (method is DynamicMethod)
//            {
//                FieldInfo fieldInfo = typeof(DynamicMethod).GetField("m_method", BindingFlags.NonPublic | BindingFlags.Instance);
//                RuntimeMethodHandle handle = ((RuntimeMethodHandle)fieldInfo.GetValue(method));

//                return handle;
//            }
//            return method.MethodHandle;
//        }
//        private static IntPtr GetMethodAddress20SP2(MethodBase method)
//        {
//            unsafe
//            {
//                return new IntPtr(((int*)method.MethodHandle.Value.ToPointer() + 2));
//            }
//        }
//        private static bool MethodSignaturesEqual(MethodBase x, MethodBase y)
//        {
//            if (x.CallingConvention != y.CallingConvention)
//            {
//                return false;
//            }
//            Type returnX = GetMethodReturnType(x), returnY = GetMethodReturnType(y);
//            if (returnX != returnY)
//            {
//                return false;
//            }
//            ParameterInfo[] xParams = x.GetParameters(), yParams = y.GetParameters();
//            if (xParams.Length != yParams.Length)
//            {
//                return false;
//            }
//            for (int i = 0; i < xParams.Length; i++)
//            {
//                if (xParams[i].ParameterType != yParams[i].ParameterType)
//                {
//                    return false;
//                }
//            }
//            return true;
//        }
//        private static Type GetMethodReturnType(MethodBase method)
//        {
//            MethodInfo methodInfo = method as MethodInfo;
//            if (methodInfo == null)
//            {
//                // Constructor info.
//                throw new ArgumentException("Unsupported MethodBase : " + method.GetType().Name, "method");
//            }
//            return methodInfo.ReturnType;
//        }
//    }
//}
