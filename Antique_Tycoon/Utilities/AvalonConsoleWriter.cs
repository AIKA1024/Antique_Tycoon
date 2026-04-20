using System;
using System.IO;
using System.Text;

namespace Antique_Tycoon.Utilities;

public class AvalonConsoleWriter : TextWriter
{
    private readonly Action<string> _outputHandler;

    public AvalonConsoleWriter(Action<string> outputHandler)
    {
        _outputHandler = outputHandler;
    }

    public override Encoding Encoding => Encoding.UTF8;

    // 重写 WriteLine，这样每当 Console.WriteLine 被调用时都会触发
    public override void WriteLine(string? value)
    {
        if (value != null)
        {
            // 转发给处理器（例如你的任务队列或 UI 集合）
            _outputHandler(value);
        }
    }

    // 也要重写 Write，处理非换行的输出
    public override void Write(char value)
    {
        _outputHandler(value.ToString());
    }
}