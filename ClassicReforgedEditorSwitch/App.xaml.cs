using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;

namespace ClassicReforgedEditorSwitch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Rect? ApplicationBounds { get; private set; } = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            // 전달된 인수를 `e.Args`로 가져옵니다.
            string[] args = e.Args;

            // 인수를 파싱하여 필요한 값 추출
            string? bounds = args.FirstOrDefault(a => a.StartsWith("-omni.bounds="))?.Split('=')[1];

            // 만약 파싱한 값이 있고 {x},{y},{width},{height} 형식이라면
            if (bounds is not null && OmniBoundsRegex().IsMatch(bounds))
            {
                // 각 값을 추출하여 Rect 구조체로 변환
                string[] boundsSplit = bounds.Split(',');
                Rect rect = new(double.Parse(boundsSplit[0]), double.Parse(boundsSplit[1]), double.Parse(boundsSplit[2]), double.Parse(boundsSplit[3]));
                ApplicationBounds = rect;
            }

            base.OnStartup(e);
        }

        [GeneratedRegex(@"[0-9]+,[0-9]+,[0-9]+,[0-9]+")]
        private static partial Regex OmniBoundsRegex();
    }

}
