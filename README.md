# NUnit-SourceInfoWriter
AddIn for [NUnit 3](https://github.com/nunit/nunit) that adds source info (filename, line, column) to each test result

[![Build status](https://ci.appveyor.com/api/projects/status/7esx13mv1ffdiy9x?svg=true)](https://ci.appveyor.com/project/MarkusHastreiter/nunit-sourceinfowriter)

## Installation
* Copy the binaries of this add-in into the "addins" folder of your nunit3.
* Add this line to the file "nunit.engine.addins" (in the nunit3 folder):
  `addins/NUnit.SourceInfoWriter.dll`

## Usage
`nunit3-console.exe TestProject.dll --result=nunit3withsourceninfo_results.xml;format=nunit3withsourceinfo`

## How it works
NUnit3 already exports the "fullname", "methodname" and "classname" per test-case. However, some tools (like [SonarQube](http://www.sonarqube.org/)) might require a reference to the actual source file. NUnit-SourceInfoWriter uses the pdb file of the testes project and determines the location of this test (name of the source file, line and column) using "methodname" and "classname" and adds this information to the test-case tag.

## Example
This is how an "nunit3" output format for test-case looks like:
```xml
<test-case
  id="0-1001"
  name="Foo"
  fullname="OtherNamespace.Class1.Foo"
  methodname="Foo"
  classname="OtherNamespace.Class1"
  runstate="Runnable"
  seed="798259755"
  result="Passed"
  start-time="2016-05-21 07:13:24Z"
  end-time="2016-05-21 07:13:24Z"
  duration="0.004684"
  asserts="0" />
```

And this is how the same line looks with "nunit3withsourceinfo" format:
```xml
<test-case
  id="0-1001"
  name="Foo"
  fullname="OtherNamespace.Class1.Foo"
  methodname="Foo"
  classname="OtherNamespace.Class1"
  runstate="Runnable"
  seed="798259755"
  result="Passed"
  start-time="2016-05-21 07:13:24Z"
  end-time="2016-05-21 07:13:24Z"
  duration="0.004684"
  asserts="0"
  sourcefile="C:\projects\NUnit-SourceInfoWriter\TestProject\SecondFile.cs"
  sourceline="9"
  sourcecolumn="5" />
```
