using System;
using UnityEngine;

public class SimpleCallbackComponent
{
    public void Open(Action onFinished)
    {
    }

    public Awaiter OpenAwait()
    {
        var ret = new Awaiter();
        Open(() => { ret.SetResult(); });
        return ret;
    }
}

public class CallbacksComponent
{
    public void Open(Action onOpened,Action<int> onClosed)
    {
    }

    public Tuple<Awaiter, Awaiter<int>> OpenAwait()
    {
        var opened = new Awaiter();
        var closed = new Awaiter<int>();
        Open(
            () => { opened.SetResult(); }, 
            (result) => { closed.SetResult(result); });
        return Tuple.Create(opened, closed);
    }
}


public class AwaiterExample 
{
    public void TestCallbackComponent()
    {
        var simple = new SimpleCallbackComponent();
        simple.Open(() =>
        {
            Debug.Log("1");
            var simple2 = new SimpleCallbackComponent();
            simple2.Open(() =>
            {
                Debug.Log("2");
                var simple3 = new SimpleCallbackComponent();
                simple3.Open(() =>
                {
                    Debug.Log("3");
                });
            });
        });
    }
    
    public async void TestCallbackComponentAwait()
    {
        var simple = new SimpleCallbackComponent();
        await simple.OpenAwait();
        Debug.Log("1");
        
        var simple2 = new SimpleCallbackComponent();
        await simple2.OpenAwait();
        Debug.Log("2");
        
        var simple3 = new SimpleCallbackComponent();
        await simple3.OpenAwait();
        Debug.Log("3");
    }
}

public class AwaiterExample2 
{
    public void TestCallbacksComponent()
    {
        var simple = new CallbacksComponent();
        simple.Open(
            () => { Debug.Log("Opened 1"); },
            (result) =>
        {
            Debug.Log("Closed 1");
            
            var simple2 = new CallbacksComponent();
            simple2.Open(
                () => { },
                (result2) =>
                {
                    Debug.Log($"Closed 2: {result2}");
                    var simple3 = new CallbacksComponent();
                    simple3.Open(
                        () => { Debug.Log("Opened 3"); }, 
                        (result3) => { });
                });
        });
    }
    
    public async void TestCallbacksComponentAwait()
    {
        var simple = new CallbacksComponent();
        var (opened, closed) = simple.OpenAwait();
        await opened;
        Debug.Log("Opened 1");
        // var result = await closed;
        await closed;
        Debug.Log("Closed 1");
            
        var simple2 = new CallbacksComponent();
        //(opened, closed) = simple2.OpenAwait();
        (_, closed) = simple2.OpenAwait();
        // await opened;
        var result2 = await closed;
        Debug.Log($"Closed 2: {result2}");
        
        
        var simple3 = new CallbacksComponent();
        (opened,_) = simple3.OpenAwait();
        await opened;
        Debug.Log("Opened 3");
    }
}
