using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FixedSizedQueue<T> : Queue<T>
{
    private readonly object syncObject = new object();

    public int Size { get; private set; }

    public FixedSizedQueue(int size)
    {
        Size = size;
    }

    public override string ToString()
    {
        string ret = "";
        T[] queued = this.ToArray();

        foreach(T o in queued)
        {
            ret += o.ToString();
        }

        return ret;
    }

    public new void Enqueue(T obj)
    {
        base.Enqueue(obj);
        lock (syncObject)
        {
            while (base.Count > Size)
            {
                base.Dequeue();
            }
        }
    }
}

public class Logger : MonoBehaviour {

    public bool onScreen;
    public bool onFile;
    public string outputFile;

    public string errorColor = "red";
    public string logColor = "green";

    private FixedSizedQueue<String> onScreenLog;

    private Text textLog;

    void Start()
    {
        textLog = GetComponentInChildren<Text>();
        onScreenLog = new FixedSizedQueue<string>(5);
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        if (onScreen)
        {
            if (type == LogType.Error || type == LogType.Exception)
                onScreenLog.Enqueue("<color="+errorColor+">" + string.Format("{0} {1} \n {2}\n {3}\n\n", DateTime.Now, type, condition, stackTrace) + "</color>");
            else
                onScreenLog.Enqueue("<color="+logColor+">" + string.Format("{0} {1} \n {2}\n\n", DateTime.Now, type, condition) + "</color>");
        }

        if (onFile)
        {
            if (outputFile != "")
            {
                try
                {
                    string logEntry = "";

                    if (type == LogType.Error || type == LogType.Exception)
                        logEntry = string.Format("{0} {1} \n {2}\n {3}\n\n", DateTime.Now, type, condition, stackTrace);
                    else
                        logEntry = string.Format("{0} {1} \n {2}\n\n", DateTime.Now, type, condition);

                    File.AppendAllText(outputFile, logEntry);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            else
            {
                Debug.Log("You have to specify an output file for logging");
            }
        }
    }

    void OnGUI()
    {
        textLog.text = onScreenLog.ToString();
    }
}
