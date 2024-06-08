namespace Example1
{
    public class ConstantParameter
    {
        public readonly string Name;
        public readonly Guid Guid;

        public ConstantParameter(string name, Guid guid)
        {
            Name = name;
            Guid = guid;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class GroupNameAttribute : System.Attribute
    {
        private string _name;

        public GroupNameAttribute(string name)
        {
            _name = name;
        }

        public string GetName() => _name;
    }

    public static class Writer
    {
        private const string NamespaceDeclaration = @"using System;
using SharedParametersTransfer.Models;

namespace SharedParametersTransfer {
public class SharedParameters {";

        private const string ParameterConstantClassName = nameof(ConstantParameter);
        private const string GroupAttributeName = nameof(GroupNameAttribute);

        private const string BasicNameGroup = "Group";
        private const string BasicNameParameter = "Parameter";

        private const string EndBrackets = @"}}";

        public static void CreateFile(string sharedFileParameter, List<IGrouping<string, ParsedParameter>> groups)
        {
            var outputFile = $"{Path.GetDirectoryName(sharedFileParameter)}\\Exported_{DateTime.Now:yyyyMMdd}.cs";

            var sw = new StreamWriter(outputFile);

            try
            {
                sw.Write(NamespaceDeclaration);

                var groupCount = 0;
                foreach (var group in groups)
                {
                    var parameterCount = 0;

                    sw.WriteLine($@"[{GroupAttributeName}(""{group.Key}"")]
public static class {BasicNameGroup}{groupCount}{{");

                    foreach (ParsedParameter fspParameter in group)
                    {
                        sw.WriteLine($@"/// <summary>
/// {fspParameter.Name}
/// </summary>
public static readonly {ParameterConstantClassName} {BasicNameParameter}{parameterCount} =
new {ParameterConstantClassName}(""{fspParameter.Name}"", new Guid(""{fspParameter.Guid}""));");

                        parameterCount++;
                    }

                    sw.WriteLine('}');
                    groupCount++;
                }

                sw.WriteLine(EndBrackets);
            }
            finally
            {
                sw.Close();
            }
        }

        public static void CreateFileV2(string sharedFileParameter, List<IGrouping<string, ParsedParameter>> groups)
        {
            var outputFile = $"{Path.GetDirectoryName(sharedFileParameter)}\\Exported_{DateTime.Now:yyyyMMdd}.cs";

            using (var sw = new StreamWriter(outputFile))
            {
                sw.WriteLine(NamespaceDeclaration);

                var groupCount = 0;
                foreach (var group in groups)
                {
                    var parameterCount = 0;

                    sw.WriteLine($@"[{GroupAttributeName}(""{group.Key}"")]
public static class {BasicNameGroup}{groupCount}{{");

                    foreach (ParsedParameter fspParameter in group)
                    {
                        sw.WriteLine($@"/// <summary>
/// {fspParameter.Name}
/// </summary>
public static readonly {ParameterConstantClassName} {BasicNameParameter}{parameterCount} =
new {ParameterConstantClassName}(""{fspParameter.Name}"", new Guid(""{fspParameter.Guid}""));");

                        parameterCount++;
                    }

                    sw.WriteLine('}');
                    groupCount++;
                }

                sw.WriteLine(EndBrackets);
            }
        }
    }

    public class ParsedParameter
    {
        public string Guid;
        public string Name;
        public string DataType;
        public string DataCategory;
        public string Group;
        public string Visible;
        public string Description;
        public string UserModifiable;
        public string HideWhenNoValue;
    }
}