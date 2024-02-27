# Moxposer - HttpClient Usage Analyzer for .NET 🛡️

[![build](https://github.com/aixasz/Moxposer/actions/workflows/dotnet_build_test.yml/badge.svg)](https://github.com/aixasz/Moxposer/actions/workflows/dotnet_build_test.yml) 
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/aixasz/Moxposer/blob/dev/LICENSE)

Moxposer is a diagnostic tool tailored to safeguard .NET applications against unintended or potentially harmful `HttpClient` usage patterns. If you're building applications with sensitive data and want to ensure data doesn't unknowingly leak to external sources via the `HttpClient` class, Moxposer is here to help!

## 🌟 Features

- **Deep Analysis:** Dive into the depths of your C# code to identify potential pitfalls in `HttpClient` usage.
- **Focused Detection:** Targets HTTP methods like `PostAsync`, `PutAsync`, and `PatchAsync` that transmit data.
- **Variable URL Warnings:** For variable URLs, receive alerts on potential data sent to unknown destinations.
- **CI/CD Ready:** Perfectly suited for CI/CD pipelines, enabling automated checks in continuous integration environments.
- **Whitelisting:** Option to exempt certain packages from the analyzer's scrutiny via custom whitelisting.
- **Comprehensive Tests:** Reliability is key! And that's why Moxposer comes with an extensive set of unit tests.

## 🚀 Getting Started

1. **Clone the Repository**

   ```bash
   git clone git@github.com:aixasz/Moxposer.git
   cd Moxposer
   ```

2. **Build the Project**

   ```bash
   dotnet build
   ```

3. **Run the Tests**

   ```bash
   dotnet test
   ```

4. **Analyze Your Project**

   To analyze the current directory

   ```bash
   moxposer.runner
   ```

   or specify path to analyze
   
   ```bash
   moxposer.runner -p [Path of C# project or path contains DLL files to analyze]
   ```

## 💡 Use Cases

- **Development Phase:** Incorporate Moxposer early in the development process to ensure code quality and data safety.
- **Code Audits:** A handy supplement during code reviews to highlight potential data leakage points.
- **Pipeline Integration:** Integrate into your CI/CD pipeline for automatic adherence to code standards and data protection norms.

## 🛠️ Customization

- **Whitelisting Packages:** Moxposer provides flexibility in exempting certain packages or libraries from analysis. 
  

## 📄 Documentation

### Global Whitelist Configuration 

Open `appsettings.json` then added whitelist dll name to `GlobalWhitelists` property.  
   ```json
   {
      "GlobalWhitelists": [
         "Microsoft.*",
         "System.*"
      ]
   }
   ```

### specifying which whitelist packages to in `csproj`.

Extract package names from PackageReference tags under ItemGroup tags that have the attribute `DllAnalyzerWhitelist="true"`

example:
```xml
  <ItemGroup DllAnalyzerWhitelist="true">
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.7.0" />
  </ItemGroup>
```


## 🤝 Contributing



## 📜 License

Moxposer is [MIT licensed](https://github.com/aixasz/Moxposer/blob/dev/LICENSE).


