using MDXPatherNEO.Models;
using NonWPF.Forms;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MDXPatherNEO
{
    /// <summary>
    /// ElementModel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ElementModel : UserControl
    {
        private readonly MDXFile _modelFile;
        private readonly MDXChunkTexture _chunkTexture = MDXChunkTexture.Empty;
        private bool _changed = false;

        public ElementModel(MDXFile modelFile)
        {
            InitializeComponent();
            _modelFile = modelFile;
            FilePathLabel.Text = $"{Path.GetFileName(modelFile.Path)} ({Path.GetDirectoryName(modelFile.Path)})";

            // 모델 파일 내 MDX 데이터의 모든 청크를 순회하며 'TEXS' 태그를 가진 청크만 필터링
            var chunks = modelFile.Data.Chunks.Where(chunk => chunk.Tag == MDXChunkTexture.Tag).ToList();

            // 이 프로그램은 2개 이상의 'TEXS' 청크를 가진 MDX 모델을 지원하지 않음
            if (chunks.Count > 1)
            {
                TexturePlaceholer.Text = "이 프로그램은 2개 이상의 'TEXS' 청크를 가진 MDX 모델을 지원하지 않습니다.";
                return;
            }

            // 'TEXS' 청크가 없는 경우 아무것도 표시하지 않음
            if (chunks.Count == 0) return;

            // 해당 청크를 MDXChunkTexture 로 파싱 시도
            if (!MDXChunkTexture.TryParse(chunks.First(), out var textureChunk) || textureChunk is null)
            {
                TexturePlaceholer.Text = "( 알 수 없는 텍스처 청크 ) ";
                return;
            }

            // 만약 텍스처가 없는 경우 아무것도 표시하지 않음
            if (textureChunk.Textures.Count == 0) return;

            // 텍스처가 있는 경우, 텍스처 정보를 표시
            _chunkTexture = textureChunk;
            TexturePlaceholer.Visibility = Visibility.Collapsed;

            textureChunk.Textures.ForEach(mdxTexture =>
            {
                ElementTexture elementTexture = new()
                {
                    TexturePath = (mdxTexture.ReplaceableId == 0) ? mdxTexture.FileName : $"Replaceable ID {mdxTexture.ReplaceableId}",
                    TextureFlag = $"{mdxTexture.Flags}",
                };

                elementTexture.TexturePathChanged += (_, _) =>
                {
                    var changedTexturePathString = elementTexture.TexturePath;

                    // 수정된 텍스처 컨트롤의 인덱스와 일치하는 텍스처 정보를 업데이트
                    var textureIndex = TextureContainer.Children.IndexOf(elementTexture);

                    // Replaceable ID 텍스처인 경우, Replaceable ID 값만 추출
                    if (changedTexturePathString.StartsWith("Replaceable ID ") && int.TryParse(changedTexturePathString.AsSpan(15), out var replaceableId) && replaceableId > 0)
                    {
                        _chunkTexture.Textures[textureIndex].FileName = string.Empty;
                        _chunkTexture.Textures[textureIndex].ReplaceableId = (uint)replaceableId;
                    }
                    else
                    {
                        _chunkTexture.Textures[textureIndex].FileName = changedTexturePathString;
                        _chunkTexture.Textures[textureIndex].ReplaceableId = 0;
                    }

                    // 변경 사항 표시
                    elementTexture.Foreground = _changedForegroundBrush;
                    MarkAsChanged();
                };

                TextureContainer.Children.Add(elementTexture);
            });
        }

        public bool Closable => !_changed;

        public bool Save()
        {
            // 저장할 텍스처가 없다면 저장하지 않음
            if (_chunkTexture.Textures.Count == 0) return true;

            try
            {
                // 파일에서 모든 기존 'TEXS' 청크를 제거
                _modelFile.Data.Chunks.RemoveAll(chunk => chunk.Tag == MDXChunkTexture.Tag);

                // 새로운 'TEXS' 청크를 추가
                _modelFile.Data.Chunks.Add(_chunkTexture.ToChunk());

                // 파일에 쓰기
                _modelFile.Save();
            }
            catch
            {
                return false;
            }

            // 변경 사항 표시 원상복구
            MarkAsSaved();
            return true;
        }

        public void Close(bool force = false)
        {
            if (!force && _changed)
            {
                var result = MessageBox.Show(Window.GetWindow(this), "저장되지 않은 변경 사항이 있습니다. 저장하시겠습니까?", "파일 닫기", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel) return;
                if (result == MessageBoxResult.Yes)
                {
                    // 저장하되, 저장에 실패할 경우, 이후 예약된 닫기 동작을 수행하지 않습니다.
                    if (!Save()) return;
                }
            }

            // remove this control from parent
            (Parent as StackPanel)?.Children.Remove(this);
        }

        private static readonly SolidColorBrush _changedForegroundBrush = new(Color.FromRgb(255, 55, 255));

        private void MarkAsChanged()
        {
            FileIconLabel.Foreground = _changedForegroundBrush;
            FilePathLabel.Foreground = _changedForegroundBrush;
            _changed = true;
        }

        private void MarkAsSaved()
        {
            // 하위 컨트롤의 변경 사항 표시 원상복구
            foreach (ElementTexture elementTexture in TextureContainer.Children)
            {
                elementTexture.ClearValue(ForegroundProperty);
            }

            // 변경 사항 표시 원상복구
            FileIconLabel.ClearValue(ForegroundProperty);
            FilePathLabel.ClearValue(ForegroundProperty);
            _changed = false;
        }

        private void FileSave_Click(object sender, RoutedEventArgs e) => Save();

        private void FileBrowse_Click(object sender, RoutedEventArgs e)
        {
            var args = $"/select, \"{_modelFile.Path}\"";

            ProcessUtils.StartProcess("explorer.exe", args);
        }

        private void FileClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
