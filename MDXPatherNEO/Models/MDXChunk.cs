using System.IO;

namespace MDXPatherNEO.Models
{
    public class MDXChunk(int tag, byte[] bytes)
    {
        public int Tag { get; private set; } = tag;

        public byte[] Bytes { get; private set; } = bytes;

        public static MDXChunk Load(BinaryReader reader)
        {
            var tag = reader.ReadInt32();
            var size = reader.ReadUInt32();
            var bytes = ReadBytes(reader, size); // BinaryReader.ReadBytes()는 uint32를 인자로 받지 못하므로, 별도로 구현된 ReadBytes 호출

            return new MDXChunk(tag, bytes);
        }

        private static byte[] ReadBytes(BinaryReader reader, uint size)
        {
            // C# 배열.Length는 int32를 반환하므로, size가 int32 크기를 초과하는 경우 예외를 던짐
            if (size > int.MaxValue)
            {
                throw new InvalidDataException("이 프로그램은 int32 크기를 초과하는 청크를 가진 거대 MDX 모델의 수정을 지원하지 않습니다.");
            }

            return reader.ReadBytes((int)size);
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Tag);
            writer.Write((uint)Bytes.Length);
            writer.Write(Bytes);
        }
    }
}
