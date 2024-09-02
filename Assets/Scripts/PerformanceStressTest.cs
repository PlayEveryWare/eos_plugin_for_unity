/*
* Copyright (c) 2021 PlayEveryWare
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    using System.Collections;
    using System.Threading;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System;
    using UnityEngine.UI;
    using Unity.Jobs;
    using UnityEngine;

    public class GaseousArray
    {
        //struct of size 4k
        public class Databomb
        {
            long[] data;

            public Databomb()
            {
                data = new long[512];
                for (int i = 0; i < 512; i++)
                {
                    data[i] = 1;
                }
            }
        }

        List<List<Databomb>> primaryContainer;

        public GaseousArray()
        {
            primaryContainer = new List<List<Databomb>>();
        }

        public void grow(int goalGB)
        {
            ulong target = ((ulong)1073741824 * (ulong)goalGB);
            ulong counter = 0;
            bool xFlag = true;
            UnityEngine.Debug.Log("Starting memory test allocation with a target of: " + target + " Bytes or: " +
                                  goalGB + "GB");
            //this loop manages everything, this flag will be triggered when the program can allocate anything at all
            while (xFlag)
            {
                List<Databomb> temp = null;
                //try creating a small array if this fails there are no more spaces available
                //for small objects and we are probably at or close enough to the system limit
                try
                {
                    temp = new List<Databomb>();
                }
                catch (OutOfMemoryException)
                {
                    xFlag = false;
                }

                //keep adding to the secondary array until the allocation fails
                bool yFlag = true;
                while (yFlag && xFlag)
                {
                    try
                    {
                        temp.Add(new Databomb());
                        counter += 8192 + 8;
                        if (counter >= target)
                        {
                            UnityEngine.Debug.Log("Allocated: " + counter + " Bytes or: " +
                                                  counter / (ulong)1073741824 + "GB");
                            xFlag = false;
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        //counter -= 4096 + 8;
                        yFlag = false;
                    }
                }
            }
        }

        ~GaseousArray()
        {
            UnityEngine.Debug.Log("Destroying gaseous array");
            for (int i = 0; i < primaryContainer.Count; i++)
            {
                primaryContainer[i].Clear();
            }

            primaryContainer.Clear();
        }
    }

    public class PerformanceStressTest : MonoBehaviour
    {
        public class UISliderToText : MonoBehaviour
        {
            public Slider sliderObject;
            public Text SliderText;

            public void updateText()
            {
                SliderText.text = (sliderObject.value.ToString());
            }
        }

        //CPU Test 
        int threads = 0;
        List<Thread> threadList;
        bool threadOverride = false;
        public Slider threadSlider;
        public Slider targetUtilizationSlider;
        private float targetUtilization;
        private UISliderToText _coreSliderScript;

        //GPU Test
        public GameObject gpuRenderObjects;

        //Memory Test
        public Slider memorySlider;
        Thread memoryThread;

        // Start is called before the first frame update
        void Start()
        {
            threads = SystemInfo.processorCount;
            UnityEngine.Debug.Log("Working with: " + threads + " threads");
            threadList = new List<Thread>();
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(targetUtilizationSlider.gameObject);
        }

        public void CPUTest()
        {
            UnityEngine.Debug.Log("CPU Thread started");
            long count = 0;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                //sleep for the desired time
                if (watch.ElapsedMilliseconds > targetUtilization)
                {
                    Thread.Sleep((int)(100 - targetUtilization));

                    watch.Reset();
                    watch.Start();
                }
                //work for the determined time to hit the target utilization
                else
                {
                    count++;
                    if (count > 10000000)
                    {
                        count = 0;
                    }
                }
            }
        }

        public void StartCPUTest()
        {
            int targetThreads = threads;
            if (!threadOverride)
            {
                targetThreads = (int)threadSlider.value;
            }

            //trim 5% off due to general program usage
            targetUtilization = targetUtilizationSlider.value - 5;

            UnityEngine.Debug.Log(
                "Starting: " + targetThreads + " threads with a target usage of: " + targetUtilization);
            for (int i = 0; i < targetThreads; i++)
            {
                threadList.Add(new Thread(new ThreadStart(CPUTest)));
                threadList[i].Start();
            }
        }

        public void StopCPUTest()
        {
            UnityEngine.Debug.Log("Stopping: " + threadList.Count + " threads");
            for (int i = 0; i < threadList.Count; i++)
            {
                threadList[i].Abort();
            }

            threadList.Clear();
        }

        public void ToggleOverride()
        {
            threadOverride = !threadOverride;
            if (threadOverride)
            {
                _coreSliderScript.SliderText.text = (threads.ToString());
            }
            else
            {
                _coreSliderScript.updateText();
            }
        }

        public void StartGPUTest()
        {
            UnityEngine.Debug.Log("Starting GPU Stress Test");
            gpuRenderObjects.SetActive(true);
        }

        public void StopGPUTest()
        {
            UnityEngine.Debug.Log("Stopping GPU Stress Test");
            gpuRenderObjects.SetActive(false);
        }

        private void memtest()
        {
            print("Memory thread spawned");
            GaseousArray gasArray;
            gasArray = new GaseousArray();
            gasArray.grow((int)memorySlider.value);
        }

        public void StartMemoryTest()
        {
            UnityEngine.Debug.Log("Starting Memory Stress Test");
            memoryThread = new Thread(new ThreadStart(memtest));
            memoryThread.Start();
        }

        public void StopMemoryTest()
        {
            memoryThread.Abort();
        }

    }
}