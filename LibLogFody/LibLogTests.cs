﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LibLogAssembly.Logging;
using NUnit.Framework;

[TestFixture]
public class LibLogTests
{
    string beforeAssemblyPath;
    Assembly assembly;
    string afterAssemblyPath;
    LogCapture logProvider;

    public LibLogTests()
    {

        AppDomainAssemblyFinder.Attach();
		beforeAssemblyPath = Path.GetFullPath(@"..\..\..\LibLogAssemblyToProcess\bin\Debug\LibLogAssemblyToProcess.dll");
#if (!DEBUG)
        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif
        afterAssemblyPath = WeaverHelper.Weave(beforeAssemblyPath);
        assembly = Assembly.LoadFile(afterAssemblyPath);

        logProvider = new LogCapture();
        LogProvider.SetCurrentLogProvider(logProvider);
    }

    [Test]
    public void MethodThatReturns()
    {
        var type = assembly.GetType("OnException");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.AreEqual("a", instance.MethodThatReturns("x", 6));
    }
    [SetUp]
    public void Setup()
    {
        logProvider.Clear();
    }

    [Test]
    public void Generic()
    {
        var type = assembly.GetType("GenericClass`1");
        var constructedType = type.MakeGenericType(typeof (string));
        var instance = (dynamic)Activator.CreateInstance(constructedType);
        instance.Debug();
        //var message = logProvider.Debugs.First();
        //Assert.IsTrue(message.StartsWith("Method: 'Void Debug()'. Line: ~"));
    }


    [Test]
    public void ClassWithExistingField()
    {
        var type = assembly.GetType("ClassWithExistingField");
        Assert.AreEqual(1, type.GetFields(BindingFlags.NonPublic | BindingFlags.Static).Count());
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Debug();
        Assert.AreEqual(1, logProvider.Debugs.Count);
        Assert.IsTrue(logProvider.Debugs.First().StartsWith("Method: 'Void Debug()'. Line: ~"));
    }

    // ReSharper disable once UnusedParameter.Local
    void CheckException(Action<object> action, List<string> list, string expected)
    {
        Exception exception = null;
        var type = assembly.GetType("OnException");
        var instance = (dynamic)Activator.CreateInstance(type);
        try
        {
            action(instance);
        }
        catch (Exception e)
        {
            exception = e;
        }
        Assert.IsNotNull(exception);
        Assert.AreEqual(1, list.Count);
        var first = list.First();
        Assert.IsTrue(first.StartsWith(expected), first);
    }


    [Test]
    public void OnExceptionToTrace()
    {
        var expected = "Exception occurred in 'void ToTrace(String, Int32)'.  param1 'x' param2 '6'";
        Action<dynamic> action = o => o.ToTrace("x", 6);
        CheckException(action, logProvider.Traces, expected);
    }

    [Test]
    public void OnExceptionToTraceWithReturn()
    {
        var expected = "Exception occurred in 'Object ToTraceWithReturn(String, Int32)'.  param1 'x' param2 '6'";
        Action<dynamic> action = o => o.ToTraceWithReturn("x", 6);
        CheckException(action, logProvider.Traces, expected);
    }
    [Test]
    public void OnExceptionToDebug()
    {
        var expected = "Exception occurred in 'void ToDebug(String, Int32)'.  param1 'x' param2 '6'";
        Action<dynamic> action = o => o.ToDebug("x", 6);
        CheckException(action, logProvider.Debugs, expected);
    }

    [Test]
    public void OnExceptionToDebugWithReturn()
    {
        var expected = "Exception occurred in 'Object ToDebugWithReturn(String, Int32)'.  param1 'x' param2 '6'";
        Action<dynamic> action = o => o.ToDebugWithReturn("x", 6);
        CheckException(action, logProvider.Debugs, expected);
    }

    [Test]
    public void OnExceptionToInfo()
    {
        var expected = "Exception occurred in 'void ToInfo(String, Int32)'.  param1 'x' param2 '6'";
        Action<dynamic> action = o => o.ToInfo("x", 6);
        CheckException(action, logProvider.Informations, expected);
    }

    [Test]
    public void OnExceptionToInfoWithReturn()
    {
        var expected = "Exception occurred in 'Object ToInfoWithReturn(String, Int32)'.  param1 'x' param2 '6'";
        Action<dynamic> action = o => o.ToInfoWithReturn("x", 6);
        CheckException(action, logProvider.Informations, expected);
    }

    [Test]
    public void OnExceptionToWarn()
    {
        var expected = "Exception occurred in 'void ToWarn(String, Int32)'.  param1 'x' param2 '6'";
        Action<dynamic> action = o => o.ToWarn("x", 6);
        CheckException(action, logProvider.Warns, expected);
    }

    [Test]
    public void OnExceptionToWarnWithReturn()
    {
        var expected = "Exception occurred in 'Object ToWarnWithReturn(String, Int32)'.  param1 'x' param2 '6'";
        Action<dynamic> action = o => o.ToWarnWithReturn("x", 6);
        CheckException(action, logProvider.Warns, expected);
    }

    [Test]
    public void OnExceptionToError()
    {
        var expected = "Exception occurred in 'void ToError(String, Int32)'.  param1 'x' param2 '6'";
        Action<dynamic> action = o => o.ToError("x", 6);
        CheckException(action, logProvider.Errors, expected);
    }

    [Test]
    public void OnExceptionToErrorWithReturn()
    {
        var expected = "Exception occurred in 'Object ToErrorWithReturn(String, Int32)'.  param1 'x' param2 '6'";
        Action<dynamic> action = o => o.ToErrorWithReturn("x", 6);
        CheckException(action, logProvider.Errors, expected);
    }
    [Test]
    public void OnExceptionToFatal()
    {
        var expected = "Exception occurred in 'void ToFatal(String, Int32)'.  param1 'x' param2 '6'";
        Action<dynamic> action = o => o.ToFatal("x", 6);
        CheckException(action, logProvider.Fatals, expected);
    }

    [Test]
    public void OnExceptionToFatalWithReturn()
    {
        var expected = "Exception occurred in 'Object ToFatalWithReturn(String, Int32)'.  param1 'x' param2 '6'";
        Action<dynamic> action = o => o.ToFatalWithReturn("x", 6);
        CheckException(action, logProvider.Fatals, expected);
    }

    [Test]
    public void IsTraceEnabled()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        Assert.IsTrue(instance.IsTraceEnabled());
    }
    [Test]
    public void Trace()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Trace();
        Assert.AreEqual(1, logProvider.Traces.Count);
        Assert.IsTrue(logProvider.Traces.First().StartsWith("Method: 'void Trace()'. Line: ~"));
    }
    [Test]
    public void TraceString()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.TraceString();
        Assert.AreEqual(1, logProvider.Traces.Count);
        Assert.IsTrue(logProvider.Traces.First().StartsWith("Method: 'void TraceString()'. Line: ~"));
    }

    [Test]
    public void TraceStringParams()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.TraceStringParams();
        Assert.AreEqual(1, logProvider.Traces.Count);
        Assert.IsTrue(logProvider.Traces.First().StartsWith("Method: 'void TraceStringParams()'. Line: ~"));
    }

    [Test]
    public void TraceStringException()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.TraceStringException();
        Assert.AreEqual(1, logProvider.Traces.Count);
        Assert.IsTrue(logProvider.Traces.First().StartsWith("Method: 'void TraceStringException()'. Line: ~"));
    }

    [Test]
    public void IsDebugEnabled()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        Assert.IsTrue(instance.IsDebugEnabled());
    }
    [Test]
    public void Debug()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Debug();
        Assert.AreEqual(1, logProvider.Debugs.Count);
        Assert.IsTrue(logProvider.Debugs.First().StartsWith("Method: 'void Debug()'. Line: ~"));
    }

    [Test]
    public void DebugString()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.DebugString();
        Assert.AreEqual(1, logProvider.Debugs.Count);
        Assert.IsTrue(logProvider.Debugs.First().StartsWith("Method: 'void DebugString()'. Line: ~"));
    }

    [Test]
    public void DebugStringParams()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.DebugStringParams();
        Assert.AreEqual(1, logProvider.Debugs.Count);
        Assert.IsTrue(logProvider.Debugs.First().StartsWith("Method: 'void DebugStringParams()'. Line: ~"));
    }

    [Test]
    public void DebugStringException()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.DebugStringException();
        Assert.AreEqual(1, logProvider.Debugs.Count);
        Assert.IsTrue(logProvider.Debugs.First().StartsWith("Method: 'void DebugStringException()'. Line: ~"));
    }
    [Test]
    public void IsInfoEnabled()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        Assert.IsTrue(instance.IsInfoEnabled());
    }

    [Test]
    public void Info()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Info();
        Assert.AreEqual(1, logProvider.Informations.Count);
        Assert.IsTrue(logProvider.Informations.First().StartsWith("Method: 'void Info()'. Line: ~"));
    }

    [Test]
    public void InfoString()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.InfoString();
        Assert.AreEqual(1, logProvider.Informations.Count);
        Assert.IsTrue(logProvider.Informations.First().StartsWith("Method: 'void InfoString()'. Line: ~"));
    }

    [Test]
    public void InfoStringParams()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.InfoStringParams();
        Assert.AreEqual(1, logProvider.Informations.Count);
        Assert.IsTrue(logProvider.Informations.First().StartsWith("Method: 'void InfoStringParams()'. Line: ~"));
    }

    [Test]
    public void InfoStringException()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.InfoStringException();
        Assert.AreEqual(1, logProvider.Informations.Count);
        Assert.IsTrue(logProvider.Informations.First().StartsWith("Method: 'void InfoStringException()'. Line: ~"));
    }

    [Test]
    public void IsWarnEnabled()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        Assert.IsTrue(instance.IsWarnEnabled());
    }
    [Test]
    public void Warn()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Warn();
        Assert.AreEqual(1, logProvider.Warns.Count);
        Assert.IsTrue(logProvider.Warns.First().StartsWith("Method: 'void Warn()'. Line: ~"));
    }

    [Test]
    public void WarnString()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.WarnString();
        Assert.AreEqual(1, logProvider.Warns.Count);
        Assert.IsTrue(logProvider.Warns.First().StartsWith("Method: 'void WarnString()'. Line: ~"));
    }

    [Test]
    public void WarnStringParams()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.WarnStringParams();
        Assert.AreEqual(1, logProvider.Warns.Count);
        Assert.IsTrue(logProvider.Warns.First().StartsWith("Method: 'void WarnStringParams()'. Line: ~"));
    }

    [Test]
    public void WarnStringException()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.WarnStringException();
        Assert.AreEqual(1, logProvider.Warns.Count);
        Assert.IsTrue(logProvider.Warns.First().StartsWith("Method: 'void WarnStringException()'. Line: ~"));
    }

    [Test]
    public void IsErrorEnabled()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        Assert.IsTrue(instance.IsErrorEnabled());
    }
    [Test]
    public void Error()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Error();
        Assert.AreEqual(1, logProvider.Errors.Count);
        Assert.IsTrue(logProvider.Errors.First().StartsWith("Method: 'void Error()'. Line: ~"));
    }

    [Test]
    public void ErrorString()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.ErrorString();
        Assert.AreEqual(1, logProvider.Errors.Count);
        Assert.IsTrue(logProvider.Errors.First().StartsWith("Method: 'void ErrorString()'. Line: ~"));
    }

    [Test]
    public void ErrorStringParams()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.ErrorStringParams();
        Assert.AreEqual(1, logProvider.Errors.Count);
        Assert.IsTrue(logProvider.Errors.First().StartsWith("Method: 'void ErrorStringParams()'. Line: ~"));
    }

    [Test]
    public void ErrorStringException()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.ErrorStringException();
        Assert.AreEqual(1, logProvider.Errors.Count);
        Assert.IsTrue(logProvider.Errors.First().StartsWith("Method: 'void ErrorStringException()'. Line: ~"));
    }

    [Test]
    public void IsFatalEnabled()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        Assert.IsTrue(instance.IsFatalEnabled());
    }
    [Test]
    public void Fatal()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.Fatal();
        Assert.AreEqual(1, logProvider.Fatals.Count);
        Assert.IsTrue(logProvider.Fatals.First().StartsWith("Method: 'void Fatal()'. Line: ~"));
    }

    [Test]
    public void FatalString()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.FatalString();
        Assert.AreEqual(1, logProvider.Fatals.Count);
        Assert.IsTrue(logProvider.Fatals.First().StartsWith("Method: 'void FatalString()'. Line: ~"));
    }

    [Test]
    public void FatalStringParams()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.FatalStringParams();
        Assert.AreEqual(1, logProvider.Fatals.Count);
        Assert.IsTrue(logProvider.Fatals.First().StartsWith("Method: 'void FatalStringParams()'. Line: ~"));
    }

    [Test]
    public void FatalStringException()
    {
        var type = assembly.GetType("ClassWithLogging");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.FatalStringException();
        Assert.AreEqual(1, logProvider.Fatals.Count);
        Assert.IsTrue(logProvider.Fatals.First().StartsWith("Method: 'void FatalStringException()'. Line: ~"));
    }

    [Test]
    public void PeVerify()
    {
        Verifier.Verify(beforeAssemblyPath, afterAssemblyPath);
    }


    [Test]
    public void AsyncMethod()
    {
        var type = assembly.GetType("ClassWithCompilerGeneratedClasses");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.AsyncMethod();
        Assert.AreEqual(1, logProvider.Debugs.Count);
        Assert.IsTrue(logProvider.Debugs.First().StartsWith("Method: 'Void AsyncMethod()'. Line: ~"));
    }
    [Test]
    public void EnumeratorMethod()
    {
        var type = assembly.GetType("ClassWithCompilerGeneratedClasses");
        var instance = (dynamic)Activator.CreateInstance(type);
        ((IEnumerable<int>)instance.EnumeratorMethod()).ToList();
        Assert.AreEqual(1, logProvider.Debugs.Count);
        Assert.IsTrue(logProvider.Debugs.First().StartsWith("Method: 'IEnumerable<Int32> EnumeratorMethod()'. Line: ~"), logProvider.Debugs.First());
    }
    [Test]
    public void DelegateMethod()
    {
        var type = assembly.GetType("ClassWithCompilerGeneratedClasses");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.DelegateMethod();
        Assert.AreEqual(1, logProvider.Debugs.Count);
        Assert.IsTrue(logProvider.Debugs.First().StartsWith("Method: 'Void DelegateMethod()'. Line: ~"), logProvider.Debugs.First());
    }
    [Test]
    public void LambdaMethod()
    {
        var type = assembly.GetType("ClassWithCompilerGeneratedClasses");
        var instance = (dynamic)Activator.CreateInstance(type);
        instance.LambdaMethod();
        Assert.AreEqual(1, logProvider.Debugs.Count);
        Assert.IsTrue(logProvider.Debugs.First().StartsWith("Method: 'Void LambdaMethod()'. Line: ~"), logProvider.Debugs.First());
    }


}