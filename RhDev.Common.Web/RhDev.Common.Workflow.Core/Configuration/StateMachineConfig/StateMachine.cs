using RhDev.Common.Web.Core.Utils;
using RhDev.Common.Workflow.Builder;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RhDev.Common.Workflow.Configuration.StateMachineConfig
{
    [Serializable]
    public class StateMachine : IWorkflowPartBuilder
    {
        static JsonSerializerOptions serializationOptions = new JsonSerializerOptions
        {
            Converters ={
                    new JsonStringEnumConverter()
                }
        };
        public string Version { get; set; } = DateTime.Now.ToString("MM-dd-yy:hh-mm-ss");

        public string VersionComment { get; set; }

        public List<ConfiguredState> UserTransitions { get; set; } = new List<ConfiguredState> { };

        public List<StateDefinition.StateDefinition> StateDefinitions { get; set; } = new List<StateDefinition.StateDefinition> { };

        public List<GenericTransition> GenericTransitions { get; set; } = new List<GenericTransition> { };

        public List<ConfiguredState> SystemTransitions { get; set; } = new List<ConfiguredState> { };
        public static StateMachine Empty => new StateMachine();

        public WorkflowDefinitionFile GetFile(string name)
        {
            var machineString = JsonSerializer.Serialize(this);
            var machineData = JsonSerializer.SerializeToUtf8Bytes(this, new JsonSerializerOptions {
                Converters ={
                    new JsonStringEnumConverter()
                }
            });

            return new WorkflowDefinitionFile
            {
                Name = name,
                Data = machineData,
                DataString = machineString,
                Version = Version
            };
        }

        public static T Get<T>(string artefact)
        {
            Guard.StringNotNullOrWhiteSpace(artefact);

            return JsonSerializer.Deserialize<T>(artefact, serializationOptions);
        }

        public static StateMachine GetMachine(byte[] file)
        {
            var encoding = DetectEncoding(file);

            ReadOnlySpan<byte> utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
            ReadOnlySpan<byte> fileToRead = file;
            if (fileToRead.StartsWith(utf8Bom))
            {
                fileToRead = fileToRead.Slice(utf8Bom.Length);
            }

            var dataString = encoding.GetString(fileToRead);
                       
            
            var machine = JsonSerializer.Deserialize<StateMachine>(dataString, serializationOptions);

            return machine;
        }

        static Encoding DetectEncoding(byte[] file)
        {
            // Read the BOM
            var bom = new byte[4] { file[0], file[1], file[2], file[3] };

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.Default;
        }
    }
}
