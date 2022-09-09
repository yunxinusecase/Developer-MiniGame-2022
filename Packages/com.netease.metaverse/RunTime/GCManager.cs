using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Profiling;

namespace MetaVerse.FrameWork
{
    public class GCManager : SingletonMonoBehaviour<GCManager>
    {
        // private BootConfigDebug mConfig = null;
        private bool mUseManualGC = false;
        [Header("打印内存使用情况Min")]
        public float mPrintMemoryTimeSlice = 60;
        public void InitGCManager(bool useManualGC, int incrementalMemSizeMB, int fullGCMemSizeMB)
        {
            // mConfig = AvatarAPPConfig.GetBootConfig;
            YXUnityLog.LogInfo(TAG, "useManualGC: " + useManualGC);
            YXUnityLog.LogInfo(TAG, "incrementalMemSizeMB: " + incrementalMemSizeMB);
            YXUnityLog.LogInfo(TAG, "fullGCMemSizeMB: " + fullGCMemSizeMB);
            YXUnityLog.LogInfo(TAG, "isIncremental: " + GarbageCollector.isIncremental);
            YXUnityLog.LogInfo(TAG, "GCMode: " + GarbageCollector.GCMode);

            incrementalMemSizeMB = incrementalMemSizeMB * 1024 * 1024;
            fullGCMemSizeMB = fullGCMemSizeMB * 1024 * 1024;
            if (useManualGC)
            {
#if !UNITY_EDITOR && UNITY_2020_1_OR_NEWER
                GarbageCollector.GCMode = GarbageCollector.Mode.Manual;
                mUseManualGC = true;
                YXUnityLog.LogInfo(TAG, "Use Manual GC Mode");
#else
                YXUnityLog.LogInfo(TAG, "UnSupport Use Manual GC Mode");
#endif
            }
        }

        private long incrementalMemSizeMB;
        private long fullGCMemSizeMB;

        private long lastFrameMemory = 0;
        private long nextCollectAt = 0;

        private string TAG = "GCManager";

        private long l11 = 0;
        private uint l22 = 0;
        private long l33 = 0;
        private long l44 = 0;
        private long l55 = 0;
        private int l66 = 0;
        private long l77 = 0;

        private float mLastTime = 0f;
        private float mNextGetMemTime = 0f;

        private void GetMemory()
        {
            if (Time.time - mLastTime >= mNextGetMemTime)
            {
                mNextGetMemTime = mPrintMemoryTimeSlice * 60;
                mLastTime = Time.time;

                // >Gets the allocated managed memory for live objects and non-collected objects.
                long l1 = Profiler.GetMonoUsedSizeLong();

                uint l2 = Profiler.GetTempAllocatorSize();
                long l3 = Profiler.GetTotalAllocatedMemoryLong();
                long l4 = Profiler.GetTotalReservedMemoryLong();
                long l5 = Profiler.GetTotalUnusedReservedMemoryLong();
                int l6 = Profiler.maxUsedMemory;

                // Returns the size of the reserved space for managed-memory.
                long l7 = Profiler.GetMonoHeapSizeLong();


                long ml1 = l1 - l11;
                long ml2 = l2 - l22;
                long ml3 = l3 - l33;
                long ml4 = l4 - l44;
                long ml5 = l5 - l55;
                long ml6 = l6 - l66;
                long ml7 = l7 - l77;

                l11 = l1;
                l22 = l2;
                l33 = l3;
                l44 = l4;
                l55 = l5;
                l66 = l6;
                l77 = l7;

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("l1 GetMonoUsedSizeLong:              {0,10}KB  {1,10}MB   {2,10}KB {3,10}MB\n", l1 / 1024.0f, l1 / 1024.0f / 1024.0f, ml1 / 1024.0f, ml1 / 1024.0f / 1024.0f);
                sb.AppendFormat("l2 GetTempAllocatorSize:             {0,10}KB  {1,10}MB   {2,10}KB {3,10}MB\n", l2 / 1024.0f, l2 / 1024.0f / 1024.0f, ml2 / 1024.0f, ml2 / 1024.0f / 1024.0f);
                sb.AppendFormat("l3 GetTotalAllocatedMemoryLong:      {0,10}KB  {1,10}MB   {2,10}KB {3,10}MB\n", l3 / 1024.0f, l3 / 1024.0f / 1024.0f, ml3 / 1024.0f, ml3 / 1024.0f / 1024.0f);
                sb.AppendFormat("l4 GetTotalReservedMemoryLong:       {0,10}KB  {1,10}MB   {2,10}KB {3,10}MB\n", l4 / 1024.0f, l4 / 1024.0f / 1024.0f, ml4 / 1024.0f, ml4 / 1024.0f / 1024.0f);
                sb.AppendFormat("l5 GetTotalUnusedReservedMemoryLong: {0,10}KB  {1,10}MB   {2,10}KB {3,10}MB\n", l5 / 1024.0f, l5 / 1024.0f / 1024.0f, ml5 / 1024.0f, ml5 / 1024.0f / 1024.0f);
                sb.AppendFormat("l6 maxUsedMemory:                    {0,10}KB  {1,10}MB   {2,10}KB {3,10}MB\n", l6 / 1024.0f, l6 / 1024.0f / 1024.0f, ml6 / 1024.0f, ml6 / 1024.0f / 1024.0f);
                sb.AppendFormat("l7 GetMonoHeapSizeLong:              {0,10}KB  {1,10}MB   {2,10}KB {3,10}MB\n", l7 / 1024.0f, l7 / 1024.0f / 1024.0f, ml7 / 1024.0f, ml7 / 1024.0f / 1024.0f);
                // Debug.Log("MEM-----------------------\n" + sb.ToString());
                YXUnityLog.LogInfo(TAG, $"MEM-----------------------\n{sb.ToString()}");
            }
        }

        void Update()
        {
            GetMemory();
            if (mUseManualGC)
            {
                long mem = Profiler.GetMonoUsedSizeLong();
                if (mem < lastFrameMemory)
                {
                    // GC happened.
                    nextCollectAt = mem + incrementalMemSizeMB;
                    YXUnityLog.LogInfo(TAG, $"GC Happened MonoUsedSizeLong: {mem.ToString()} NextCollectAt: {nextCollectAt.ToString()}");
                }
                else if (mem > fullGCMemSizeMB)
                {
                    // Trigger immediate GC
                    System.GC.Collect(0);
                    YXUnityLog.LogInfo(TAG, $"Make Full GC MonoUsedSizeLong: {mem.ToString()}");
                }
                else if (mem >= nextCollectAt)
                {
                    // Trigger incremental GC
                    UnityEngine.Scripting.GarbageCollector.CollectIncremental(0);
                    lastFrameMemory = mem + incrementalMemSizeMB;
                    YXUnityLog.LogInfo(TAG, $"Make Incremental GC MonoUsedSizeLong: {mem.ToString()}");
                }
                lastFrameMemory = mem;
            }
        }
    }

}
