# SecEdgarMiner

Azure Durable Function App that reads and analyzes SEC filings.

Technologies
- C# 7.0
- .NET Core 3.1
- Azure Functions v3

Features
- RSS feed consumption using conditional GET requests
- Fan-out/fan-in durable functions orchestration
- Emails sent via SendGrid Api
- Dependency Injection
- Local development secrets read from Azure Key Vault
    - This bit is actually simple, but the how-to is poorly documented on the web.
- Strongly-typed configuration using IOptions
- Project deployed via: Azure Resource Group project + ARM template + MSDeploy + Zip pkg
    - This bit is a wicket and doesn't work yet. But I've seen it done so stay tuned.

Copyright (c) 2021 Osita Christopher Okonkwo
