using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            nodes = new List<NBTNode>();
            this.fileName = fileName;
        }
        public FileWriterNBT(string fileName, List<NBTNode> nodes)
        {
            this.nodes = nodes;
            this.fileName = fileName;
        }
        public FileWriterNBT(string fileName, params NBTNode[] nodes)
        {
            this.nodes = new List<NBTNode>(nodes);
            this.fileName = fileName;
        }

        /// <summary>
        /// Write the file.
        /// </summary>
        public void Write()
        {
            using (FileStream stream = File.OpenWrite(fileName))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                NBTNode[] queue = new NBTNode[]
                {
                    new NBTCompound() { name = "", values = nodes.ToArray() },
                    new NBTEnd()
                };
                WriteToExisting(queue, writer);
            }
        }
        public static void WriteToExisting(NBTNode[] nodes, BinaryWriter writer)
        {
            foreach (NBTNode node in nodes)
            {
                writer.Write((byte)node.tagType);
                if(node.tagType != TAG.End)
                {
                    string name = node.name;
                    ushort length = (ushort)name.Length;
                    byte[] nBytes = Encoding.UTF8.GetBytes(name);
                    writer.Write(length);
                    writer.Write(nBytes);
                    node.Write(writer);
                }
            }
        }

        /*public static FileWriterNBT ConstructFloatingItem()
        {
            NBTNode[] nodes = new NBTNode[]
            {

            };
        }*/
    }
}
