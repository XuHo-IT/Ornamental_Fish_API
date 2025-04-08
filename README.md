# 🐠 Ornamental_Fish Web API

**Ornamental_Fish** is a modern web platform for browsing and purchasing ornamental fish online. Built with ASP.NET Core 8, Entity Framework Core, and Identity Framework for secure authentication, it also includes cutting-edge features like **voice search** and **hand recognition-based ordering**.

---

## 🌐 Features

- 🛒 Buy ornamental fish online
- 🔐 Secure login with **Gmail** and **Facebook**
- 🎤 **Voice-based search** powered by Vosk
- ✋ **Hand gesture recognition** to simplify the ordering process
- 📦 Modular architecture with clean API design
- 📚 API versioning and Swagger/OpenAPI integration

---

## 🧰 Technologies Used

- **.NET Core 8 / ASP.NET Core**
- **Entity Framework Core**
- **Identity Framework**
- **AutoMapper**
- **Vosk Speech Recognition**
- **Cloudinary for image hosting**
- **NAudio for audio processing**
- **Azure OpenAI for AI assistance**

---

## 📦 Required NuGet Packages

To run this project, ensure the following packages are installed:

```bash
dotnet add package AutoMapper --version 12.0.0
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.0
dotnet add package Azure.AI.OpenAI --version 2.1.0
dotnet add package CloudinaryDotNet --version 1.27.4
dotnet add package Microsoft.AspNetCore.Authentication.Cookies --version 2.3.0
dotnet add package Microsoft.AspNetCore.Authentication.Facebook --version 8.0.0
dotnet add package Microsoft.AspNetCore.Authentication.Google --version 8.0.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0
dotnet add package Microsoft.AspNetCore.Authentication.OpenIdConnect --version 8.0.0
dotnet add package Microsoft.AspNetCore.Cors --version 2.3.0
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.0
dotnet add package Microsoft.AspNetCore.JsonPatch --version 9.0.2
dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson --version 8.0.0
dotnet add package Microsoft.AspNetCore.Mvc.Versioning --version 5.1.0
dotnet add package Microsoft.AspNetCore.OpenApi --version 8.0.2
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.2
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.2
dotnet add package Microsoft.Extensions.Configuration.Json --version 8.0.0
dotnet add package NAudio --version 2.2.1
dotnet add package Newtonsoft.Json --version 13.0.3
dotnet add package OpenAI --version 2.1.0
dotnet add package RestSharp --version 112.1.0
dotnet add package Swashbuckle.AspNetCore --version 6.4.0
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.0.0
dotnet add package Vosk --version 0.3.38
