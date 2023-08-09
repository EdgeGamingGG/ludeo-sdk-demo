using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIView_Logger : MonoBehaviour
{
    public TMP_Text LogPrefab;
    public RectTransform Content;
    public int size = 11;

    private Queue<TMP_Text> _logQueue;

    private void Awake()
    {
        _logQueue = new Queue<TMP_Text>(size);
    }

    public void QueueLog(string message)
    {
        if (_logQueue.Count >= size) 
            Destroy(_logQueue.Dequeue());

        var log = Instantiate(LogPrefab, Content);
        log.text = message;
        log.transform.SetAsFirstSibling();
        _logQueue.Enqueue(log);
    }
}
