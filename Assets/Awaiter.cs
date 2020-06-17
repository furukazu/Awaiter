using System;
using System.Runtime.CompilerServices;

/// <summary>
/// 戻り値を持たない奴を await で待てるようにするためだけのクラス
/// </summary>
public class Awaiter: ICriticalNotifyCompletion
{
    public Awaiter GetAwaiter()
    {
        return this;
    }

    private bool _isCompleted;
    
    public bool IsCompleted => _isCompleted;

    public void GetResult()
    {
    }

    /// <summary>
    /// これを呼ぶと完了になるのでawaitの次を呼ぶ
    /// </summary>
    public void SetResult()
    {
        _isCompleted = true;

        // await の次の処理持ってたら呼ぶ。来てなかったらそのうちOnCompletedが呼ばれるはずなのでそこに任す
        if (_nextOperation != null)
        {
            // 既に await の次の処理を持っているのでそれを呼びに行く
            OnCompleted(_nextOperation);
        }
    }

    private Action _nextOperation;
        
    /// <summary>
    /// await の次の行からの処理が continuation に詰められて渡される
    /// </summary>
    /// <param name="continuation">awaitからの次の処理</param>
    public void OnCompleted(Action continuation)
    {
        // とりあえずメモっておく
        _nextOperation = continuation;

        // 既に完了してたのだとしたら即次へ
        if (IsCompleted)
        {
            // 次を呼ぶ
            _nextOperation();
            // 念のため消しておく
            _nextOperation = () => { };
        }
    }

    /// <summary>
    /// よく分からんけどOnCompletedに任す
    /// </summary>
    /// <param name="continuation"></param>
    public void UnsafeOnCompleted(Action continuation)
    {
        OnCompleted(continuation);
    }

}

/// <summary>
/// 戻り値を持つコールバックをawaitとして置き換えるための雑実装
/// 戻り値が複数になる場合はクラスに詰めるかTupleにしてください
/// </summary>
/// <typeparam name="T"></typeparam>
public class Awaiter<T>: ICriticalNotifyCompletion
{
    public Awaiter<T> GetAwaiter()
    {
        return this;
    }

    private bool _isCompleted;
    
    public bool IsCompleted => _isCompleted;

    private T _result;
    
    /// <summary>
    /// var res = await awaiter;
    /// みたいなところで呼ばれる奴
    /// </summary>
    /// <returns></returns>
    public T GetResult()
    {
        return _result;
    }

    /// <summary>
    /// GetResultで返す値をセットしつつ完了させる。
    /// 既にawaitの次の処理を手に入れていたらそれを呼び出す
    /// </summary>
    /// <param name="output"></param>
    public void SetResult(T output)
    {
        _result = output;
        _isCompleted = true;

        // 既に awit の次の処理を手に入れているのでそれを呼ぶ
        if (_nextOperation != null)
        {
            OnCompleted(_nextOperation);
        }
    }

    private Action _nextOperation;
        
    /// <summary>
    /// awaitの次の行からの実行処理を与えてくれる奴
    /// </summary>
    /// <param name="continuation">これを呼ぶとawaitの次の行からの処理が始まります</param>
    public void OnCompleted(Action continuation)
    {
        // とりあえずメモっておく
        _nextOperation = continuation;

        // 終わってたら
        if (IsCompleted)
        {
            // 終わってたらawaitの次の行からの処理を開始させる
            _nextOperation();
            // 念のため消す
            _nextOperation = () => { };
        }
    }

    /// <summary>
    /// なんか知らんがOnCompletedに任す
    /// </summary>
    /// <param name="continuation"></param>
    public void UnsafeOnCompleted(Action continuation)
    {
        OnCompleted(continuation);
    }
}