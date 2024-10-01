using System.IO;
using System.Text.Json;

namespace WC3OmniTool.Models
{
    public class OmniToolConfigScanner
    {
        // JSON 직렬화 옵션 (보기 좋은 형식으로 출력)
        private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

        public OmniToolConfig[] ToolConfigs { get; private set; }

        public Exception? Error { get; private set; }

        public bool IsFailure => Error != null;

        public bool IsEmpty => ToolConfigs.Length == 0;

        private OmniToolConfigScanner(OmniToolConfig[] toolConfigs, Exception? error)
        {
            ToolConfigs = toolConfigs;
            Error = error;
        }

        public static OmniToolConfigScanner Of(Exception ex)
        {
            return new OmniToolConfigScanner([], ex);
        }

        public static OmniToolConfigScanner Of(OmniToolConfig[] omniConfigs)
        {
            return new OmniToolConfigScanner(omniConfigs, null);
        }

        public static OmniToolConfigScanner Scan(string scanRootDirectory, string scanTargetFileName)
        {
            try
            {
                // 도구 디렉토리 존재 보장
                Directory.CreateDirectory(scanRootDirectory);

                // 도구 디렉토리 내의 모든 디렉토리를 스캔
                var toolDirectories = Directory.GetDirectories(scanRootDirectory);

                // 각 디렉토리에서 도구 구성 파일 스캔
                List<OmniToolConfig> tools = [];
                foreach (var toolDirectory in toolDirectories)
                {
                    try
                    {
                        var omniConfigPath = Path.Combine(toolDirectory, scanTargetFileName);

                        if (!File.Exists(omniConfigPath)) continue;

                        var jsonContent = File.ReadAllText(omniConfigPath);
                        var configData = JsonSerializer.Deserialize<OmniToolConfig>(jsonContent, JsonSerializerOptions);

                        if (configData is null) continue;

                        configData.Executable = Path.Combine(toolDirectory, configData.Executable);

                        tools.Add(configData);
                    }
                    catch
                    {
                        continue;
                    }
                }

                return Of([.. tools]);
            }
            catch (Exception ex)
            {
                return Of(ex);
            }
        }
    }
}
