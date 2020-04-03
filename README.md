[![Nuget](https://img.shields.io/nuget/v/Yandex.Cloud.SDK)](https://www.nuget.org/packages/Yandex.Cloud.SDK/)
[![CircleCI](https://img.shields.io/circleci/build/gh/yandex-cloud/dotnet-sdk/master)](https://circleci.com/gh/yandex-cloud/dotnet-sdk/tree/master)
[![License](https://img.shields.io/github/license/yandex-cloud/dotnet-sdk.svg)](https://github.com/yandex-cloud/dotnet-sdk/blob/master/LICENSE)

# Yandex.Cloud SDK (C#) 

Need to automate your infrastructure or use services provided by Yandex.Cloud? We've got you covered.

Installation:

    dotnet add package Yandex.Cloud.SDK

## Getting started

There are several options for authorization your requests - OAuth Token,
Metadata Service (if you're executing code inside VMs or Functions
running in Yandex.Cloud) and Service Account Keys

### OAuth Token

```csharp
using Yandex.Cloud;
using Yandex.Cloud.Credentials;

var sdk = new Sdk(new OAuthCredentialsProvider("AQAD-....."));
```

### Metadata Service

Don't forget to assign Service Account for your Instance or Function and grant required roles.

```csharp
using Yandex.Cloud;

var sdk = new Sdk();
```

Check `Example` directory for more examples.

## Hacking

To build and test this SDK you need to have installed `dotnet` utility. All required tasks are defined
in `Makefile` (`all` will build, generate and run all tests).