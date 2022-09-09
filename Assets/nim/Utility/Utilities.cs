using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NimUtility
{
    public class Utilities
    {
        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public static T IntPtrToStructure<T>(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return default(T);
            return (T)System.Runtime.InteropServices.Marshal.PtrToStructure(ptr, typeof(T));
        }

        public static IntPtr StructureToIntPtr(object obj)
        {
            int nSizeOfParam = System.Runtime.InteropServices.Marshal.SizeOf(obj);
            IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(nSizeOfParam);
            return ptr;
        }

        public static string MarshalUtf8String(IntPtr ptr)
        {
            Utf8StringMarshaler marshaler = new Utf8StringMarshaler();
            var obj = marshaler.MarshalNativeToManaged(ptr);
            return obj as string;
        }
    }

    public class DelegateBaton<T>
    {
        public object Data { get; set; }

        public T Action { get; set; }

        public IntPtr ToIntPtr()
        {
            var ptr = DelegateConverter.ConvertToIntPtr(this);
            return ptr;
        }

        public static DelegateBaton<T> FromIntPtr(IntPtr ptr)
        {
            var obj = DelegateConverter.ConvertFromIntPtr(ptr);
            var baton = obj as DelegateBaton<T>;
            return baton;
        }
    }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID

    public class AotTypes : UnityEngine.MonoBehaviour
    {
        private static System.ComponentModel.TypeConverter _unused = new System.ComponentModel.TypeConverter();
    }
#endif

}
