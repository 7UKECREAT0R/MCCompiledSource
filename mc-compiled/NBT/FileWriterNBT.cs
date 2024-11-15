using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mc_compiled.NBT
{
    /// <summary>
    /// Constructs and writes an NBT file.
    /// </summary>
    public class FileWriterNBT
    {
        public readonly List<NBTNode> nodes;
        public readonly string fileName;

        public FileWriterNBT(string fileName)
        {
            this.nodes = [];
            this.fileName = fileName;
        }
        public FileWriterNBT(string fileName, List<NBTNode> nodes)
        {
            this.nodes = nodes;
            this.fileName = fileName;
        }
        public FileWriterNBT(string fileName, params NBTNode[] nodes)
        {
            this.nodes = [..nodes];
            this.fileName = fileName;
        }

        /// <summary>
        /// Write the file.
        /// </summary>
        public void Write()
        {
            using FileStream stream = File.OpenWrite(this.fileName);
            using BinaryWriter writer = new BinaryWriter(stream);
            NBTNode[] queue =
            [
                new NBTCompound { name = "", values = this.nodes.ToArray() },
                new NBTEnd()
            ];
            WriteToExisting(queue, writer);
        }
        public static void WriteToExisting(NBTNode[] nodes, BinaryWriter writer)
        {
            foreach (NBTNode node in nodes)
            {
                writer.Write((byte)node.tagType);
                if(node.tagType != TAG.End)
                {
                    string name = node.name;
                    ushort length = name == null ? (ushort)0 : (ushort)name.Length;
                    writer.Write(length);
                    if (length > 0)
                    {
                        byte[] nBytes = Encoding.UTF8.GetBytes(name);
                        writer.Write(nBytes);
                    }
                    node.Write(writer);
                }
            }
        }
        public static byte[] GetBytes(NBTNode[] nodes)
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);
            NBTNode[] queue =
            [
                new NBTCompound { name = "", values = nodes.ToArray() },
                new NBTEnd()
            ];
            WriteToExisting(queue, writer);
            return stream.ToArray();
        }
    }
}
