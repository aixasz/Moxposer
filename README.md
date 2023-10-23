# Moxposer - HttpClient Usage Analyzer for .NET 🛡️

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

4. **Integrate in Your Project**

   [Instructions on how to use the analyzer in other projects]

## 💡 Use Cases

- **Development Phase:** Incorporate Moxposer early in the development process to ensure code quality and data safety.
- **Code Audits:** A handy supplement during code reviews to highlight potential data leakage points.
- **Pipeline Integration:** Integrate into your CI/CD pipeline for automatic adherence to code standards and data protection norms.

## 🛠️ Customization

- **Whitelisting Packages:** Moxposer provides flexibility in exempting certain packages or libraries from analysis. [Details on how to whitelist]

## 📄 Documentation



## 🤝 Contributing



## 📜 License

Moxposer is [MIT licensed](https://github.com/aixasz/Moxposer/blob/dev/LICENSE).

---

Note: You might need to replace placeholders like `[your_repository_url]`, `path_to_your_logo_if_any`, `link_to_detailed_docs_if_any`, etc., with actual URLs or paths as applicable.

