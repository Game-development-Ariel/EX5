using System;
using NUnit.Framework;
using UnityEngine.TestTools;

public class SafeEventRaiseTests
{
    [Test]
    public void RaiseIsolated_CallsEveryListener_WhenAllSucceed()
    {
        int a = 0;
        int b = 0;
        Action evt = null;
        evt += () => a++;
        evt += () => b++;

        SafeEventRaise.RaiseIsolated(evt);

        Assert.AreEqual(1, a);
        Assert.AreEqual(1, b);
    }

    [Test]
    public void RaiseIsolated_ContinuesAfterOneListenerThrows()
    {
        int a = 0;
        int c = 0;
        Action evt = null;
        evt += () => a++;
        evt += () => throw new InvalidOperationException("bad listener");
        evt += () => c++;

        LogAssert.ignoreFailingMessages = true;
        SafeEventRaise.RaiseIsolated(evt);
        LogAssert.ignoreFailingMessages = false;

        Assert.AreEqual(1, a, "Listener before the thrower still ran");
        Assert.AreEqual(1, c, "Listener after the thrower still ran");
    }

    [Test]
    public void RaiseIsolated_NullEvent_IsNoOp()
    {
        Action evt = null;
        Assert.DoesNotThrow(() => SafeEventRaise.RaiseIsolated(evt));
    }

    [Test]
    public void RaiseIsolated_Typed_CallsEveryListenerWithPayload()
    {
        int sum = 0;
        Action<int> evt = null;
        evt += x => sum += x;
        evt += x => sum += x * 2;

        SafeEventRaise.RaiseIsolated(evt, 5);

        Assert.AreEqual(5 + 10, sum);
    }

    [Test]
    public void RaiseIsolated_Typed_ContinuesAfterException()
    {
        int payloadDelivered = 0;
        Action<int> evt = null;
        evt += x => throw new InvalidOperationException("skip me");
        evt += x => payloadDelivered = x;

        LogAssert.ignoreFailingMessages = true;
        SafeEventRaise.RaiseIsolated(evt, 42);
        LogAssert.ignoreFailingMessages = false;

        Assert.AreEqual(42, payloadDelivered);
    }

    [Test]
    public void RaiseIsolated_TwoParam_WorksIsolated()
    {
        string received = null;
        int receivedNumber = 0;
        Action<string, int> evt = null;
        evt += (s, n) => throw new Exception("first fails");
        evt += (s, n) => { received = s; receivedNumber = n; };

        LogAssert.ignoreFailingMessages = true;
        SafeEventRaise.RaiseIsolated(evt, "hello", 7);
        LogAssert.ignoreFailingMessages = false;

        Assert.AreEqual("hello", received);
        Assert.AreEqual(7, receivedNumber);
    }
}
