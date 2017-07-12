using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SInnovations.Azure.ResourceManager.ConsoleHelper
{
    public class ApplicationCredentialsOptions
    {
        [Option('c', "clientId",
            HelpText = "The clientId of the Azure Application Id to use")]
        public string CliendId { get; set; }

        [Option('t', "tenantId",
         HelpText = "The clientId of the Azure Application Id to use")]
        public string TenantId { get; set; }

        
        [Option("replyUrl", MutuallyExclusiveSet = "Personal",
           HelpText = "The clientId of the Azure Application Id to use")]
        public string ReplyUrl { get; set; }

        [Option("secret", MutuallyExclusiveSet = "Personal",
           HelpText = "The clientId of the Azure Application Id to use")]
        public string Secret { get; set; }

        [Option('s', "subscriptionid",
           HelpText = "The Azure Subscription Id to use")]
        public string SubscriptionId { get; set; }

        [Option('a', "AccessToken",
            HelpText = "The oauthToken to use for authorization")]
        public string AccessToken { get; set; }
    }

    public class test
    {
        [Option("hello",
             HelpText = "The Azure Subscription Id to use")]
        public string hello { get; set; }

    }

        //}
        //public class myOps
        //{
        //    public myOps()
        //    {
        //        AddVerb = new test();
        //    }
        //    [VerbOption("add", HelpText = "Add file contents to the index.")]
        //    public test AddVerb { get; set; }
        //}

        public static class MyTypeBuilder
    {
        public static Object CreateNewObject(Dictionary<string, Type> yourListOfFields)
        {
            var myType = CompileResultType(yourListOfFields);
            return Activator.CreateInstance(myType);
        }
        public static Type CompileResultType(Dictionary<string,Type> yourListOfFields)
        {
            TypeBuilder tb = GetTypeBuilder();
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            // NOTE: assuming your list contains Field objects with fields FieldName(string) and FieldType(Type)
            foreach (var field in yourListOfFields)
                CreateProperty(tb, field.Key, field.Value);

            Type objectType = tb.CreateTypeInfo();
            return objectType;
        }

        private static TypeBuilder GetTypeBuilder()
        {
            var typeSignature = "MyDynamicType";
            var an = new AssemblyName(typeSignature);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature
                                , TypeAttributes.Public |
                                TypeAttributes.Class |
                                TypeAttributes.AutoClass |
                                TypeAttributes.AnsiClass |
                                TypeAttributes.BeforeFieldInit |
                                TypeAttributes.AutoLayout
                                , null);
            return tb;
        }

        private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            var ci = typeof(VerbOptionAttribute).GetConstructor(new Type[] { typeof(string)});
            var builder = new CustomAttributeBuilder(ci, new object[] { propertyName });
            propertyBuilder.SetCustomAttribute(builder);

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }
    }

    public static class ConsoleReaderHelper
    {
        public static ApplicationCredentialsOptions ReadFromConsole(this ApplicationCredentials cred, string[] args)
        {
            return cred.ReadFromConsole<ApplicationCredentialsOptions>(args);
            
        }
        public static T ReadFromConsole<T>(this ApplicationCredentials options, string[] arguments) where T : ApplicationCredentialsOptions,new()
        {
            var consoleOps = new T();
            var b = new CommandLine.Parser((s)=>
            {
                s.IgnoreUnknownArguments = true; 
                
            });
            
            if(!b.ParseArguments(arguments, consoleOps))
            {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }


            options.TenantId = consoleOps.TenantId;
            options.SubscriptionId = consoleOps.SubscriptionId;
            options.ReplyUrl = consoleOps.ReplyUrl;
            options.Secret = consoleOps.Secret;
            options.CliendId = consoleOps.CliendId;
            options.AccessToken = consoleOps.AccessToken;

            return consoleOps;
        }

        public static Tuple<string,object> ReadFromConsole(this ApplicationCredentials options, Dictionary<string,Type> verbs,  string[] arguments) 
        {
            options.ReadFromConsole(arguments);

            var verbsOptions = MyTypeBuilder.CreateNewObject(verbs);
           

            string invokedVerb ="";
            object invokedVerbInstance;

            var b = new CommandLine.Parser((s) =>
            {
                s.IgnoreUnknownArguments = true;

            });
            if (!b.ParseArguments(arguments, verbsOptions,
              (verb, subOptions) =>
              {
                  // if parsing succeeds the verb name and correct instance
                  // will be passed to onVerbCommand delegate (string,object)
                  invokedVerb = verb;
                  invokedVerbInstance = subOptions;
              }))
            {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }

            return new Tuple<string, object>(invokedVerb, verbsOptions.GetType().GetProperty(invokedVerb).GetValue(verbsOptions));

        }
    }
}
