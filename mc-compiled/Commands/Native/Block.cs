namespace mc_compiled.Commands.Native;

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
        return str.ToUpper() switch
        {
            "R" or "REPLACE" or "DEFAULT" or "REMOVE" => DestroyMode.REPLACE,
            "K" or "KEEP" or "AIR" or "PRESERVE" => DestroyMode.KEEP,
            "D" or "DESTROY" or "BREAK" or "SIMULATE" => DestroyMode.DESTROY,
            _ => DestroyMode.REPLACE
        };
    }

    public string id;
    public byte data;
}