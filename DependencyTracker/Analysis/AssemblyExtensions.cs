namespace DependencyTracker.Analysis
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;

  public static class AssemblyExtensions
  {
    public static string GetProductName(this ModelAssembly descriptor, RuntimeSettings settings)
    {
      var productName = descriptor.Assembly.GetAssemblyInfo<AssemblyProductAttribute>(attribute => attribute.Product);
      var sanitizedProductName = SanitizeAssemblyInfo(productName, descriptor.Assembly, settings.NamingConfiguration["ProductNameFallBacks"], settings.NamingConfiguration["ProductNameOverrides"]);
      return sanitizedProductName;
    }

    public static string GetCompanyName(this ModelAssembly descriptor, RuntimeSettings settings)
    {
      var companyName = descriptor.Assembly.GetAssemblyInfo<AssemblyCompanyAttribute>(attribute => attribute.Company);
      var sanitizedCompanyName = SanitizeAssemblyInfo(companyName, descriptor.Assembly, settings.NamingConfiguration["CompanyNameFallbacks"], settings.NamingConfiguration["CompanyNameOverrides"]);

      return sanitizedCompanyName;
    }

    private static string SanitizeAssemblyInfo(string assemblyInfo, Assembly assembly, Dictionary<string, string> fallbackDictionary, Dictionary<string, string> aliasesDictionary)
    {
      if (string.IsNullOrEmpty(assemblyInfo))
      {
        assemblyInfo = GetFallbackForMissingMetadata(assembly, fallbackDictionary);
      }
      else
      {
        assemblyInfo = GetPreferredAlias(aliasesDictionary, assemblyInfo);
      }
      return assemblyInfo;
    }

    private static string GetAssemblyInfo<T>(this Assembly assembly, Func<T, string> resolveInfo) where T : Attribute
    {
      var assemblyAttribute = assembly.GetCustomAttribute<T>();

      if (assemblyAttribute == null)
      {
        return default(string);
      }

      return resolveInfo(assemblyAttribute);
    }

    private static string GetPreferredAlias(Dictionary<string, string> aliases, string assemblyInfo)
    {
      var matchedStem = aliases.Keys.FirstOrDefault(alias => assemblyInfo.StartsWith(alias, StringComparison.OrdinalIgnoreCase));

      if (matchedStem != default(string))
      {
        var preferedAlias = aliases[matchedStem];
        if (preferedAlias != assemblyInfo)
          return preferedAlias;
      }

      return assemblyInfo;
    }

    private static string GetFallbackForMissingMetadata(this Assembly assembly, Dictionary<string, string> fallbacks)
    {
      foreach (var key in fallbacks.Keys.Where(key => assembly.FullName.StartsWith(key, StringComparison.OrdinalIgnoreCase)))
      {
        return fallbacks[key];
      }
      return "Missing metadata";
    }
  }

  public class RuntimeSettings
  {
    public RuntimeSettings(Dictionary<string, Dictionary<string, string>> namingConfiguration)
    {
      if (namingConfiguration == null)
      {
        namingConfiguration = this.DefaultNamingConfiguration;
      }
      
      this.NamingConfiguration = namingConfiguration;
    }

    public Dictionary<string, Dictionary<string, string>> NamingConfiguration { get; private set; }

    public Dictionary<string, Dictionary<string, string>> DefaultNamingConfiguration = new Dictionary<string, Dictionary<string, string>>()
                                                                       {
                                                                         {"ProductNameOverrides", new Dictionary<string, string>()
                                                                                {
                                                                                  //{"Names starting with this"}, {"Will be replaced by this"}
                                                                                  //{"Sitecore.PathAnalyzer","Sitecore Path Analyzer"},
                                                                                  //{"Sitecore.SequenceAnalyzer","Sitecore Path Analyzer"},
                                                                                  //{"Sitecore Speak", "Sitecore SPEAK"},
                                                                                  //{"Sitecore.EmailCampaign", "Sitecore Email Experience Manager"},
                                                                                  //{"Email Experience Manager", "Sitecore Email Experience Manager"}                                                                                  
                                                                                }
                                                                         },
                                                                         {"ProductNameFallBacks", new Dictionary<string, string>()
                                                                                {
                                                                                  //{"Names starting with this"}, {"Will be replaced by this"}
                                                                                  {"ecmascript.net.","Ecmascript for .Net"},
                                                                                  {"mvp.xml","Mvp.Xml"},
                                                                                  {"google.apis","Google APIs"},
                                                                                  {"telerik.web.ui","Telerik.Web.UI"},
                                                                                  {"componentart","Component Art"},                                                                                  
                                                                                }
                                                                         },
                                                                         {"CompanyNameOverrides", new Dictionary<string, string>()
                                                                                {
                                                                                  //{"Names starting with this"}, {"Will be replaced by this"}
                                                                                  //{"Sitecore", "Sitecore Corporation" },
                                                                                  {"Microsoft", "Microsoft" },
                                                                                }
                                                                         },
                                                                         {"CompanyNameFallbacks", new Dictionary<string, string>()
                                                                                {
                                                                                  //{"Names starting with this"}, {"Will be replaced by this"}
                                                                                  {"DotNetOpenAuth", "DotNetOpenAuth community" },
                                                                                  {"OAuthLinkedIn", "OAuthLinkedIn community" },
                                                                                  {"ecmascript.net", "Ecmascript community" },
                                                                                  {"yahoo.", "Yahoo! Inc" },
                                                                                  {"mvp.xml", "Microsoft MVPs in XML technologies" },
                                                                                  {"google.", "Google Inc" },
                                                                                  //{"sitecore.", "Sitecore Corporation" },
                                                                                  {"telerik.", "Telerik" },
                                                                                  {"lucene.", "The Apache Software Foundation" },
                                                                                }
                                                                         },
                                                                       };
  }
}