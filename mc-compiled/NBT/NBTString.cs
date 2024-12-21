using System.IO;
using System.Text;

namespace mc_compiled.NBT;

public class NBTString : NBTNode
{
    public string value;

    public NBTString()
    {
        this.tagType = TAG.String;
    }

    public override void Write(BinaryWriter writer)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(this.value);
        ushort len = (ushort) bytes.Length;

        writer.Write(len);
        writer.Write(bytes);
    }
}