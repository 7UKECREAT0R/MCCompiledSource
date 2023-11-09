using Newtonsoft.Json.Linq;

namespace mc_compiled.Modding
{
    /// <summary>
    /// A 2D offset.
    /// </summary>
    public struct Offset2
    {
        public int x, y;
        public Offset2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public JProperty ToProperty(string name) =>
            new JProperty(name, new JArray(new[] { x, y }));
        public JArray ToArray() =>
            new JArray(new[] { x, y });
    }
    /// <summary>
    /// A 3D offset.
    /// </summary>
    public struct Offset3
    {
        public int x, y, z;
        public Offset3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public JProperty ToProperty(string name) =>
            new JProperty(name, new JArray(new[] { x, y, z }));
        public JArray ToArray() =>
            new JArray(new[] { x, y, z });
    }
}
