using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace Xnlab.Filio
{
    #region Serialization
    //http://codebetter.com/blogs/gregyoung/archive/2008/08/24/fast-serialization.aspx
    internal class CustomBinaryFormatter : IFormatter
    {
        private SerializationBinder m_Binder;
        private StreamingContext m_StreamingContext;
        private ISurrogateSelector m_SurrogateSelector;
        private readonly MemoryStream m_WriteStream;
        private MemoryStream m_IndexWriteStream;
        private readonly MemoryStream m_ReadStream;
        private readonly BinaryWriter m_Writer;
        private readonly BinaryReader m_Reader;
        private readonly Dictionary<Type, int> m_ByType = new Dictionary<Type, int>();
        private readonly Dictionary<int, Type> m_ById = new Dictionary<int, Type>();
        private const int sizeLength = 8;
        private readonly byte[] m_LengthBuffer = new byte[sizeLength];
        private readonly byte[] m_CopyBuffer;
        private Stream indexStream = null;
        private Stream serializationStream = null;
        private long count = 0;
        private long pending = 0;
        private const int chunckSize = 5000;

        public CustomBinaryFormatter(Stream indexStream)
            : this(indexStream, null)
        {
        }

        ~CustomBinaryFormatter()
        {
            Close();
        }

        public Stream DataStream
        {
            get { return serializationStream; }
        }

        public Stream IndexStream
        {
            get { return indexStream; }
        }

        public CustomBinaryFormatter(Stream indexStream, Stream serializationStream)
        {
            m_CopyBuffer = new byte[sizeLength * 1000000];
            m_WriteStream = new MemoryStream(100000);
            m_ReadStream = new MemoryStream(100000);
            m_Writer = new BinaryWriter(m_WriteStream);
            m_Reader = new BinaryReader(m_ReadStream);
            m_IndexWriteStream = new MemoryStream(chunckSize * sizeLength);
            this.indexStream = indexStream;
            this.serializationStream = serializationStream;
            if (indexStream != null)
            {
                bool indexReady;
                if (indexStream.Length >= sizeLength)
                    if (indexStream.Read(m_LengthBuffer, 0, sizeLength) == sizeLength)
                    {
                        count = BitConverter.ToInt64(m_LengthBuffer, 0);
                        indexReady = true;
                    }
                    else
                        indexReady = false;
                else
                    indexReady = false;
                if (!indexReady)
                {
                    indexStream.Position = 0;
                    indexStream.Write(BitConverter.GetBytes(0L), 0, sizeLength);
                }
                indexStream.Seek(indexStream.Length, SeekOrigin.Begin);
            }
        }

        public void Flush()
        {
            if (indexStream != null)
            {
                if (pending % chunckSize != 0)
                {
                    byte[] buffer = m_IndexWriteStream.ToArray();
                    indexStream.Write(buffer, 0, buffer.Length);
                    m_IndexWriteStream = new MemoryStream(chunckSize * sizeLength);
                    pending = 0;
                }
                indexStream.Position = 0;
                indexStream.Write(BitConverter.GetBytes(count), 0, sizeLength);
            }
        }

        public void Close()
        {
            if (indexStream != null)
                indexStream.Close();
            if (serializationStream != null)
                serializationStream.Close();
        }

        public void Register<T>(int _TypeId) where T : ICustomBinarySerializable
        {
            m_ById.Add(_TypeId, typeof(T));
            m_ByType.Add(typeof(T), _TypeId);
        }

        public void MoveTo(long Index)
        {
            MoveTo(Index, true);
        }

        public void MoveTo(long Index, bool Relocate)
        {
            if (indexStream != null && serializationStream != null)
            {
                if (Index >= 0 && Index * sizeLength <= (indexStream.Length - sizeLength))
                {
                    long pos = indexStream.Position;
                    indexStream.Position = sizeLength + Index * sizeLength;
                    if (indexStream.Read(m_LengthBuffer, 0, sizeLength) == sizeLength)
                    {
                        serializationStream.Seek(BitConverter.ToInt64(m_LengthBuffer, 0), SeekOrigin.Begin);
                    }
                    if (Relocate)
                        indexStream.Position = pos;
                }
            }
        }

        public void MoveToEnd()
        {
            indexStream.Seek(0, SeekOrigin.End);
            serializationStream.Seek(0, SeekOrigin.End);
        }

        public long Count
        {
            get { return count; }
        }

        public object Deserialize(Stream serializationStream)
        {
            return null;
        }

        public T Deserialize<T>(bool Full)
        {
            if (serializationStream.Read(m_LengthBuffer, 0, sizeLength) != sizeLength)
                //throw new SerializationException("Could not read length from the stream.");
                return default(T);
            int length = BitConverter.ToInt32(m_LengthBuffer, 0);
            //TODO make this support partial reads from stream
            if (serializationStream.Read(m_CopyBuffer, 0, length) != length)
                throw new SerializationException("Could not read " + length + " bytes from the stream.");
            m_ReadStream.Seek(0L, SeekOrigin.Begin);
            m_ReadStream.Write(m_CopyBuffer, 0, length);
            m_ReadStream.Seek(0L, SeekOrigin.Begin);
            int typeid = m_Reader.ReadInt32();
            Type t;
            if (!m_ById.TryGetValue(typeid, out t))
                throw new SerializationException("TypeId " + typeid + " is not a registerred type id");
            object obj = FormatterServices.GetUninitializedObject(t);
            ICustomBinarySerializable deserialize = (ICustomBinarySerializable)obj;
            deserialize.SetDataFrom(m_Reader, Full);
            if (m_ReadStream.Position != length)
                throw new SerializationException("object of type " + t + " did not read its entire buffer during deserialization. This is most likely an inbalance between the writes and the reads of the object.");
            return (T)deserialize;
        }

        public void Serialize(Stream serializationStream, object graph)
        {

        }

        public void Serialize<T>(T graph)
        {
            Serialize<T>(graph, false);
        }

        public void Serialize<T>(T graph, bool IsUpdate)
        {
            int key;
            if (!m_ByType.TryGetValue(graph.GetType(), out key))
                throw new SerializationException(graph.GetType() + " has not been registered with the serializer");
            ICustomBinarySerializable c = (ICustomBinarySerializable)graph; //this will always work due to generic constraint on the Register
            m_WriteStream.Seek(0L, SeekOrigin.Begin);
            m_Writer.Write((int)key);
            c.WriteDataTo(m_Writer);
            if (indexStream != null && !IsUpdate)
            {
                count++;
                pending++;
                if (pending % chunckSize == 0)
                {
                    byte[] buffer = m_IndexWriteStream.ToArray();
                    indexStream.Write(buffer, 0, buffer.Length);
                    m_IndexWriteStream = new MemoryStream(chunckSize * sizeLength);
                    pending = 0;
                }
                m_IndexWriteStream.Write(BitConverter.GetBytes(serializationStream.Position), 0, sizeLength);
            }
            serializationStream.Write(BitConverter.GetBytes(m_WriteStream.Position), 0, sizeLength);
            serializationStream.Write(m_WriteStream.GetBuffer(), 0, (int)m_WriteStream.Position);
        }

        public ISurrogateSelector SurrogateSelector
        {
            get { return m_SurrogateSelector; }
            set { m_SurrogateSelector = value; }
        }

        public SerializationBinder Binder
        {
            get { return m_Binder; }
            set { m_Binder = value; }
        }

        public StreamingContext Context
        {
            get { return m_StreamingContext; }
            set { m_StreamingContext = value; }
        }
    }

    public interface ICustomBinarySerializable
    {
        void WriteDataTo(BinaryWriter _Writer);
        void SetDataFrom(BinaryReader _Reader, bool Full);
    }
    #endregion
}
