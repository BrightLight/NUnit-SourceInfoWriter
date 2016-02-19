namespace NUnit.SourceInfoWriter
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Xml;
  using Microsoft.Cci;
  using global::NUnit.Engine.Extensibility;
  using System.Diagnostics;
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
    }

    /// <summary>
    /// Writes result to the specified output path.
    /// </summary>
    /// <param name="resultNode">XmlNode for the result</param>
    /// <param name="outputPath">Path to which it should be written</param>
    public void WriteResultFile(XmlNode resultNode, string outputPath)
    {
      this.AddSourceInfoToTestResults(resultNode);
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
      this.AddSourceInfoToTestResults(resultNode);
      writer.WriteLine(resultNode.OuterXml);
      WriteDebugToOutput("WriteResultFile (TestWriter): " + resultNode.OuterXml);
    }

    private void AddSourceInfoToTestResults(XmlNode testResultsXml)
    {
      // ToDo: more safety checks (null references, etc.)
      WriteDebugToOutput("Analyzing XML");
      foreach (XmlNode assemblyNode in testResultsXml.OwnerDocument.SelectNodes("/test-run/test-suite[@type='Assembly']"))
      {
        WriteDebugToOutput("Assembly node: " + assemblyNode.Name);
        var assemblyLocation = assemblyNode.Attributes["fullname"];
        foreach (XmlNode testcaseNode in assemblyNode.SelectNodes("//test-case"))
        {
          WriteDebugToOutput("TestCase node: " + testcaseNode.Name);
          var className = testcaseNode.Attributes["classname"].Value;
          var methodName = testcaseNode.Attributes["methodname"];
          var namespaceName = string.Empty;

          var lastDot = className.LastIndexOf('.');
          if (lastDot > 0)
          {
            namespaceName = className.Substring(0, lastDot);
            className = className.Substring(lastDot + 1);
          }

          SourceLocation sourceLocation;
          if (TryFindSourceLocationForMethod(assemblyLocation.Value, namespaceName, className, methodName.Value, out sourceLocation))
          {
            var sourceFileAttribute = testcaseNode.OwnerDocument.CreateAttribute("sourcefile");
            sourceFileAttribute.Value = sourceLocation.File;
            testcaseNode.Attributes.Append(sourceFileAttribute);

            var sourceLineAttribute = testcaseNode.OwnerDocument.CreateAttribute("sourceline");
            sourceLineAttribute.Value = sourceLocation.Line.ToString();
            testcaseNode.Attributes.Append(sourceLineAttribute);

            var sourceColumnAttribute = testcaseNode.OwnerDocument.CreateAttribute("sourcecolumn");
            sourceColumnAttribute.Value = sourceLocation.Column.ToString();
            testcaseNode.Attributes.Append(sourceColumnAttribute);
          }
        }
      }
    }

    private bool TryFindSourceLocationForMethod(string assemblyPath, string namespaceName, string typeName, string methodName, out SourceLocation sourceLocation)
    {
      // ToDo: add check whether files (dll, exe, pdb) exist
      // ToDo: more safety checks (null references, etc.)
      // ToDo: add logging if something goes wrong (no pdb, etc)
      WriteDebugToOutput("Trying to find source location for class {0} (in namespace {3}) method {1} in assembly {2}", typeName, methodName, assemblyPath, namespaceName);
      var host = new PeReader.DefaultHost();
      var pdbFile = Path.ChangeExtension(assemblyPath, "pdb");
      using (var pdbStream = File.OpenRead(pdbFile))
      using (var pdbReader = new PdbReader(pdbStream, host))
      {
        var assembly = host.LoadUnitFrom(assemblyPath) as IAssembly;
        foreach (var someType in assembly.GetAllTypes().Where(x => IsRequestedType(x, namespaceName, typeName)))
        {
          foreach (var member in someType.Methods)
          {
            if (member.Name.Value == methodName)
            {
              foreach (var location in member.Locations)
              {
                WriteDebugToOutput("Location: {0}, ({1})", location.Document.Name.Value, location.ToString());
                foreach (var primarySourceLocation in pdbReader.GetClosestPrimarySourceLocationsFor(location))
                {
                  WriteDebugToOutput("File: {0}, Line {1}, Column {2}", primarySourceLocation.SourceDocument.Location, primarySourceLocation.StartLine - 1, primarySourceLocation.StartColumn);
                  sourceLocation = new SourceLocation() { File = primarySourceLocation.SourceDocument.Location, Line = primarySourceLocation.StartLine - 1, Column = primarySourceLocation.StartColumn };
                  return true;
                }
              }
            }
          }
        }
      }

      sourceLocation = null;
      return false;
    }

    private static bool IsRequestedType(INamedTypeDefinition typeDefinition, string namespaceName, string typeName)
    {
      var namespaceMember = typeDefinition as INamespaceMember;
      WriteDebugToOutput("Inspecting {0}, namespace {1}", typeDefinition.Name, namespaceMember.ContainingNamespace.Name.Value);

      // check namespace if type belongs to a namespace
      if (!string.IsNullOrEmpty(namespaceName)
        && (namespaceMember == null))
      {
        return false;
      }

      if (namespaceMember != null)
      {
        if (namespaceMember.ContainingNamespace.Name.Value != namespaceName)
        {
          return false;
        }
      }

      return typeDefinition.Name.Value == typeName;
    }

    [Conditional("DEBUG")]
    private static void WriteDebugToOutput(string format, params object[] arg)
    {
      Console.WriteLine(format, arg);
    }
  }
}