using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ConsoleWriter : TextWriter
{
    public override void Write(char value)
    {
    }

    public override void Write(string value)
    {
        Debug.Log(value);
    }

    public override Encoding Encoding
    {
        get { return Encoding.ASCII; }
    }
}

public class ErrorWriter : TextWriter
{
    public override void Write(char value)
    {
    }

    public override void Write(string value)
    {
        Debug.LogError(value);
    }

    public override Encoding Encoding
    {
        get { return Encoding.ASCII; }
    }
}

[InitializeOnLoad]
public class ConsoleRouter
{
    static ConsoleRouter()
    {
        System.Console.SetOut(new ConsoleWriter());
        System.Console.SetError(new ErrorWriter());
    }
}
