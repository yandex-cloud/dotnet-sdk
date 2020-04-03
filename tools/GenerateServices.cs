using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using Yandex.Cloud.Iam.V1;

namespace tools
{
    class Namespace
    {
        string Name;
        Dictionary<string, Namespace> Nested;
        Dictionary<string, Type> Services;

        public Namespace(string name)
        {
            Name = name;
            Nested = new Dictionary<string, Namespace>();
            Services = new Dictionary<string, Type>();
        }

        public bool Add(Type t, List<string> hierarchy = null)
        {
            if (hierarchy == null)
            {
                hierarchy = ExtractHierarchy(t);
            }

            if (hierarchy.Count == 0)
            {
                Services[t.Name] = t;
                return true;
            }

            var key = hierarchy[0];
            if (!Nested.ContainsKey(key))
            {
                Nested[key] = new Namespace(key);
            }

            return Nested[key].Add(t, hierarchy.GetRange(1, hierarchy.Count - 1));
        }

        private static List<string> ExtractHierarchy(Type t)
        {
            var result = new List<string>();
            var parts = t.FullName?.Split('.');
            for (var i = 2; i < parts.Length - 1; i++)
            {
                if (parts[i] != "V1" && parts[i] != "V2")
                {
                    result.Add(parts[i]);
                }
            }

            return result;
        }

        public void Generate(CodeNamespace ns, string name = null)
        {
            if (name == null)
            {
                name = Name;
            }

            var code = new CodeTypeDeclaration(name);
            code.BaseTypes.Add(new CodeTypeReference("Yandex.Cloud.ServiceRegistry"));

            EndpointConfig endpointConfig;
            if (GeneratorConfig.Endpoints.ContainsKey(name))
            {
                endpointConfig = GeneratorConfig.Endpoints[name];
            }
            else
            {
                Console.Error.WriteLine($"failed to lookup endpoint for {name}");
                endpointConfig = new EndpointConfig("unknown");
            }

            var ctor = new CodeConstructor
            {
                Attributes = MemberAttributes.Public
            };
            ctor.Parameters.Add(new CodeParameterDeclarationExpression("Yandex.Cloud.Sdk", "sdk"));
            ctor.BaseConstructorArgs.Add(new CodeVariableReferenceExpression("sdk"));
            ctor.BaseConstructorArgs.Add(new CodePrimitiveExpression(endpointConfig.Endpoint));
            if (endpointConfig.Endpoint == "unknown")
            {
                ctor.Statements.Add(new CodeThrowExceptionStatement(
                    new CodeObjectCreateExpression(
                        "System.Exception",
                        new CodePrimitiveExpression($"service {name} is not supported at this moment")
                    )
                ));
            }

            code.Members.Add(ctor);

            foreach (var item in Services)
            {
                var key = item.Key;
                var value = item.Value;
                
                var methodName = key.Replace("ServiceClient", "Service");
                var member = new CodeMemberProperty
                {
                    Name = methodName,
                    Attributes = MemberAttributes.Public,
                    Type = new CodeTypeReference(value),
                    HasGet = true,
                    HasSet = false
                };

                CodeMethodInvokeExpression channelExpression;
                if (!endpointConfig.ServiceEndpointOverrides.ContainsKey(methodName))
                {
                    channelExpression = new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(),
                        "GetChannel"
                    );
                }
                else
                {
                    channelExpression = new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(),
                        "GetChannel",
                        new CodePrimitiveExpression(endpointConfig.ServiceEndpointOverrides[methodName]));
                }


                member.GetStatements.Add(new CodeMethodReturnStatement(
                    new CodeObjectCreateExpression(
                        value,
                        channelExpression
                    )
                ));

                code.Members.Add(member);
            }

            foreach (var item in Nested)
            {
                var key = item.Key;
                var value = item.Value;
                
                value.Generate(ns, name + "_" + key);

                var member = new CodeMemberProperty
                {
                    Name = key,
                    Attributes = MemberAttributes.Public,
                    Type = new CodeTypeReference(name + "_" + key),
                    HasGet = true,
                    HasSet = false
                };
                member.GetStatements.Add(new CodeMethodReturnStatement(
                    new CodeObjectCreateExpression(
                        name + "_" + key,
                        new CodeMethodInvokeExpression(
                            new CodeThisReferenceExpression(),
                            "Sdk"
                        )
                    )
                ));
                code.Members.Add(member);
            }

            ns.Types.Add(code);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("One argument (outputFile) is required");
                return;
            }
            
            var outputFile = args[0];

            var tokenServiceType = typeof(IamTokenService.IamTokenServiceClient);
            var rootAssembly = Assembly.GetAssembly(tokenServiceType);
            var rootNamespace = new Namespace("Services");

            var processed = rootAssembly.GetExportedTypes()
                .Where(t => t.IsClass)
                .Where(t => t.Name.EndsWith("ServiceClient"))
                .Where(t => !t.FullName.Contains(".V1Alpha."))
                .All(t => rootNamespace.Add(t));

            if (!processed)
            {
                throw new Exception("not all services are processed");
            }

            var codeUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace("Yandex.Cloud.Generated");
            rootNamespace.Generate(codeNamespace);
            codeUnit.Namespaces.Add(codeNamespace);
            
            new CSharpCodeProvider().GenerateCodeFromCompileUnit(
                codeUnit,
                new StreamWriter(outputFile) {AutoFlush = true},
                new CodeGeneratorOptions
                {
                    BlankLinesBetweenMembers = true,
                    BracingStyle = "C",
                    ElseOnClosing = false,
                    IndentString = "    "
                }
            );
        }
    }
}