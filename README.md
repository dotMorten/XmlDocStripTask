# XmlDocStripTask
Optimizes your xml documentation for intellisense by stripping out internal documentation and remarks.

Simply add the nuget package `dotMorten.XmlDocStripTask` and build. If your project has XML documentation, a secondary xml doc file named `[assemblyname].intellisense.xml` will be generated which excludes all internal members as well as the remarks section. Use this to distribute with your assemblies instead, and only use the original to generate documentation pages.

#### Nuget Package:

https://www.nuget.org/packages/dotMorten.XmlDocStripTask
