
namespace NUnit.SourceInfoWriter
{
  using System;
  using System.Diagnostics;
  using System.IO;
  using System.Text;
  using System.Xml;
  using NUnit.Engine.Addins;
  using NUnit.Engine.Extensibility;

  [Extension]
  [ExtensionProperty("Format", "nunit2withsourceinfo")]
  public class NUnitV2WithSourceInfoResultWriter : IResultWriter
  {
    public void CheckWritability(string outputPath)
    {
      using (new StreamWriter(outputPath, false, Encoding.UTF8)) { }
    }

    public void WriteResultFile(XmlNode resultNode, string outputPath)
    {
      using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
      {
        WriteResultFile(resultNode, writer);
      }
    }

    public void WriteResultFile(XmlNode resultNode, TextWriter writer)
    {
      var nunit2Output = new MemoryStream();      
      using (var nunit2OutputWriter = new StreamWriter(nunit2Output))
      {
        var nunit2XmlResultWriter = new NUnit2XmlResultWriter();
        nunit2XmlResultWriter.WriteResultFile(resultNode, nunit2OutputWriter);
      }

      nunit2Output.Position = 0;
      var nunit2WithSourceInfo = new XmlDocument();
      nunit2WithSourceInfo.Load(nunit2Output);

      // ToDo: SourceInfoHelper does not yet understand nunit2 xml format
      SourceInfoHelper.AddSourceInfoToTestResults(nunit2WithSourceInfo);
      writer.WriteLine(nunit2WithSourceInfo.OuterXml);
      WriteDebugToOutput("WriteResultFile (TestWriter): " + nunit2WithSourceInfo.OuterXml);
    }

    [Conditional("DEBUG")]
    private static void WriteDebugToOutput(string format, params object[] arg)
    {
      Console.WriteLine(format, arg);
    }
  }
}
