namespace NUnit.SourceInfoWriter
{
  using System;
  using System.IO;
  using System.Xml;
  using global::NUnit.Engine.Extensibility;
  using System.Diagnostics;
  using System.Text;

  [Extension]
  [ExtensionProperty("Format", "nunit3withsourceinfo")]
  public class SourceInfoWriterFactory : IResultWriter
  {
    /// <summary>
    /// Checks if the output path is writable. If the output is not
    /// writable, this method should throw an exception.
    /// </summary>
    /// <param name="outputPath"></param>
    public void CheckWritability(string outputPath)
    {
      WriteDebugToOutput("CheckWritability: " + outputPath);
      using (new StreamWriter(outputPath, false, Encoding.UTF8)) { }
    }

    /// <summary>
    /// Writes result to the specified output path.
    /// </summary>
    /// <param name="resultNode">XmlNode for the result</param>
    /// <param name="outputPath">Path to which it should be written</param>
    public void WriteResultFile(XmlNode resultNode, string outputPath)
    {
      SourceInfoHelper.AddSourceInfoToTestResults(resultNode);
      File.WriteAllText(outputPath, resultNode.OuterXml);
      WriteDebugToOutput("WriteResultFile (outputPath): " + resultNode.OuterXml);
    }

    /// <summary>
    /// Writes result to a TextWriter.
    /// </summary>
    /// <param name="resultNode">XmlNode for the result</param>
    /// <param name="writer">TextWriter to which it should be written</param>
    public void WriteResultFile(XmlNode resultNode, TextWriter writer)
    {
      SourceInfoHelper.AddSourceInfoToTestResults(resultNode);
      writer.WriteLine(resultNode.OuterXml);
      WriteDebugToOutput("WriteResultFile (TestWriter): " + resultNode.OuterXml);
    }

    [Conditional("DEBUG")]
    private static void WriteDebugToOutput(string format, params object[] arg)
    {
      Console.WriteLine(format, arg);
    }
  }
}
