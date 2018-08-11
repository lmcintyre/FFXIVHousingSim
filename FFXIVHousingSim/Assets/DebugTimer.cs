using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class DebugTimer
{
    Stopwatch sw = new Stopwatch();
    
    public DebugTimer()
    {
        
    }

    public void startInitial()
    {
        sw.Start();
    }

    public void registerEvent(String eventName)
    {
        Debug.LogFormat("Event {0}: took {1} ms.", eventName, sw.ElapsedMilliseconds);
        sw.Reset();
        sw.Start();
    }
    
}