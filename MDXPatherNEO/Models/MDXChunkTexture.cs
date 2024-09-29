using System.Text;

namespace MDXPatherNEO.Models
{
    public class MDXChunkTexture
    {
        /// <summary>
        /// 'TEXS' 태그
        /// </summary>
        public static readonly int Tag = 0x53584554;

        public static MDXChunkTexture Empty => new([]);

        public List<MDXTexture> Textures { get; private set; }

        private MDXChunkTexture(List<MDXTexture> textures)
        {
            Textures = textures;
        }

        public static bool TryParse(MDXChunk chunk, out MDXChunkTexture? texture)
        {
            texture = null;

            try
            {
                texture = Parse(chunk);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static MDXChunkTexture Parse(MDXChunk chunk)
        {
            // 데이터 크기가 268 바이트 단위가 아닐 경우 지원하지 않음
            if (chunk.Bytes.Length % 268 != 0)
                throw new FormatException("이 프로그램은 268 바이트 단위로 정렬되지 않은 텍스처 청크를 가진 MDX 모델을 지원하지 않습니다.");

            int expectedTexturesCount = chunk.Bytes.Length / 268;
            List<MDXTexture> textures = [];

            // 예상 텍스처 개수만큼 읽기 반복
            for (int i = 0; i < expectedTexturesCount; i++)
            {
                // 각 텍스처 데이터를 268 바이트 단위로 잘라서 MDXChunkTexture 인스턴스 생성
                textures.Add(new MDXTexture()
                {
                    ReplaceableId = BitConverter.ToUInt32(chunk.Bytes, i * 268),
                    FileName = Encoding.UTF8.GetString(chunk.Bytes, i * 268 + 4, 260).TrimEnd('\0'),
                    Flags = BitConverter.ToUInt32(chunk.Bytes, i * 268 + 264)
                });
            }

            return new MDXChunkTexture(textures);
        }

        public MDXChunk ToChunk()
        {
            return new MDXChunk(Tag, ToBytes());
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[Textures.Count * 268];
            for (int i = 0; i < Textures.Count; i++)
            {
                BitConverter.GetBytes(Textures[i].ReplaceableId).CopyTo(bytes, i * 268);

                // 새로운 코드 (바이트 오버플로우 방지를 위해 259바이트로 제한)
                byte[] fileNameBytes = Encoding.UTF8.GetBytes(Textures[i].FileName);

                // 259바이트를 초과하는 경우 예외 발생
                if (fileNameBytes.Length > 259)
                {
                    throw new InvalidOperationException($"The file name '{Textures[i].FileName}' exceeds the 259-byte limit in UTF-8 encoding.");
                }

                // 파일 이름의 바이트 배열을 복사
                fileNameBytes.CopyTo(bytes, i * 268 + 4);

                BitConverter.GetBytes(Textures[i].Flags).CopyTo(bytes, i * 268 + 264);
            }
            return bytes;
        }
    }

    public class MDXTexture
    {
        public required uint ReplaceableId { get; set; }
        public required string FileName { get; set; }
        public required uint Flags { get; set; }
    }
}
