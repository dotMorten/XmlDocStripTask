# XmlDocStripTask
Optimizes your xml documentation for intellisense by stripping out internal documentation and remarks.

Simply add the nuget package `dotMorten.XmlDocStripTask` and build. If your project has XML documentation, a xml doc file will be stripped by excluding all internal members as well as the remarks sections. Use this to distribute with your assemblies instead, reducing the size of your packages and increase intellisense load times.

#### Nuget Package:

https://www.nuget.org/packages/dotMorten.XmlDocStripTask
