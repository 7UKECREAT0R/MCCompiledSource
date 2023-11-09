namespace mc_compiled.Commands.Native
{
    public struct Block
    {
        public enum DestroyMode
        {
            REPLACE,
            KEEP,
            DESTROY
        }
        public static DestroyMode ParseDestroyMode(string str)
        {
            switch (str.ToUpper())
            {
                case "R":
                case "REPLACE":
                case "DEFAULT":
                case "REMOVE":
                    return DestroyMode.REPLACE;
                case "K":
                case "KEEP":
                case "AIR":
                case "PRESERVE":
                    return DestroyMode.KEEP;
                case "D":
                case "DESTROY":
                case "BREAK":
                case "SIMULATE":
                    return DestroyMode.DESTROY;
                default:
                    return DestroyMode.REPLACE;
            }
        }

        public string id;
        public byte data;
    }

}