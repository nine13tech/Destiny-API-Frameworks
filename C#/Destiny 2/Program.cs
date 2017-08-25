using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Destiny2ApiGenerator
{
    class Program
    {
        static void Main(string[] args)
        {


            DeserializedOpenapi api = JsonConvert.DeserializeObject<DeserializedOpenapi>(File.ReadAllText("apiv3.json"));
            List<Tag> tags = api.tags.ToObject(typeof(List<Tag>));
            List<Scope> scopes = new List<Scope>();
            List<NewEnum> enums = new List<NewEnum>();
            foreach (JProperty scope in api.Components.securitySchemes.oauth2.flows.authorizationCode.scopes)
            {
                scopes.Add(new Scope()
                {
                    Name = scope.Name,
                    Description = (string)scope.Value,
                });
            }

            foreach (var definition in api.Components.schemas)
            {
                foreach (var property in definition)
                {
                    foreach (var value in property)
                    {
                        if (value.Name == "x-enum-values")
                        {
                            NewEnum _newenum = new NewEnum();
                            _newenum.EnumValues = new Dictionary<int, string>();
                            foreach (var valueentries in value)
                            {
                                foreach (var enumvalue in valueentries)
                                {
                                    _newenum.Name = (string)definition.Name;
                                    int Num = (int)enumvalue.numericValue;
                                    string Id = (string)enumvalue.identifier;
                                    _newenum.EnumValues.Add(Num,Id);
                                }
                            }
                            enums.Add(_newenum);

                        }
                    }
                }
            }
            foreach(var enuminfo in enums)
            {
                string ns = "OutputFromGenerator";
                string _code = $"namespace {ns}\r\n{{\r\n\t";
                _code = _code + $"enum {enuminfo.Name.Replace(".", string.Empty)} {{\r\n";
                foreach (var enumentry in enuminfo.EnumValues)
                {
                    _code = _code + $"\t\t{enumentry.Value} = {enumentry.Key},\r\n";
                }
                _code = _code + "\t}\r\n";
                _code = _code + "}";
                File.WriteAllText($"enums/{enuminfo.Name.Replace(".", string.Empty)}.cs", _code);
            }
            Console.WriteLine("Reached end of program.");
            Console.ReadLine();
        }
    }
}
