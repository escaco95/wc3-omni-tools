using System.IO;

namespace MDXPatherNEO.Models
{
    public class MDXData
    {
        public static readonly int Tag = 0x584c444d;

        public List<MDXChunk> Chunks { get; private set; }

        private MDXData(List<MDXChunk> chunks)
        {
            Chunks = chunks;
        }

        public static MDXData Load(BinaryReader reader)
        {
            // 1. 헤더 검증
            ValidateHeader(reader);
            var chunks = ReadChunksUntilFileEnds(reader);
            return new MDXData(chunks);
        }

        private static void ValidateHeader(BinaryReader reader)
        {
            // 헤더 데이터가 'MDLX' 인지 확인
            if (reader.ReadInt32() != Tag)
            {
                throw new InvalidDataException("MDX 파일이 아니거나 파일이 손상되었습니다.");
            }
        }

        private static List<MDXChunk> ReadChunksUntilFileEnds(BinaryReader reader)
        {
            // 파일의 끝을 만날 때까지 청크를 읽어들임
            List<MDXChunk> chunks = [];
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                chunks.Add(MDXChunk.Load(reader));
            }
            return chunks;
        }

        public void Save(BinaryWriter writer)
        {
            // 헤더 기록
            writer.Write(Tag);
            // 각 청크 데이터 쓰기
            Chunks.ForEach(chunk => chunk.Save(writer));
        }
    }
}
